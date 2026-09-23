using System.Data.Common;

using Microsoft.Data.SqlClient;

namespace aDataLib.Schema;

internal static class SchemaConnectionFactory
{
  public static (DbConnection Connection, bool OwnsConnection) OpenSchemaConnection(DbConnect dbConnect)
  {
    ArgumentNullException.ThrowIfNull(dbConnect);

    if (dbConnect.DbType != DataBaseTypes.MsSqlServer && dbConnect.DbType != DataBaseTypes.Oracle &&
        dbConnect.DbType != DataBaseTypes.MySql && dbConnect.DbType != DataBaseTypes.Maria)
      throw new NotSupportedException(
        $"Schema connection factory currently supports only SQL Server. Current DbType: {dbConnect.DbType}.");

    return (OpenSqlServerConnection(dbConnect.ConnectionString, dbConnect.Connection!), true);
  }

  private static DbConnection OpenSqlServerConnection(string connectionString, DbConnection conn)
  {
    if (string.IsNullOrWhiteSpace(connectionString))
      throw new InvalidOperationException("Connection string is not available for SQL Server schema loading.");

    var source = new DbConnectionStringBuilder { ConnectionString = connectionString };
    var target = new SqlConnectionStringBuilder
    {
      TrustServerCertificate = true
    };

    if (TryGet(source, out var value, "Data Source", "DataSource", "Server", "Address", "Addr", "Network Address",
          "DSN")) target.DataSource = value;

    if (TryGet(source, out value, "Initial Catalog", "Database", "DbName")) target.InitialCatalog = value;

    if (TryGet(source, out value, "User ID", "UID", "UserId")) target.UserID = value;

    if (TryGet(source, out value, "Password", "PassWord", "PWD")) target.Password = value;

    if (TryGet(source, out value, "Integrated Security", "Trusted_Connection") &&
        bool.TryParse(value, out var integrated))
      target.IntegratedSecurity = integrated;

    if (TryGet(source, out value, "Connect Timeout", "Connection Timeout") &&
        int.TryParse(value, out var timeout))
      target.ConnectTimeout = timeout;

    if (TryGet(source, out value, "Trust Server Certificate", "TrustServerCertificate") &&
        bool.TryParse(value, out var trustCert))
      target.TrustServerCertificate = trustCert;

    return conn;
  }

  private static bool TryGet(DbConnectionStringBuilder builder, out string value, params string[] keys)
  {
    foreach (var key in keys)
    {
      if (!builder.TryGetValue(key, out var raw)) continue;
      var text = raw.ToString() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(text)) continue;
      value = text;
      return true;
    }

    value = string.Empty;
    return false;
  }
}