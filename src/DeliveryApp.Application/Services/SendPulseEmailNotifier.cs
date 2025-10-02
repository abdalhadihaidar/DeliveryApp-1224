using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;

namespace DeliveryApp.Application.Services
{
    public class SendPulseEmailNotifier : ApplicationService, IEmailNotifier
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendPulseEmailNotifier> _logger;
        private string _accessToken;
        private DateTime _tokenExpiry;

        public SendPulseEmailNotifier(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<SendPulseEmailNotifier> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAccountApprovedAsync(string email, string userName)
        {
            var subject = "Account Approved - Welcome to Delivery App";
            var htmlContent = $@"
                <html>
                <body>
                    <h2>مرحباً {userName}</h2>
                    <p>تم تفعيل حسابك بنجاح من قبل الإدارة.</p>
                    <p>يمكنك الآن تسجيل الدخول والاستمتاع بخدماتنا.</p>
                    <br>
                    <h2>Welcome {userName}</h2>
                    <p>Your account has been successfully approved by our admin team.</p>
                    <p>You can now sign in and enjoy our services.</p>
                    <hr>
                    <p>Best regards,<br>Delivery App Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlContent);
        }

        public async Task SendWelcomeAsync(string email, string userName)
        {
            var subject = "Welcome to Delivery App";
            var htmlContent = $@"
                <html>
                <body>
                    <h2>مرحباً بك {userName}</h2>
                    <p>شكراً لتسجيلك في تطبيق التوصيل.</p>
                    <p>حسابك قيد المراجعة وسيتم تفعيله قريباً.</p>
                    <br>
                    <h2>Welcome {userName}</h2>
                    <p>Thank you for registering with Delivery App.</p>
                    <p>Your account is under review and will be activated soon.</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlContent);
        }

        public async Task SendOrderStatusAsync(string email, string orderNumber, string status)
        {
            var subject = $"Order Update - {orderNumber}";
            var htmlContent = $@"
                <html>
                <body>
                    <h2>تحديث الطلب {orderNumber}</h2>
                    <p>حالة طلبك الآن: {status}</p>
                    <br>
                    <h2>Order Update {orderNumber}</h2>
                    <p>Your order status: {status}</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlContent);
        }

        private async Task SendEmailAsync(string email, string subject, string htmlContent)
        {
            try
            {
                await EnsureValidTokenAsync();

                var emailData = new
                {
                    email = new
                    {
                        html = htmlContent,
                        text = "",
                        subject = subject,
                        from = new
                        {
                            name = _configuration["SendPulse:FromName"] ?? "Delivery App",
                            email = _configuration["SendPulse:FromEmail"] ?? "noreply@deliveryapp.com"
                        },
                        to = new[]
                        {
                            new { email = email }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(emailData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.PostAsync("https://api.sendpulse.com/smtp/emails", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Email sent successfully to {email}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send email to {email}. Status: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while sending email to {email}");
            }
        }

        private async Task EnsureValidTokenAsync()
        {
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
                return;

            await GetAccessTokenAsync();
        }

        private async Task GetAccessTokenAsync()
        {
            try
            {
                var clientId = _configuration["SendPulse:ClientId"];
                var clientSecret = _configuration["SendPulse:ClientSecret"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("SendPulse credentials not configured");
                    return;
                }

                var tokenData = new
                {
                    grant_type = "client_credentials",
                    client_id = clientId,
                    client_secret = clientSecret
                };

                var json = JsonSerializer.Serialize(tokenData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.sendpulse.com/oauth/access_token", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    _accessToken = tokenResponse.GetProperty("access_token").GetString();
                    var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Refresh 1 minute early
                    
                    _logger.LogInformation("SendPulse access token obtained successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get SendPulse access token. Status: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while getting SendPulse access token");
            }
        }
    }
}
