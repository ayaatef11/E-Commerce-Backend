namespace E_Commerce.Core.Shared.Utilties.Identity;
    public class JwtConfig
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public TimeSpan ExpiryMinutes { get; set; } 
        public string Audience { get; set; } = string.Empty;
        public double ExpireDays { get;  set; }
    }
