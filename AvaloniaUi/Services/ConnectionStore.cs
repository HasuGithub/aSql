using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using aSql.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace aSql.Services;

public static class ConnectionStore
{
  private static readonly string StorePath;

  // ── AES-Fallback (nicht-Windows) ──────────────────────────────────────
  // Schlüssel: SHA-256 über App-Name + Maschinenname (nicht exportierbar)
  private static readonly byte[] _aesKey =
    SHA256.HashData(Encoding.UTF8.GetBytes("aSql_ConnectionStore_v1_" + Environment.MachineName));

  static ConnectionStore()
  {
    var appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    var folderPath = Path.Combine(appDataRoot, "aSqlFiles");

    Directory.CreateDirectory(folderPath);

    StorePath = Path.Combine(folderPath, "connections.json");
  }

  public static List<DbConnectionEntry>? Load()
  {
    if (!File.Exists(StorePath))
      return [];
    try
    {
      var json = File.ReadAllText(StorePath);

      return JsonSerializer.Deserialize(json, AppJsonContext.Default.ListDbConnectionEntry);
    }
    catch (Exception ex)
    {
      MessageBoxManager.GetMessageBoxStandard("Fehler", ex.Message, ButtonEnum.Ok, Icon.Error)
        .ShowWindowDialogAsync(null!);
      return [];
    }
  }

  public static void Save(IEnumerable<DbConnectionEntry> entries)
  {
    var json = JsonSerializer.Serialize([.. entries], AppJsonContext.Default.ListDbConnectionEntry);
    File.WriteAllText(StorePath, json);
  }

  public static string EncryptPassword(string plaintext)
  {
    if (string.IsNullOrEmpty(plaintext))
      return string.Empty;

    var bytes = Encoding.UTF8.GetBytes(plaintext);

    return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
      ? Convert.ToBase64String(DpapiProtect(bytes))
      : AesEncrypt(bytes);
  }

  public static string DecryptPassword(string encrypted)
  {
    if (string.IsNullOrEmpty(encrypted))
      return string.Empty;
    try
    {
      var bytes = Convert.FromBase64String(encrypted);

      return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Encoding.UTF8.GetString(DpapiUnprotect(bytes))
        : AesDecrypt(bytes);
    }
    catch
    {
      return string.Empty;
    }
  }

  public static string BuildConnectionString(DbConnectionEntry entry, string plaintextPassword)
  {
    switch (entry.Provider)
    {
      case DbProvider.SqlServer:
      {
        var encrypt = entry.Encrypt.ToString().ToLower();

        return $"Server={entry.ServerName};" +
               $"Database={entry.DatabaseName};" +
               $"User Id={entry.UserName};" +
               $"Password={plaintextPassword};" +
               $"Encrypt={encrypt};" +
               $"TrustServerCertificate={entry.TrustServerCertificate};";
      }
      case DbProvider.Oracle when plaintextPassword.Length > 0:
        return $"Data Source={entry.ServerName};" +
               $"User Id={entry.UserName};" +
               $"Password={plaintextPassword};";
      case DbProvider.Oracle:
        return $"Data Source={entry.ServerName};" +
               $"User Id={entry.UserName};";
      case DbProvider.MySql:
        return $"Server={entry.ServerName};" +
               $"Database={entry.DatabaseName};" +
               $"Uid={entry.UserName};" +
               $"Pwd={plaintextPassword};";
      case DbProvider.MariaDb:
        return $"Server={entry.ServerName};" +
               $"Port={entry.Port};" +
               $"Database={entry.DatabaseName};" +
               $"Uid={entry.UserName};" +
               $"Pwd={plaintextPassword};";
      default:
        return string.Empty;
    }
  }

  [SupportedOSPlatform("windows")]
  private static byte[] DpapiProtect(byte[] data)
  {
    return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
  }

  [SupportedOSPlatform("windows")]
  private static byte[] DpapiUnprotect(byte[] data)
  {
    return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
  }

  private static string AesEncrypt(byte[] data)
  {
    using var aes = Aes.Create();
    aes.Key = _aesKey;
    aes.GenerateIV();
    using var enc = aes.CreateEncryptor();
    var cipher = enc.TransformFinalBlock(data, 0, data.Length);
    // Präfix: IV (16 Byte) + Cipher
    var result = new byte[aes.IV.Length + cipher.Length];
    aes.IV.CopyTo(result, 0);
    cipher.CopyTo(result, aes.IV.Length);
    return Convert.ToBase64String(result);
  }

  private static string AesDecrypt(byte[] data)
  {
    using var aes = Aes.Create();
    aes.Key = _aesKey;
    aes.IV = data[..16];
    using var dec = aes.CreateDecryptor();
    return Encoding.UTF8.GetString(dec.TransformFinalBlock(data, 16, data.Length - 16));
  }
}

[JsonSerializable(typeof(DbConnectionEntry))]
[JsonSerializable(typeof(List<DbConnectionEntry>))]
internal partial class AppJsonContext : JsonSerializerContext;