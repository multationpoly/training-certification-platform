namespace TrainingCertification.API.Helpers;

public class JwtSettings
{
    public string Issuer { get; set; } = "TrainingCertificationPlatform";
    public string Audience { get; set; } = "TrainingCertificationPlatformUsers";
    public string Key { get; set; } = "TrainingCertificationPlatform-Demo-Key-Change-In-Production-2026";
    public string Secret { get => Key; set => Key = value; }
    public int ExpiryMinutes { get; set; } = 120;
}
