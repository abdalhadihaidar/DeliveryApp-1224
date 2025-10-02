using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Stripe payment service implementation
    /// </summary>
    public class StripePaymentService : ApplicationService, IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly IRepository<Order, Guid> _orderRepository;
        private readonly IRepository<Restaurant, Guid> _restaurantRepository;
        private readonly IRepository<PaymentTransaction, int> _paymentTransactionRepository;
        private readonly string _stripeSecretKey;
        private readonly string _stripeWebhookSecret;

        public StripePaymentService(
            IConfiguration configuration,
            ILogger<StripePaymentService> logger,
            IRepository<Order, Guid> orderRepository,
            IRepository<Restaurant, Guid> restaurantRepository,
            IRepository<PaymentTransaction, int> paymentTransactionRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _orderRepository = orderRepository;
            _restaurantRepository = restaurantRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            
            _stripeSecretKey = _configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe secret key not configured");
            _stripeWebhookSecret = _configuration["Stripe:WebhookSecret"] ?? throw new InvalidOperationException("Stripe webhook secret not configured");
            
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntentAsync(CreatePaymentIntentRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating payment intent for order {OrderId} with amount {Amount}", request.OrderId, request.Amount);

                var order = await _orderRepository.GetAsync(request.OrderId);
                if (order == null)
                {
                    throw new InvalidOperationException($"Order {request.OrderId} not found");
                }

                // Calculate fees
                var feeCalculation = await CalculateFeesAsync(request.Amount, order.RestaurantId);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convert to cents
                    Currency = request.Currency.ToLower(),
                    Description = request.Description ?? $"Payment for Order #{request.OrderId}",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = request.AutomaticPaymentMethods,
                    },
                    CaptureMethod = request.CaptureMethod ? "automatic" : "manual",
                    Metadata = new Dictionary<string, string>
                    {
                        ["order_id"] = request.OrderId.ToString(),
                        ["platform_fee"] = feeCalculation.PlatformFee.ToString("F2"),
                        ["restaurant_commission"] = feeCalculation.RestaurantCommission.ToString("F2")
                    }
                };

                if (!string.IsNullOrEmpty(request.CustomerId))
                {
                    options.Customer = request.CustomerId;
                }

                if (!string.IsNullOrEmpty(request.PaymentMethodId))
                {
                    options.PaymentMethod = request.PaymentMethodId;
                    options.ConfirmationMethod = "manual";
                    options.Confirm = false;
                }

                // Add application fee for connected accounts
                if (!string.IsNullOrEmpty(request.ConnectedAccountId) && feeCalculation.PlatformFee > 0)
                {
                    options.ApplicationFeeAmount = (long)(feeCalculation.PlatformFee * 100);
                    options.TransferData = new PaymentIntentTransferDataOptions
                    {
                        Destination = request.ConnectedAccountId,
                    };
                }

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                // Save payment transaction record
                var transaction = new PaymentTransaction
                {
                    OrderId = request.OrderId,
                    PaymentIntentId = paymentIntent.Id,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Status = paymentIntent.Status.ToLower() switch
                    {
                        "succeeded" => TransactionStatus.Succeeded,
                        "processing" => TransactionStatus.Processing,
                        "failed" => TransactionStatus.Failed,
                        "canceled" => TransactionStatus.Cancelled,
                        _ => TransactionStatus.Pending
                    },
                    PaymentProvider = "Stripe",
                    PlatformFee = feeCalculation.PlatformFee,
                    RestaurantCommission = feeCalculation.RestaurantCommission,
                    CreatedAt = DateTime.UtcNow
                };

                await _paymentTransactionRepository.InsertAsync(transaction);

                _logger.LogInformation("Payment intent created successfully: {PaymentIntentId}", paymentIntent.Id);

                return new CreatePaymentIntentResponseDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    CreatedAt = DateTime.UtcNow,
                    RequiresAction = paymentIntent.Status == "requires_action",
                    NextActionType = paymentIntent.NextAction?.Type
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent: {Error}", ex.Message);
                throw new InvalidOperationException($"Payment processing error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent for order {OrderId}", request.OrderId);
                throw;
            }
        }

        public async Task<ConfirmPaymentResponseDto> ConfirmPaymentAsync(string paymentIntentId, string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Confirming payment intent {PaymentIntentId}", paymentIntentId);

                var service = new PaymentIntentService();
                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId,
                    ReturnUrl = _configuration["App:BaseUrl"] + "/payment/return"
                };

                var paymentIntent = await service.ConfirmAsync(paymentIntentId, options);

                // Update transaction status
                var transaction = await _paymentTransactionRepository.FirstOrDefaultAsync(t => t.PaymentIntentId == paymentIntentId);
                if (transaction != null)
                {
                    transaction.Status = paymentIntent.Status.ToLower() switch
                    {
                        "succeeded" => TransactionStatus.Succeeded,
                        "processing" => TransactionStatus.Processing,
                        "failed" => TransactionStatus.Failed,
                        "canceled" => TransactionStatus.Cancelled,
                        _ => TransactionStatus.Pending
                    };
                    transaction.PaymentMethodId = paymentMethodId;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    await _paymentTransactionRepository.UpdateAsync(transaction);
                }

                _logger.LogInformation("Payment intent confirmed: {PaymentIntentId}, Status: {Status}", paymentIntentId, paymentIntent.Status);

                return new ConfirmPaymentResponseDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    Status = paymentIntent.Status,
                    Success = paymentIntent.Status == "succeeded",
                    RequiresAction = paymentIntent.Status == "requires_action",
                    NextActionType = paymentIntent.NextAction?.Type,
                    ClientSecret = paymentIntent.ClientSecret
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error confirming payment: {Error}", ex.Message);
                return new ConfirmPaymentResponseDto
                {
                    PaymentIntentId = paymentIntentId,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment intent {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        public async Task<PaymentIntentDetailsDto> GetPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                var result = new PaymentIntentDetailsDto
                {
                    Id = paymentIntent.Id,
                    Amount = paymentIntent.Amount / 100m,
                    Currency = paymentIntent.Currency.ToUpper(),
                    Status = paymentIntent.Status,
                    Description = paymentIntent.Description,
                    CreatedAt = paymentIntent.Created,
                    CustomerId = paymentIntent.CustomerId,
                    PaymentMethodId = paymentIntent.PaymentMethodId,
                    ReceiptEmail = paymentIntent.ReceiptEmail,
                    ApplicationFeeAmount = paymentIntent.ApplicationFeeAmount / 100m
                };

                if (paymentIntent.LatestCharge != null)
                {
                    var charge = paymentIntent.LatestCharge;
                    result.LatestCharge = new PaymentChargeDto
                    {
                        Id = charge.Id,
                        Amount = charge.Amount / 100m,
                        Currency = charge.Currency.ToUpper(),
                        Paid = charge.Paid,
                        Status = charge.Status,
                        CreatedAt = charge.Created,
                        ReceiptUrl = charge.ReceiptUrl
                    };

                    if (charge.PaymentMethodDetails?.Card != null)
                    {
                        var card = charge.PaymentMethodDetails.Card;
                        result.LatestCharge.PaymentMethodDetails = new PaymentMethodDetailsDto
                        {
                            Type = "card",
                            Card = new CardDetailsDto
                            {
                                Brand = card.Brand,
                                Last4 = card.Last4,
                                ExpMonth = (int)card.ExpMonth,
                                ExpYear = (int)card.ExpYear,
                                Country = card.Country,
                                Funding = card.Funding
                            }
                        };
                    }
                }

                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error retrieving payment intent: {Error}", ex.Message);
                throw new InvalidOperationException($"Error retrieving payment: {ex.Message}", ex);
            }
        }

        public async Task<RefundResponseDto> ProcessRefundAsync(ProcessRefundRequestDto request)
        {
            try
            {
                _logger.LogInformation("Processing refund for payment intent {PaymentIntentId}", request.PaymentIntentId);

                // Get the payment intent to find the charge
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(request.PaymentIntentId);

                if (paymentIntent.LatestChargeId == null)
                {
                    throw new InvalidOperationException("No charge found for this payment intent");
                }

                var options = new RefundCreateOptions
                {
                    Charge = paymentIntent.LatestChargeId,
                    Reason = request.Reason switch
                    {
                        "duplicate" => "duplicate",
                        "fraudulent" => "fraudulent",
                        "requested_by_customer" => "requested_by_customer",
                        _ => "requested_by_customer"
                    }
                };

                if (request.Amount.HasValue)
                {
                    options.Amount = (long)(request.Amount.Value * 100);
                }

                if (request.ReverseTransfer)
                {
                    options.ReverseTransfer = true;
                }

                if (request.RefundApplicationFee.HasValue)
                {
                    options.RefundApplicationFee = request.RefundApplicationFee.Value > 0;
                }

                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(options);

                // Update transaction record
                var transaction = await _paymentTransactionRepository.FirstOrDefaultAsync(t => t.PaymentIntentId == request.PaymentIntentId);
                if (transaction != null)
                {
                    transaction.RefundId = refund.Id;
                    transaction.RefundAmount = refund.Amount / 100m;
                    transaction.RefundStatus = refund.Status;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    await _paymentTransactionRepository.UpdateAsync(transaction);
                }

                _logger.LogInformation("Refund processed successfully: {RefundId}", refund.Id);

                return new RefundResponseDto
                {
                    RefundId = refund.Id,
                    Status = refund.Status,
                    Amount = refund.Amount / 100m,
                    Currency = refund.Currency.ToUpper(),
                    Reason = refund.Reason,
                    CreatedAt = refund.Created,
                    Success = true
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund: {Error}", ex.Message);
                return new RefundResponseDto
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment intent {PaymentIntentId}", request.PaymentIntentId);
                throw;
            }
        }

        public async Task<List<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId)
        {
            try
            {
                var service = new PaymentMethodService();
                var options = new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = "card"
                };

                var paymentMethods = await service.ListAsync(options);
                var result = new List<PaymentMethodDto>();

                foreach (var pm in paymentMethods)
                {
                    // Use StripePaymentMethodDto for Stripe logic
                    var stripeDto = new StripePaymentMethodDto
                    {
                        Id = pm.Id,
                        Type = pm.Type,
                        CreatedAt = pm.Created,
                        IsDefault = pm.Metadata != null && pm.Metadata.ContainsKey("is_default") && pm.Metadata["is_default"] == "true",
                        Card = pm.Card != null ? new CardDetailsDto
                        {
                            Brand = pm.Card.Brand,
                            Last4 = pm.Card.Last4,
                            ExpMonth = (int)pm.Card.ExpMonth,
                            ExpYear = (int)pm.Card.ExpYear,
                            Country = pm.Card.Country,
                            Funding = pm.Card.Funding
                        } : null
                    };

                    // Map to canonical PaymentMethodDto for app logic
                    var dto = new PaymentMethodDto
                    {
                        Id = Guid.TryParse(pm.Id, out var guid) ? guid : Guid.Empty,
                        Type = MapStripePaymentTypeToPaymentType(pm.Type),
                        Title = pm.Card != null ? $"{pm.Card.Brand} ****{pm.Card.Last4}" : pm.Type,
                        LastFourDigits = pm.Card?.Last4,
                        CardHolderName = pm.BillingDetails?.Name,
                        ExpiryDate = pm.Card != null ? $"{pm.Card.ExpMonth:D2}/{pm.Card.ExpYear}" : null,
                        IsDefault = stripeDto.IsDefault
                    };

                    result.Add(dto);
                }

                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error retrieving payment methods: {Error}", ex.Message);
                throw new InvalidOperationException($"Error retrieving payment methods: {ex.Message}", ex);
            }
        }

        public async Task<StripeCustomerDto> CreateOrGetCustomerAsync(CreateCustomerRequestDto request)
        {
            try
            {
                var service = new CustomerService();
                
                // First, try to find existing customer by email
                var searchOptions = new CustomerSearchOptions
                {
                    Query = $"email:'{request.Email}'"
                };

                var existingCustomers = await service.SearchAsync(searchOptions);
                if (existingCustomers.Data.Count > 0)
                {
                    var existing = existingCustomers.Data[0];
                    return new StripeCustomerDto
                    {
                        Id = existing.Id,
                        Email = existing.Email,
                        Name = existing.Name,
                        Phone = existing.Phone,
                        CreatedAt = existing.Created,
                        IsActive = (bool)!existing.Deleted
                    };
                }

                // Create new customer
                var options = new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = request.Name,
                    Phone = request.Phone,
                    Metadata = new Dictionary<string, string>
                    {
                        ["user_id"] = request.UserId.ToString()
                    }
                };

                var address = request.Address;
                if (address is StripeAddressDto stripeAddress)
                {
                    options.Address = new AddressOptions
                    {
                        Line1 = stripeAddress.Line1,
                        Line2 = stripeAddress.Line2,
                        City = stripeAddress.City,
                        State = stripeAddress.State,
                        PostalCode = stripeAddress.PostalCode,
                        Country = stripeAddress.Country
                    };
                }
                else if (address is AddressDto addressDto)
                {
                    options.Address = new AddressOptions
                    {
                        Line1 = addressDto.Street, // or addressDto.FullAddress
                        City = addressDto.City,
                        State = addressDto.State,
                        PostalCode = addressDto.ZipCode,
                        // Country is not available in AddressDto, so leave null
                    };
                }

                var customer = await service.CreateAsync(options);

                _logger.LogInformation("Stripe customer created: {CustomerId} for user {UserId}", customer.Id, request.UserId);

                return new StripeCustomerDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    Phone = customer.Phone,
                    CreatedAt = customer.Created,
                    IsActive = true
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer: {Error}", ex.Message);
                throw new InvalidOperationException($"Error creating customer: {ex.Message}", ex);
            }
        }

        public async Task<WebhookHandlingResultDto> HandleWebhookAsync(string payload, string signature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(payload, signature, _stripeWebhookSecret);
                
                _logger.LogInformation("Processing Stripe webhook: {EventType} - {EventId}", stripeEvent.Type, stripeEvent.Id);

                var result = new WebhookHandlingResultDto
                {
                    EventType = stripeEvent.Type,
                    EventId = stripeEvent.Id,
                    ProcessedAt = DateTime.UtcNow
                };

                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;
                    case Events.PaymentIntentPaymentFailed:
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;
                    case Events.ChargeDisputeCreated:
                        await HandleChargeDispute(stripeEvent);
                        break;
                    default:
                        _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                        break;
                }

                result.Success = true;
                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error: {Error}", ex.Message);
                return new WebhookHandlingResultDto
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                throw;
            }
        }

        public async Task<FeeCalculationDto> CalculateFeesAsync(decimal amount, Guid restaurantId)
        {
            try
            {
                var restaurant = await _restaurantRepository.GetAsync(restaurantId);
                
                // Stripe processing fee (2.9% + $0.30)
                var stripeFee = (amount * 0.029m) + 0.30m;
                
                // Platform commission (configurable per restaurant, default 15%)
                var commissionRate = restaurant.CommissionRate ?? 0.15m;
                var platformFee = amount * commissionRate;
                
                // Restaurant commission (what restaurant pays to platform)
                var restaurantCommission = platformFee;
                
                // Restaurant payout (what restaurant receives)
                var restaurantPayout = amount - stripeFee - platformFee;
                
                // Total fees
                var totalFees = stripeFee + platformFee;

                return new FeeCalculationDto
                {
                    OrderAmount = amount,
                    StripeFee = Math.Round(stripeFee, 2),
                    PlatformFee = Math.Round(platformFee, 2),
                    RestaurantCommission = Math.Round(restaurantCommission, 2),
                    RestaurantPayout = Math.Round(restaurantPayout, 2),
                    TotalFees = Math.Round(totalFees, 2),
                    Currency = "SYP"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fees for restaurant {RestaurantId}", restaurantId);
                throw;
            }
        }

        public async Task<ConnectedAccountDto> CreateConnectedAccountAsync(CreateConnectedAccountRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating connected account for restaurant {RestaurantId}", request.RestaurantId);

                var options = new AccountCreateOptions
                {
                    Type = "express",
                    Country = request.Country,
                    Email = request.Email,
                    BusinessType = request.BusinessType,
                    Metadata = new Dictionary<string, string>
                    {
                        ["restaurant_id"] = request.RestaurantId.ToString()
                    }
                };

                var service = new AccountService();
                var account = await service.CreateAsync(options);

                _logger.LogInformation("Connected account created: {AccountId} for restaurant {RestaurantId}", account.Id, request.RestaurantId);

                return new ConnectedAccountDto
                {
                    Id = account.Id,
                    Email = account.Email,
                    BusinessType = account.BusinessType,
                    Country = account.Country,
                    ChargesEnabled = account.ChargesEnabled,
                    PayoutsEnabled = account.PayoutsEnabled,
                    DetailsSubmitted = account.DetailsSubmitted,
                    CreatedAt = account.Created,
                    Requirements = account.Requirements?.CurrentlyDue?.ToList() ?? new List<string>()
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating connected account: {Error}", ex.Message);
                throw new InvalidOperationException($"Error creating connected account: {ex.Message}", ex);
            }
        }

        public async Task<TransferResponseDto> TransferToRestaurantAsync(TransferRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating transfer of {Amount} to account {DestinationAccountId}", request.Amount, request.DestinationAccountId);

                var options = new TransferCreateOptions
                {
                    Amount = (long)(request.Amount * 100),
                    Currency = request.Currency.ToLower(),
                    Destination = request.DestinationAccountId,
                    Description = request.Description
                };

                if (!string.IsNullOrEmpty(request.SourceTransaction))
                {
                    options.SourceTransaction = request.SourceTransaction;
                }

                var service = new TransferService();
                var transfer = await service.CreateAsync(options);

                _logger.LogInformation("Transfer created successfully: {TransferId}", transfer.Id);

                return new TransferResponseDto
                {
                    TransferId = transfer.Id,
                    Amount = transfer.Amount / 100m,
                    Currency = transfer.Currency.ToUpper(),
                    DestinationAccountId = transfer.DestinationId,
                    Status = TransactionStatus.Succeeded.ToString(), // Transfers are immediate
                    CreatedAt = transfer.Created,
                    Success = true
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating transfer: {Error}", ex.Message);
                return new TransferResponseDto
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            var transaction = await _paymentTransactionRepository.FirstOrDefaultAsync(t => t.PaymentIntentId == paymentIntent.Id);
            if (transaction != null)
            {
                transaction.Status = TransactionStatus.Succeeded;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _paymentTransactionRepository.UpdateAsync(transaction);

                // Update order status
                var order = await _orderRepository.GetAsync(transaction.OrderId);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.Status = OrderStatus.Pending; // Payment confirmed, order now pending restaurant acceptance
                    await _orderRepository.UpdateAsync(order);
                }

                _logger.LogInformation("Payment succeeded for order {OrderId}", transaction.OrderId);
            }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            var transaction = await _paymentTransactionRepository.FirstOrDefaultAsync(t => t.PaymentIntentId == paymentIntent.Id);
            if (transaction != null)
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _paymentTransactionRepository.UpdateAsync(transaction);

                // Update order status
                var order = await _orderRepository.GetAsync(transaction.OrderId);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.Status = OrderStatus.Cancelled;
                    await _orderRepository.UpdateAsync(order);
                }

                _logger.LogWarning("Payment failed for order {OrderId}", transaction.OrderId);
            }
        }

        private async Task HandleChargeDispute(Event stripeEvent)
        {
            var dispute = stripeEvent.Data.Object as Dispute;
            if (dispute == null) return;

            _logger.LogWarning("Charge dispute received: {DisputeId} for charge {ChargeId}", dispute.Id, dispute.ChargeId);
            
            // Handle dispute logic here (notify restaurant, update order status, etc.)
        }

        /// <summary>
        /// Maps Stripe payment method type to our PaymentType enum
        /// </summary>
        private PaymentType MapStripePaymentTypeToPaymentType(string stripeType)
        {
            return stripeType?.ToLower() switch
            {
                "card" => PaymentType.CreditCard,
                "bank_account" => PaymentType.BankTransfer,
                "us_bank_account" => PaymentType.BankTransfer,
                "sepa_debit" => PaymentType.BankTransfer,
                "ideal" => PaymentType.BankTransfer,
                "sofort" => PaymentType.BankTransfer,
                "bancontact" => PaymentType.BankTransfer,
                "eps" => PaymentType.BankTransfer,
                "giropay" => PaymentType.BankTransfer,
                "p24" => PaymentType.BankTransfer,
                "alipay" => PaymentType.DigitalWallet,
                "wechat_pay" => PaymentType.DigitalWallet,
                "paypal" => PaymentType.DigitalWallet,
                "apple_pay" => PaymentType.DigitalWallet,
                "google_pay" => PaymentType.DigitalWallet,
                _ => PaymentType.CreditCard // Default fallback
            };
        }
    }

    // Define StripePaymentMethodDto and StripeAddressDto at the top of the file:
    internal class StripePaymentMethodDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public CardDetailsDto? Card { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDefault { get; set; }
    }

    internal class StripeAddressDto : DeliveryApp.Application.Contracts.Dtos.AddressDto
    {
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}

