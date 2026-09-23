using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;

using aDataLib;

using ReactiveUI;

namespace aSql.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
  private DbConnect? _dbConnect;

  public MainWindowViewModel()
  {
    StatusMessage = new StatusMessageViewModel();
    SqlEditor = new SqlEditorViewModel();
    SqlEditor.PropertyChanged += OnSqlEditorPropertyChanged;
    StatusMessage.AppendMessageLine(SqlEditor.StatusMessage);
  }

  public DbConnect? DbConnect
  {
    get => _dbConnect;
    set
    {
      _dbConnect = value;
      if (_dbConnect != null) DbConnectRefresh(_dbConnect, DbTreeView, true);
    }
  }

  public DbTreeViewModel DbTreeView { get; set; } = new();

  public SqlEditorViewModel SqlEditor { get; set; }

  public StatusMessageViewModel StatusMessage { get; }

  public string NativeDbProviderText
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = "kein DB-Provider...";

  public string ConnectionStatusText
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = "kein Connection-Status";

  public bool IsWithNoLockVisible
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = true;

  public bool IsUseTopVisible
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = true;

  public bool WithNoLock
  {
    get;
    set
    {
      if (field == value) return;

      field = value;
      this.RaiseAndSetIfChanged(ref field, value);
      SqlEditor.WithNoLock = value;
      SqlEditor.RegenerateSqlFromDefinition();
    }
  }

  public bool WithDelimiter
  {
    get;
    set
    {
      if (field == value) return;

      field = value;
      this.RaiseAndSetIfChanged(ref field, value);
      SqlEditor.DbConnect?.WithDelimiter = value;
      SqlEditor.RegenerateSqlFromDefinition();
    }
  }

  public bool UseSimpleDate
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      SqlEditor.DbConnect?.UseSimpleDate = value;
      SqlEditor.UseSimpleDate = value;
      SqlEditor.RegenerateSqlFromDefinition();
    }
  }

  public bool UseUpperCaseSql
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      SqlEditor.DbConnect?.UseUpperCaseSql = value;
      SqlEditor.RegenerateSqlFromDefinition();
    }
  }

  public bool UseTopEnabled
  {
    get;
    set
    {
      SqlEditor.UseTopEnabled = value;
      SqlEditor.RegenerateSqlFromDefinition();
      this.RaiseAndSetIfChanged(ref field, value);
    }
  } = true;

  public bool IsLoading
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public void DbConnectRefresh(DbConnect dbCon, DbTreeViewModel dbTreeModel, bool withRefreshDictionary = false)
  {
    IsLoading = true;
    _dbConnect = dbCon;

    SqlEditor.DbConnect = _dbConnect;

    if (_dbConnect != null)
    {
      IsWithNoLockVisible = DetermineIsSqlServerFromProvider(_dbConnect);

      if (withRefreshDictionary)
        BuildDataDictionaryTree();
      else
        DbTreeView = dbTreeModel;

      NativeDbProviderText = BuildNativeDbProviderText(_dbConnect);
      ConnectionStatusText = BuildConnectionStatusText(_dbConnect);
    }

    IsLoading = false;
  }

  private void OnSqlEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(SqlEditorViewModel.StatusMessage))
      StatusMessage.AppendMessageLine(SqlEditor.StatusMessage);
  }

  private void BuildDataDictionaryTree()
  {
    DbTreeView.BuildFrom(DbConnect);
  }

  private static string BuildNativeDbProviderText(DbConnect? dbConnect)
  {
    var providerName = ResolveNativeProviderName(dbConnect);
    return $"Provider: {providerName}";
  }

  private static string ResolveNativeProviderName(DbConnect? dbConnect)
  {
    if (dbConnect?.Connection == null)
    {
      return "Unbekannt";
    }

    var providerTypeName = dbConnect.Connection.GetType().FullName ?? dbConnect.Connection.GetType().Name;

    if (providerTypeName.Contains("Microsoft.Data.SqlClient.SqlConnection", StringComparison.Ordinal))
      return "SQL Server (Microsoft.Data.SqlClient)";

    if (providerTypeName.Contains("System.Data.SqlClient", StringComparison.Ordinal))
      return "SQL Server (System.Data.SqlClient)";

    if (providerTypeName.Contains("MySqlConnector.MySqlConnection", StringComparison.Ordinal))
      return dbConnect.DbType == DataBaseTypes.MySql ? "MySQL (MySqlConnector)" : "MariaDB (MySqlConnector)";

    return providerTypeName.Contains("Oracle.ManagedDataAccess", StringComparison.Ordinal)
      ? "Oracle (Oracle.ManagedDataAccess)"
      : "No Provider";
  }

  private static bool DetermineIsSqlServerFromProvider(DbConnect? dbConnect)
  {
    return dbConnect?.DbType == DataBaseTypes.MsSqlServer;
  }

  private static string BuildConnectionStatusText(DbConnect? dbConnect)
  {
    if (dbConnect == null) return "Keine aktive DB-Verbindung";

    var server = ExtractServerFromConnectionString(dbConnect.ConnectionString);
    if (string.IsNullOrWhiteSpace(server)) server = "(unbekannt)";

    var dbName = string.IsNullOrWhiteSpace(dbConnect.DbName) ? "(unbekannt)" : dbConnect.DbName;
    var user = string.IsNullOrWhiteSpace(dbConnect.UserId) ? "(unbekannt)" : dbConnect.UserId;

    return $"Server: {server} | DB: {dbName} | User: {user}";
  }

  private static string ExtractServerFromConnectionString(string connectionString)
  {
    if (string.IsNullOrWhiteSpace(connectionString)) return string.Empty;

    var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
    foreach (var key in new[] { "Data Source", "DataSource", "Server", "Address", "Addr", "Network Address" })
    {
      if (!builder.TryGetValue(key, out var raw)) continue;
      var value = raw.ToString() ?? string.Empty;
      if (!string.IsNullOrWhiteSpace(value)) return value;
    }

    return string.Empty;
  }
}

public sealed class DataTreeNode(string name, DataTreeNodeKind kind, object? tag, string? toolTipText = null)
{
  public DataTreeNode() : this(string.Empty, DataTreeNodeKind.Database, string.Empty, string.Empty)
  {
  }

  public string Name { get; } = name;
  public DataTreeNodeKind Kind { get; } = kind;
  public object? Tag { get; } = tag;
  public string? ToolTipText { get; } = toolTipText;

  public DataTreeNode? Parent { get; set; }

  public bool IsExpanded { get; set; }

  public ObservableCollection<DataTreeNode> Children { get; } = [];
}

public enum DataTreeNodeKind
{
  Database,
  Table,
  ColumnsGroup,
  Column,
  IndexesGroup,
  Index,
  IndexColumn,
  RelationsGroup,
  Relation,
  RelationColumn,
  Select,
  SqlTablesGroup,
  SqlJoinsGroup,
  SqlWhereGroup,
  SqlGroupByGroup,
  SqlHavingGroup,
  SqlOrderByGroup,
  SqlJoin,
  SqlCondition,
  SqlGrouping,
  SqlOrdering
}