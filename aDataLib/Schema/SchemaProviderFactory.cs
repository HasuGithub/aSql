namespace aDataLib.Schema;

public static class SchemaProviderFactory
{
  internal static SchemaProviderContext Resolve(DbConnect dbConnect)
  {
    ArgumentNullException.ThrowIfNull(dbConnect);

    var provider = Create(dbConnect.DbType);
    var connectionContext = SchemaConnectionFactory.OpenSchemaConnection(dbConnect);
    return new SchemaProviderContext(provider, connectionContext.Connection, connectionContext.OwnsConnection);
  }

  public static ISchemaProvider Create(DataBaseTypes dbType)
  {
    switch (dbType)
    {
      case DataBaseTypes.MsSqlServer:
        return new SqlServerSchemaProvider();
      case DataBaseTypes.Oracle:
        return new OracleSchemaProvider();
      case DataBaseTypes.MySql:
        return new MySqlSchemaProvider();
      case DataBaseTypes.Maria:
        return new MariaDbSchemaProvider();
      case DataBaseTypes.Asa7:
      case DataBaseTypes.MsAccess:
      case DataBaseTypes.Asa8:
      case DataBaseTypes.Asa9:
      case DataBaseTypes.Asa10:
      case DataBaseTypes.Asa11:
      case DataBaseTypes.NotDefined:
      default:
        throw new NotSupportedException($"Schema provider for database type '{dbType}' is not implemented.");
    }
  }
}