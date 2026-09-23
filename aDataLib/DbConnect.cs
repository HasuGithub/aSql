using System.Data.Common;

using Microsoft.Data.SqlClient;

using MySqlConnector;

using Oracle.ManagedDataAccess.Client;

namespace aDataLib;

public class DbConnect
{
  private bool _lWithDelimiter = true;

  internal string lDataSourceName = "";

  internal DataBaseTypes lDbType;

  internal string lPassWord = "";

  public DbConnect()
  {
    ErrMessage = new ErrMessage();
    DriverAndDsn = new FathersAndSons();
    DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);
    DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
    DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnectorFactory.Instance);
  }

  public FathersAndSons? DriverAndDsn { get; set; }

  public string ConnectionString { get; private set; } = "";

  public string DbName { get; private set; } = "";

  public DataDictionary DataDic
  {
    get
    {
      field ??= new DataDictionary(this, false);
      return field;
    }
  }

  public DataBaseTypes DbType => lDbType;

  public DbConnection? Connection { get; set; }

  public ErrMessage? ErrMessage { get; }

  public bool UseUiForErrors { get; set; }

  public string SqlBrackOpenSign { get; set; } = string.Empty;

  public string SqlBrackCloseSign { get; set; } = string.Empty;

  public string SqlMetaSignOne { get; set; } = string.Empty;

  public string SqlMetaSignMany { get; set; } = string.Empty;

  public bool WithDelimiter
  {
    get => _lWithDelimiter;
    set
    {
      if (_lWithDelimiter == value) return;
      _lWithDelimiter = value;
      SwitchDbSpecifika(_lWithDelimiter);
    }
  }

  public bool UseSimpleDate { get; set; }

  public bool UseUpperCaseSql { get; set; }

  public string DbSchema { get; set; } = "";

  public string UserId { get; private set; } = "";

  public static DbConnect CreateWithConnectionString(string connectionString, DataBaseTypes dbType,
    bool withDelimiter = true, string? dbSchema = null, string dbName = "")
  {
    if (string.IsNullOrWhiteSpace(connectionString))
      throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

    if (dbType != DataBaseTypes.MsSqlServer && dbType != DataBaseTypes.Oracle && dbType != DataBaseTypes.MySql &&
        dbType != DataBaseTypes.Maria)
      throw new NotSupportedException($"Only SQL Server is supported in refactored mode. Current DbType: {dbType}.");

    var db = new DbConnect
    {
      lDbType = dbType,
      _lWithDelimiter = withDelimiter,
      DbSchema = dbSchema ?? string.Empty,
      ConnectionString = connectionString,
      UseUiForErrors = false
    };

    if (!db.ConnectSql(dbType, true))
      throw new InvalidOperationException("Failed to open database connection using SqlClient.");

    db.ApplyOpenConnectionMetadata(dbName);

    db.SwitchDbSpecifika(db._lWithDelimiter);
    return db;
  }

  private void ApplyOpenConnectionMetadata(string dbName = "")
  {
    if (Connection == null) return;

    if (Connection.Database.Length > 0)
    {
      lDataSourceName = Connection.Database;
      DbName = Connection.Database;
    }

    if (dbName.Length > 0) DbName = dbName;

    if (string.IsNullOrWhiteSpace(ConnectionString)) ConnectionString = Connection.ConnectionString;
  }

  private bool ConnectSql(DataBaseTypes dbType, bool throwOnError = false)
  {
    var providerName = dbType is DataBaseTypes.Oracle ? "Oracle.ManagedDataAccess.Client" : "Microsoft.Data.SqlClient";
    providerName = dbType is DataBaseTypes.MySql or DataBaseTypes.Maria ? "MySqlConnector" : providerName;
    var factory = DbProviderFactories.GetFactory(providerName)
                  ?? throw new Exception($"Provider '{providerName}' nicht gefunden.");

    var connection = factory.CreateConnection();
    connection?.ConnectionString = ConnectionString;

    try
    {
      connection?.Open();
    }
    catch (SqlException ex)
    {
      if (throwOnError) throw;
      ErrMessage!.Msg = "";
      if (ex.Errors.Count <= 0) return false;
      for (var i = 0; i < ex.Errors.Count; i++)
        ErrMessage.Msg += string.Concat(new object[]
        {
          "Index #",
          i,
          "\nMessage: ",
          ex.Errors[i].Message,
          "\nNumber: ",
          ex.Errors[i].Number.ToString(),
          "\nSource: ",
          ex.Errors[i].Source,
          "\nState: ",
          ex.Errors[i].State.ToString(),
          "\n"
        });
      return false;
    }
    catch (InvalidOperationException ex2)
    {
      if (throwOnError) throw;
      ErrMessage!.Msg = "";
      ErrMessage.Msg += string.Concat(new[]
      {
        "Source #",
        ex2.Source ?? string.Empty,
        "\nMessage: ",
        ex2.Message,
        "\n"
      });
      return false;
    }
    catch (ArgumentException ex3)
    {
      if (throwOnError) throw;
      ErrMessage!.Msg = "";
      ErrMessage.Msg += string.Concat(new[]
      {
        "Source #",
        ex3.Source ?? string.Empty,
        "\nMessage: ",
        ex3.Message,
        "\n"
      });
      return false;
    }
    catch (Exception ex4)
    {
      if (throwOnError) throw;
      ErrMessage!.Msg = "";
      ErrMessage.Msg += string.Concat(new[]
      {
        "Source #",
        ex4.Source ?? string.Empty,
        "\nMessage: ",
        ex4.Message,
        "\n"
      });
      return false;
    }

    var text = DbName;
    if (connection?.Database.Length > 0)
      DbName = connection.Database;
    else if (lDataSourceName.Length > 0)
      DbName = lDataSourceName;
    else
      DbName = text;
    var num = ConnectionString.ToLower().IndexOf("user id", StringComparison.Ordinal);
    if (num == -1) num = ConnectionString.ToLower().IndexOf("uid", StringComparison.Ordinal);
    if (num > -1)
    {
      num = ConnectionString.IndexOf("=", num, StringComparison.Ordinal);
      if (num > -1)
      {
        var num2 = ConnectionString.IndexOf(";", num, StringComparison.Ordinal);
        if (num2 > -1) UserId = ConnectionString.Substring(num + 1, num2 - num - 1);
      }
    }

    var num3 = ConnectionString.ToLower().IndexOf("database password", StringComparison.Ordinal);
    var num4 = 0;
    if (num3 > -1) num4 = 2;
    num = ConnectionString.ToLower().IndexOf("password", StringComparison.Ordinal);
    if (num > -1)
    {
      if (num4 == 2 && num - num3 == 9) num4 = 3;
    }
    else
    {
      num = ConnectionString.ToLower().IndexOf("pwd", StringComparison.Ordinal);
      num4 = 1;
    }

    if (num > -1)
    {
      num = ConnectionString.IndexOf("=", num, StringComparison.Ordinal);
      if (num > -1)
      {
        var num2 = ConnectionString.IndexOf(";", num, StringComparison.Ordinal);
        if (num2 > -1 && num4 != 3) lPassWord = ConnectionString.Substring(num + 1, num2 - num - 1);
      }
    }

    Connection = connection;
    SwitchDbSpecifika(_lWithDelimiter);
    return true;
  }

  private void SwitchDbSpecifika(bool withDelimiter)
  {
    switch (lDbType)
    {
      case DataBaseTypes.MySql:
      case DataBaseTypes.Maria:
      {
        if (withDelimiter)
        {
          SqlBrackOpenSign = "`";
          SqlBrackCloseSign = "`";
        }
        else
        {
          SqlBrackOpenSign = "";
          SqlBrackCloseSign = "";
        }

        SqlMetaSignOne = "_";
        SqlMetaSignMany = "%";
        return;
      }
      case DataBaseTypes.Asa7:
      case DataBaseTypes.Asa8:
      case DataBaseTypes.Asa9:
      case DataBaseTypes.Asa10:
      case DataBaseTypes.Asa11:
      {
        break;
      }
      case DataBaseTypes.MsSqlServer:
      case DataBaseTypes.MsAccess:
      {
        if (withDelimiter)
        {
          SqlBrackOpenSign = "[";
          SqlBrackCloseSign = "]";
        }
        else
        {
          SqlBrackOpenSign = "";
          SqlBrackCloseSign = "";
        }

        SqlMetaSignOne = "?";
        SqlMetaSignMany = "*";
        return;
      }
      case DataBaseTypes.Oracle:
      case DataBaseTypes.NotDefined:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    if (withDelimiter)
    {
      SqlBrackOpenSign = "\"";
      SqlBrackCloseSign = "\"";
    }
    else
    {
      SqlBrackOpenSign = "";
      SqlBrackCloseSign = "";
    }

    SqlMetaSignOne = "?";
    SqlMetaSignMany = "%";
  }
}