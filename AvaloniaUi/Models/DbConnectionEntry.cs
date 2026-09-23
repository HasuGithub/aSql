namespace aSql.Models;

public enum DbProvider
{
  SqlServer = 0,
  Oracle = 1,
  MySql = 2,
  MariaDb = 3
}

public sealed class DbConnectionEntry
{
  public string Alias { get; set; } = string.Empty;
  public DbProvider Provider { get; set; } = DbProvider.SqlServer;
  public string Port { get; set; } = string.Empty;
  public string ServerName { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public string PasswordEncrypted { get; set; } = string.Empty;
  public bool SavePassword { get; set; }
  public string DatabaseName { get; set; } = string.Empty;
  public bool Encrypt { get; set; } = false;
  public bool TrustServerCertificate { get; set; } = true;
  public DateTime LastUsed { get; set; } = DateTime.UtcNow;
}