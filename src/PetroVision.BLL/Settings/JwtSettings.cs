namespace PetroVision.BLL.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PetroVision";
    public string Audience { get; set; } = "PetroVision.Clients";
    public int ExpiryMinutes { get; set; } = 480;
}
