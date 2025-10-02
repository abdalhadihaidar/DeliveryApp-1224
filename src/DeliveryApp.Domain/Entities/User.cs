using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        [StringLength(100)]
        public new string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string ProfileImageUrl { get; set; } = string.Empty;

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<PaymentMethod> PaymentMethods { get; set; }
        public virtual ICollection<FavoriteRestaurant> FavoriteRestaurants { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        // Additional properties for delivery app functionality
        public virtual UserPreferences? Preferences { get; set; }
        public virtual Location? CurrentLocation { get; set; }
        public virtual DeliveryStatus? DeliveryStatus { get; set; }

        // KYC/Review status and reason
        public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Pending;
        public string? ReviewReason { get; set; }

        // Admin approval & confirmation flags
        public bool IsEmailConfirmed { get; set; } = false;
        public bool IsPhoneConfirmed { get; set; } = false;
        public bool IsAdminApproved { get; set; } = false;
        public DateTime? ApprovedTime { get; set; }
        public Guid? ApprovedById { get; set; }
        
        // Device token for push notifications
        public string? DeviceToken { get; set; }

        public AppUser()
        {
            Addresses = new List<Address>();
            PaymentMethods = new List<PaymentMethod>();
            FavoriteRestaurants = new List<FavoriteRestaurant>();
            Orders = new List<Order>();
        }

        // Constructor with ID parameter for seeding only
        public AppUser(Guid id, string userName, string email, Guid? tenantId = null) : base(id, userName, email, tenantId)
        {
            Addresses = new List<Address>();
            PaymentMethods = new List<PaymentMethod>();
            FavoriteRestaurants = new List<FavoriteRestaurant>();
            Orders = new List<Order>();
        }
    }

    public class PaymentMethod : FullAuditedEntity<Guid>
    {
        public PaymentMethod() { }
        
        // Constructor with ID parameter for seeding only
        public PaymentMethod(Guid id) : base(id) { }
        
        [Required]
        public PaymentType Type { get; set; } = PaymentType.CreditCard;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4)]
        public string LastFourDigits { get; set; } = string.Empty;

        [StringLength(100)]
        public string CardHolderName { get; set; } = string.Empty;

        [StringLength(10)]
        public string ExpiryDate { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; } = null!;
    }

    /// <summary>
    /// Payment method types
    /// </summary>
    public enum PaymentType
    {
        CreditCard = 1,
        DebitCard = 2,
        CashOnDelivery = 3,
        BankTransfer = 4,
        DigitalWallet = 5
    }

    public class FavoriteRestaurant : FullAuditedEntity<Guid>
    {
        public FavoriteRestaurant() { }
        
        // Constructor with ID parameter for seeding only
        public FavoriteRestaurant(Guid id) : base(id) { }
        
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; } = null!;

        public Guid RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; } = null!;
    }

    public class Order : FullAuditedAggregateRoot<Guid>
    {
        // Constructor with ID parameter for seeding only
        public Order(Guid id) : base(id)
        {
            Items = new List<OrderItem>();
            OrderDate = DateTime.Now;
        }
        
        public Guid RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; } = null!;

        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; } = null!;

        public Guid? DeliveryPersonId { get; set; }
        public virtual AppUser? DeliveryPerson { get; set; }

        public DateTime OrderDate { get; set; }

        [Range(0, 10000)]
        public decimal Subtotal { get; set; }

        [Range(0, 1000)]
        public decimal DeliveryFee { get; set; }

        [Range(0, 1000)]
        public decimal Tax { get; set; }

        [Range(0, 10000)]
        public decimal TotalAmount { get; set; }

        public int EstimatedDeliveryTime { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public Guid DeliveryAddressId { get; set; }
        public virtual Address DeliveryAddress { get; set; } = null!;

        public Guid? PaymentMethodId { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; }

        // Default constructor for EF Core
        public Order()
        {
            Items = new List<OrderItem>();
            OrderDate = DateTime.Now;
        }

        public decimal Total => Subtotal + DeliveryFee + Tax;

        // Payment status of the order
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }

    public class OrderItem : FullAuditedEntity<Guid>
    {
        // Default constructor for EF Core
        public OrderItem()
        {
            Options = new List<string>();
            SelectedOptions = new List<string>();
        }
        
        // Constructor with ID for seeding only
        public OrderItem(Guid id) : base(id)
        {
            Options = new List<string>();
            SelectedOptions = new List<string>();
        }
        
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public Guid MenuItemId { get; set; }
        public virtual MenuItem MenuItem { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 100)]
        public int Quantity { get; set; }

        [Range(0, 1000)]
        public decimal Price { get; set; }

        public virtual ICollection<string> Options { get; set; }
        
        public virtual ICollection<string> SelectedOptions { get; set; }
        
        [StringLength(500)]
        public string? SpecialInstructions { get; set; }

        // Constructor removed to avoid duplication
    }

    public class UserPreferences : FullAuditedEntity<Guid>
    {
        public UserPreferences() { }
        
        // Constructor with ID parameter for seeding only
        public UserPreferences(Guid id) : base(id) { }
        
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; } = null!;
        
        public virtual ICollection<string> FavoriteCuisines { get; set; } = new List<string>();
        public virtual ICollection<string> DietaryRestrictions { get; set; } = new List<string>();
        public virtual NotificationSettings NotificationSettings { get; set; } = new NotificationSettings();
    }

    public class NotificationSettings : FullAuditedEntity<Guid>
    {
        public NotificationSettings() { }
        
        // Constructor with ID parameter for seeding only
        public NotificationSettings(Guid id) : base(id) { }
        
        public bool OrderUpdates { get; set; } = true;
        public bool SpecialOffers { get; set; } = true;
        public bool NewsletterSubscription { get; set; } = false;
    }

    public class Location : FullAuditedEntity<Guid>
    {
        public Location() { }
        
        // Constructor with ID parameter for seeding only
        public Location(Guid id) : base(id) { }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class DeliveryStatus : FullAuditedEntity<Guid>
    {
        public DeliveryStatus() { }
        
        // Constructor with ID parameter for seeding only
        public DeliveryStatus(Guid id) : base(id) { }
        
        public bool IsAvailable { get; set; } = true;
        public Guid? CurrentOrderId { get; set; }
        public DateTime LastStatusUpdate { get; set; } = DateTime.Now;
        
        // Cash on Delivery (COD) related fields
        public decimal CashBalance { get; set; } = 0;
        public decimal MaxCashLimit { get; set; } = 1000; // Maximum cash driver can carry
        public bool AcceptsCOD { get; set; } = true; // Whether driver accepts COD orders
    }

    /// <summary>
    /// Cash on Delivery transaction tracking
    /// </summary>
    public class CODTransaction : FullAuditedEntity<Guid>
    {
        public CODTransaction() { }
        
        // Constructor with ID parameter for seeding only
        public CODTransaction(Guid id) : base(id) { }
        
        [Required]
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        
        [Required]
        public Guid DeliveryPersonId { get; set; }
        public virtual AppUser DeliveryPerson { get; set; } = null!;
        
        [Required]
        public Guid RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; } = null!;
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public CODTransactionType Type { get; set; }
        
        public CODTransactionStatus Status { get; set; } = CODTransactionStatus.Pending;
        
        public DateTime? CompletedAt { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // For tracking cash flow: driver pays restaurant, then collects from customer
        public decimal DriverPaidToRestaurant { get; set; } = 0;
        public decimal DriverCollectedFromCustomer { get; set; } = 0;
        public decimal DriverProfit { get; set; } = 0; // Delivery fee
    }

    /// <summary>
    /// Types of COD transactions
    /// </summary>
    public enum CODTransactionType
    {
        DriverToRestaurant = 1,    // Driver pays restaurant
        CustomerToDriver = 2,     // Customer pays driver
        Refund = 3                // Refund transaction
    }

    /// <summary>
    /// Status of COD transactions
    /// </summary>
    public enum CODTransactionStatus
    {
        Pending = 0,             // Transaction initiated
        Completed = 1,            // Transaction completed successfully
        Failed = 2,              // Transaction failed
        Cancelled = 3            // Transaction cancelled
    }


}
