namespace Identity.Common.Config;

public class AppConfig
{
  public required string SigningCertPath { get; set; }
  public required string SigningCertPassword { get; set; }
  public required string EncryptionKey { get; set; }
}
