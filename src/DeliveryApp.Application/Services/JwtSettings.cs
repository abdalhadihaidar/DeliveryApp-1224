namespace DeliveryApp.Application.Services
{
    public class JwtSettings
    {
        // IMPORTANT: Do NOT hard-code secrets in source control.
        // Provide the secret key via configuration or environment variable (e.g. JWT_SECRET_KEY).
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "DeliveryApp";
        public string Audience { get; set; } = "DeliveryAppMobile";
    }
} 
