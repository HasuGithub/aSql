using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using aDataLib;
using Avalonia.Threading;
using Microsoft.Data.SqlClient;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MySqlConnector;
using Oracle.ManagedDataAccess.Client;
using ReactiveUI;

namespace aSql.ViewModels;

public sealed class SqlEditorViewModel : ViewModelBase
{
  public enum SqlExecutionDialect
  {
    SqlServer,
    Oracle,
    MySql,
    MariaDb
  }

  private readonly DispatcherTimer _autoExecuteTimer;
  private readonly Lock _executionStateSync = new();
  private CancellationTokenSource? _currentManualExecutionCts;

  private string normalizedConnectionString = string.Empty;

  public SqlEditorViewModel()
  {
    DecreaseCommandTimeoutCommand = ReactiveCommand.Create(DecreaseCommandTimeout);
    IncreaseCommandTimeoutCommand = ReactiveCommand.Create(IncreaseCommandTimeout);
    DecreaseMaxDisplayedRowsCommand = ReactiveCommand.Create(DecreaseMaxDisplayedRows);
    IncreaseMaxDisplayedRowsCommand = ReactiveCommand.Create(IncreaseMaxDisplayedRows);
    SqlCheckCommand = ReactiveCommand.Create(CheckSql, CanExecuteSql);

    ClearCommand = ReactiveCommand.Create(ClearResults, CanClear);
    ExecuteCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      try
      {
        await ExecuteSqlAsync();
      }
      catch (Exception e)
      {
        await MessageBoxManager.GetMessageBoxStandard("Fehler", e.Message, ButtonEnum.Ok, Icon.Error)
          .ShowWindowDialogAsync(null!);
      }
    }, CanExecuteSql);

    CancelLoadCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      try
      {
        await CancelLoad();
      }
      catch (Exception e)
      {
        await MessageBoxManager.GetMessageBoxStandard("Fehler", e.Message, ButtonEnum.Ok, Icon.Error)
          .ShowWindowDialogAsync(null!);
      }
    }, CanCancelLoad);

    _autoExecuteTimer = new DispatcherTimer
    {
      Interval = TimeSpan.FromSeconds(1)
    };
    _autoExecuteTimer.Tick += async (_, _) =>
    {
      if (AutoExecuteEnabled && !IsExecuting) await ExecuteSqlAsync();
    };

    ConditionsGrid = new ConditionsGridViewModel();
    ConditionsGrid.ConditionsChanged += ConditionsGrid_ConditionsChanged;

    TablesGrid = new TablesGridViewModel();
    TablesGrid.TableChanged += TablesGrid_Changed;

    FieldsGrid = new FieldsGridViewModel();
    FieldsGrid.FieldChanged += FieldsGrid_FieldChanged;

    JoinsGrid = new JoinsGridViewModel();
    JoinsGrid.JoinsChanged += JoinsGrid_FieldChanged;

    HavingsGrid = new ConditionsGridViewModel
    {
      IsHaving = true
    };
    HavingsGrid.ConditionsChanged += HavingsGrid_ConditionsChanged;

    SortsGrid = new SortsGridViewModel();
    SortsGrid.FieldChanged += SortsGrid_FieldChanged;

    SqlTreeView = new SqlTreeViewModel(SqlDefinition, OnSqlRegeneratedFromTree);
    SqlTreeView.SqlTreeViewChanged += SqlTreeView_SqlTreeViewChanged;
  }

  public DbConnect? DbConnect
  {
    get;
    set
    {
      field = value;
      ExecutionDialect = field?.DbType switch
      {
        DataBaseTypes.MsSqlServer => SqlExecutionDialect.SqlServer,
        DataBaseTypes.Oracle => SqlExecutionDialect.Oracle,
        DataBaseTypes.MySql => SqlExecutionDialect.MySql,
        DataBaseTypes.Maria => SqlExecutionDialect.MariaDb,
        _ => ExecutionDialect
      };

      normalizedConnectionString = field != null
        ? NormalizeForNativeProvider(field.ConnectionString, ExecutionDialect)
        : string.Empty;
      SqlDefinition = field != null ? new DatDef(field) : null;
      SqlDefinition?.UseTopEnabled = UseTopEnabled;
      SqlDefinition?.UseSimpleDate = UseSimpleDate;
      FieldsGrid.DataDict = value!.DataDic;
      JoinsGrid.DataDict = value.DataDic;
      RefreshTabs();
    }
  }

  public string SqlText
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanExecuteSql));
    }
  } = string.Empty;

  public string StatusMessage
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = string.Empty;

  public bool UseTopEnabled
  {
    get;
    set
    {
      SqlDefinition?.UseTopEnabled = value;
      this.RaiseAndSetIfChanged(ref field, value);
    }
  }

  public bool UseSimpleDate
  {
    get;
    set
    {
      SqlDefinition?.UseSimpleDate = value;
      this.RaiseAndSetIfChanged(ref field, value);
    }
  }

  public int MaxDisplayedRows
  {
    get;
    set
    {
      var normalized = NormalizeMaxDisplayedRows(value);
      field = normalized;
      this.RaiseAndSetIfChanged(ref field, normalized);
      this.RaisePropertyChanged(nameof(MaxDisplayedRowsDisplayText));
      SqlDefinition?.MaxDisplayedRows = MaxDisplayedRows;
      if (UseTopEnabled) RegenerateSqlFromTree();
    }
  } = 300;

  public DataTreeNode CurrentSqlTreeNode
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      RefreshCurrentTab();
    }
  } = new();

  public string MaxDisplayedRowsDisplayText => MaxDisplayedRows.ToString();

  public DataTable? Results
  {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public ResultsGridViewModel ResultsGrid { get; } = new();

  public IObservable<bool> CanExecuteSql =>
    this.WhenAnyValue(x => x.SqlText).Select(s => !string.IsNullOrWhiteSpace(s));

  public IObservable<bool> CanClear => this.WhenAnyValue(x => x.SqlText).Select(s => !string.IsNullOrWhiteSpace(s));

  public bool IsExecuting
  {
    get;
    private set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanCancelLoad));
      this.RaisePropertyChanged(nameof(CancelLoadCommand));
    }
  }

  public IObservable<bool> CanCancelLoad => this.WhenAnyValue(x => x.IsExecuting).Select(isExecuting => isExecuting);

  public SqlExecutionDialect ExecutionDialect { get; set; } = SqlExecutionDialect.SqlServer;

  public bool WithNoLock
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      SqlDefinition?.WithNoLock = field;
    }
  }

  public int CommandTimeoutSeconds
  {
    get;
    set
    {
      var normalized = NormalizeCommandTimeoutSeconds(value);
      field = normalized;
      this.RaiseAndSetIfChanged(ref field, normalized);
      this.RaisePropertyChanged(nameof(CommandTimeoutDisplayText));
    }
  } = 3;

  public string CommandTimeoutDisplayText => $"{CommandTimeoutSeconds}s";

  public bool AutoExecuteEnabled
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);

      if (field)
        _autoExecuteTimer.Start();
      else
        _autoExecuteTimer.Stop();
    }
  }

  public ICommand ExecuteCommand { get; }

  public ICommand CancelLoadCommand { get; }

  public ICommand DecreaseCommandTimeoutCommand { get; }

  public ICommand IncreaseCommandTimeoutCommand { get; }

  public ICommand DecreaseMaxDisplayedRowsCommand { get; }

  public ICommand IncreaseMaxDisplayedRowsCommand { get; }

  public ICommand SqlCheckCommand { get; }

  public ICommand ClearCommand { get; }

  public SqlTreeViewModel SqlTreeView
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public DatDef? SqlDefinition { get; private set; }

  public ConditionsGridViewModel ConditionsGrid { get; }

  public ConditionsGridViewModel HavingsGrid { get; }

  public TablesGridViewModel TablesGrid { get; }

  public FieldsGridViewModel FieldsGrid
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public JoinsGridViewModel JoinsGrid
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public SortsGridViewModel SortsGrid
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  private void RefreshTabs()
  {
    TablesGrid.SqlDefinition = SqlDefinition;
    TablesGrid.Refresh();
    FieldsGrid.SqlDefinition = SqlDefinition;
    FieldsGrid.Refresh();
    ConditionsGrid.SyncFromDatDef(SqlDefinition!);
    JoinsGrid.Refresh();
    HavingsGrid.SyncFromDatDef(SqlDefinition!);
    SortsGrid.SqlDefinition = SqlDefinition;
    SortsGrid.Refresh();
    SqlTreeView.SyncFromDatDef(SqlDefinition);
  }

  private void HavingsGrid_ConditionsChanged(object? sender, EventArgs e)
  {
    RegenerateSqlFromDefinition(true);
  }

  private void SqlTreeView_SqlTreeViewChanged(object? sender, EventArgs e)
  {
    RefreshTabs();
    SqlText = string.Empty;
  }

  private void SortsGrid_FieldChanged(object? sender, EventArgs e)
  {
    SqlTreeView.SyncFromDatDef(SqlDefinition);
    RegenerateSqlFromDefinition(true);
  }

  private void FieldsGrid_FieldChanged(object? sender, EventArgs e)
  {
    SqlTreeView.SyncFromDatDef(SqlDefinition);
    RegenerateSqlFromDefinition(true);
  }

  private void JoinsGrid_FieldChanged(object? sender, EventArgs e)
  {
    SqlTreeView.SyncFromDatDef(SqlDefinition);
    RegenerateSqlFromDefinition(true);
  }

  private void TablesGrid_Changed(object? sender, EventArgs e)
  {
    SqlTreeView.SyncFromDatDef(SqlDefinition);
    RegenerateSqlFromDefinition(true);
  }

  private void ConditionsGrid_ConditionsChanged(object? sender, EventArgs e)
  {
    RegenerateSqlFromDefinition(true);
  }

  public void RefreshCurrentTab()
  {
    switch (CurrentSqlTreeNode.Kind)
    {
      case DataTreeNodeKind.Database:
        break;
      case DataTreeNodeKind.Table:
        SwitchCurrentTabToFields();
        break;
      case DataTreeNodeKind.ColumnsGroup:
      case DataTreeNodeKind.Column:
      case DataTreeNodeKind.IndexesGroup:
      case DataTreeNodeKind.Index:
      case DataTreeNodeKind.IndexColumn:
      case DataTreeNodeKind.RelationsGroup:
      case DataTreeNodeKind.Relation:
      case DataTreeNodeKind.RelationColumn:
      case DataTreeNodeKind.Select:
        break;
      case DataTreeNodeKind.SqlTablesGroup:
        SwitchCurrentTabToTables();
        break;
      case DataTreeNodeKind.SqlJoinsGroup:
      case DataTreeNodeKind.SqlWhereGroup:
      case DataTreeNodeKind.SqlGroupByGroup:
      case DataTreeNodeKind.SqlHavingGroup:
        break;
      case DataTreeNodeKind.SqlOrderByGroup:
        SwitchCurrentTabToSorts();
        break;
      case DataTreeNodeKind.SqlJoin:
      case DataTreeNodeKind.SqlCondition:
      case DataTreeNodeKind.SqlGrouping:
      case DataTreeNodeKind.SqlOrdering:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private void SwitchCurrentTabToConditions()
  {
    ConditionsGrid.SyncFromDatDef(SqlDefinition!);
  }

  private void SwitchCurrentTabToHavings()
  {
    HavingsGrid.SyncFromDatDef(SqlDefinition!);
  }


  private void SwitchCurrentTabToTables()
  {
    TablesGrid.Refresh();
  }

  private void SwitchCurrentTabToSorts()
  {
    SortsGrid.FieldChanged -= SortsGrid_FieldChanged;
    SortsGrid = new SortsGridViewModel
    {
      SqlDefinition = SqlDefinition
    };
    SortsGrid.FieldChanged += SortsGrid_FieldChanged;
    SortsGrid.Refresh();
  }

  private void SwitchCurrentTabToFields()
  {
    FieldsGrid.FieldChanged -= FieldsGrid_FieldChanged;
    FieldsGrid = new FieldsGridViewModel
    {
      SqlDefinition = SqlDefinition,
      DataDict = DbConnect!.DataDic
    };

    FieldsGrid.Refresh();
    FieldsGrid.FieldChanged += FieldsGrid_FieldChanged;
  }

  private void SwitchCurrentTabToJoins()
  {
    JoinsGrid.JoinsChanged -= JoinsGrid_FieldChanged;
    JoinsGrid = new JoinsGridViewModel
    {
      SqlDefinition = SqlDefinition,
      DataDict = DbConnect!.DataDic
    };

    JoinsGrid.Refresh();
    JoinsGrid.JoinsChanged += JoinsGrid_FieldChanged;
  }

  public void RefreshCurrentTabByMaintab(int index)
  {
    switch (index)
    {
      case 0: // SQL
        break;
      case 1: // Tables
        SwitchCurrentTabToTables();
        break;
      case 2: // Fields
        SwitchCurrentTabToFields();
        break;
      case 3: // Joins
        SwitchCurrentTabToJoins();
        break;
      case 4: // Wheres
        SwitchCurrentTabToConditions();
        break;
      case 5: // Havings
        SwitchCurrentTabToHavings();
        break;
      case 6: // Sorts
        SwitchCurrentTabToSorts();
        break;
    }
  }


  private void OnSqlRegeneratedFromTree()
  {
    RegenerateSqlFromDefinition();
  }

  public void RegenerateSqlFromTree()
  {
    SqlTreeView.RegenerateSqlText();
  }

  public async Task ExecuteSqlAsync()
  {
    if (IsExecuting) return;

    if (string.IsNullOrWhiteSpace(SqlText))
    {
      StatusMessage = "Keine ausführbare SQL-Anweisung.";
      return;
    }

    IsExecuting = true;
    StatusMessage = string.Concat(
      "Führe SQL aus ...",
      Environment.NewLine,
      SqlText.TrimEnd(),
      Environment.NewLine);

    await using var con = CreateNativeExecutionConnection();

    try
    {
      var stopwatch = Stopwatch.StartNew();
      var executeStart = DateTime.Now;

      Results = null;
      ResultsGrid.ColumnNames = [];
      ResultsGrid.SetRows([]);

      var sqlToExecute = SqlText;
      var limitRows = UseTopEnabled;
      var maxRows = MaxDisplayedRows;
      var timeoutSeconds = CommandTimeoutSeconds;

      using var manualCancelCts = new CancellationTokenSource();
      using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
      using var executionCts = CancellationTokenSource.CreateLinkedTokenSource(manualCancelCts.Token, timeoutCts.Token);
      SetCurrentManualExecutionCts(manualCancelCts);

      var loadResult = await LoadQueryResultAsync(
        con,
        sqlToExecute,
        timeoutSeconds,
        limitRows,
        maxRows,
        executionCts.Token,
        manualCancelCts.Token);

      Results = loadResult.Table;
      ResultsGrid.ColumnNames = [.. loadResult.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)];
      ResultsGrid.SetRows(loadResult.Rows);
      this.RaisePropertyChanged(nameof(ResultsGrid.Rows));

      stopwatch.Stop();
      var executeEnd = DateTime.Now;

      var summary = loadResult.CancellationReason switch
      {
        QueryCancellationReason.Manual => $"Laden manuell abgebrochen. Zeilen: {loadResult.RowCount}.",
        QueryCancellationReason.Timeout =>
          $"Timeout nach {CommandTimeoutSeconds} Sekunden. Zeilen: {loadResult.RowCount}.",
        _ => loadResult.LimitRows
          ? $"Abfrage ausgeführt. Zeilen: {loadResult.RowCount} (max {loadResult.MaxRows})."
          : $"Abfrage ausgeführt. Zeilen: {loadResult.RowCount}."
      };

      StatusMessage = BuildExecuteStatusMessage(executeStart, executeEnd, stopwatch.Elapsed, summary);
    }
    catch (Exception ex)
    {
      Results = null;
      ResultsGrid.SetRows([]);
      this.RaisePropertyChanged(nameof(ResultsGrid.Rows));
      StatusMessage = "Fehler bei der SQL-Ausführung: " + ex.Message;
    }
    finally
    {
      ClearCurrentManualExecutionCts();
      IsExecuting = false;
    }
  }

  private async Task CancelLoad()
  {
    CancellationTokenSource? cts;
    lock (_executionStateSync)
    {
      cts = _currentManualExecutionCts;
    }

    await cts?.CancelAsync()!;
  }

  private void SetCurrentManualExecutionCts(CancellationTokenSource cts)
  {
    lock (_executionStateSync)
    {
      _currentManualExecutionCts?.Dispose();
      _currentManualExecutionCts = cts;
    }
  }

  private void ClearCurrentManualExecutionCts()
  {
    lock (_executionStateSync)
    {
      _currentManualExecutionCts?.Dispose();
      _currentManualExecutionCts = null;
    }
  }

  private static async Task<QueryLoadResult> LoadQueryResultAsync(
    DbConnection con,
    string sql,
    int timeoutSeconds,
    bool limitRows,
    int maxRows,
    CancellationToken cancellationToken,
    CancellationToken manualCancellationToken)
  {
    var table = new DataTable();
    var bufferedRows = new List<Dictionary<string, object?>>();
    var rowCount = 0;

    await con.OpenAsync(cancellationToken).ConfigureAwait(false);
    await using var cmd = con.CreateCommand();
    cmd.CommandText = sql;
    cmd.CommandTimeout = timeoutSeconds;

    await using var cancellationRegistration = cancellationToken.Register(() =>
    {
      try
      {
        // ReSharper disable once AccessToDisposedClosure
        cmd.Cancel();
      }
      catch (ObjectDisposedException)
      {
      }
      catch (InvalidOperationException)
      {
      }

      try
      {
        if (con.State != ConnectionState.Closed)
          con.Close();
      }
      catch (ObjectDisposedException)
      {
      }
      catch (InvalidOperationException)
      {
      }
    });

    try
    {
      await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken)
        .ConfigureAwait(false);

      var fieldCount = reader.FieldCount;
      var names = new string[fieldCount];
      for (var i = 0; i < fieldCount; i++)
      {
        names[i] = reader.GetName(i);
        table.Columns.Add(names[i], reader.GetFieldType(i));
      }

      var values = new object[fieldCount];

      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && (!limitRows || rowCount < maxRows))
      {
        cancellationToken.ThrowIfCancellationRequested();

        reader.GetValues(values);
        var rowValues = (object[])values.Clone(); // isolate per-row values
        table.Rows.Add(rowValues);

        var dict = new Dictionary<string, object?>(fieldCount);
        for (var i = 0; i < fieldCount; i++) dict[names[i]] = rowValues[i];

        bufferedRows.Add(dict);
        rowCount++;
      }
    }
    catch (OperationCanceledException)
    {
      var reason = manualCancellationToken.IsCancellationRequested
        ? QueryCancellationReason.Manual
        : QueryCancellationReason.Timeout;

      return new QueryLoadResult(table, bufferedRows, rowCount, limitRows, maxRows, reason);
    }

    return new QueryLoadResult(table, bufferedRows, rowCount, limitRows, maxRows, QueryCancellationReason.None);
  }

  public void CheckSql()
  {
    if (IsExecuting) return;

    IsExecuting = true;
    StatusMessage = "Prüfe SQL-Syntax ...";

    if (string.IsNullOrWhiteSpace(SqlText))
    {
      StatusMessage = "Keine prüfbare SQL-Anweisung.";
      IsExecuting = false;
      return;
    }

    using var con = CreateNativeExecutionConnection();

    try
    {
      con.Open();
      using var cmd = con.CreateCommand();
      var sqlToCheck = SqlText.Trim().TrimEnd(';');

      cmd.CommandText = ExecutionDialect switch
      {
        SqlExecutionDialect.Oracle => "EXPLAIN PLAN FOR " + sqlToCheck,
        _ => "SET NOEXEC ON; " + sqlToCheck + "; SET NOEXEC OFF;"
      };
      cmd.CommandTimeout = CommandTimeoutSeconds;

      cmd.ExecuteNonQuery();
      StatusMessage = "SQL-Syntaxprüfung erfolgreich.";
    }
    catch (DbException ex)
    {
      StatusMessage = "SQL-Syntaxfehler: " + ex.Message;
    }
    catch (InvalidOperationException ex)
    {
      StatusMessage = "SQL-Prüfung fehlgeschlagen: " + ex.Message;
    }
    finally
    {
      IsExecuting = false;
    }
  }

  private DbConnection CreateNativeExecutionConnection()
  {
    return ExecutionDialect switch
    {
      SqlExecutionDialect.Oracle => new OracleConnection(normalizedConnectionString),
      SqlExecutionDialect.MySql or SqlExecutionDialect.MariaDb => new MySqlConnection(normalizedConnectionString),
      _ => new SqlConnection(normalizedConnectionString)
    };
  }

  private static string NormalizeForNativeProvider(string connectionString, SqlExecutionDialect dialect)
  {
    return dialect switch
    {
      SqlExecutionDialect.Oracle => NormalizeForOracle(connectionString),
      SqlExecutionDialect.MySql or SqlExecutionDialect.MariaDb => connectionString,
      _ => NormalizeForSqlServer(connectionString)
    };
  }

  private static string NormalizeForSqlServer(string connectionString)
  {
    var source = new DbConnectionStringBuilder { ConnectionString = connectionString };
    var target = new SqlConnectionStringBuilder
    {
      // Required in this environment so SQL Server connections can be established
      // even when the server certificate chain is not trusted locally.
      TrustServerCertificate = true
    };

    if (TryGet(source, out var value, "Data Source", "DataSource", "Server", "Address", "Addr", "Network Address",
          "DSN")) target.DataSource = value;

    if (TryGet(source, out value, "Initial Catalog", "Database", "DbName")) target.InitialCatalog = value;

    if (TryGet(source, out value, "User ID", "UID", "UserId")) target.UserID = value;

    if (TryGet(source, out value, "Password", "PassWord", "PWD")) target.Password = value;

    if (TryGet(source, out value, "Integrated Security", "Trusted_Connection"))
      if (bool.TryParse(value, out var integrated))
        target.IntegratedSecurity = integrated;

    if (TryGet(source, out value, "Connect Timeout", "Connection Timeout"))
      if (int.TryParse(value, out var timeout))
        target.ConnectTimeout = timeout;

    if (!TryGet(source, out value, "Trust Server Certificate", "TrustServerCertificate"))
      return target.ConnectionString;
    if (bool.TryParse(value, out var trustServerCertificate)) target.TrustServerCertificate = trustServerCertificate;

    return target.ConnectionString;
  }

  private static string NormalizeForOracle(string connectionString)
  {
    var source = new DbConnectionStringBuilder { ConnectionString = connectionString };
    var target = new OracleConnectionStringBuilder();

    if (TryGet(source, out var value, "Data Source", "DataSource", "Server", "DSN")) target.DataSource = value;

    if (TryGet(source, out value, "User ID", "UID", "UserId")) target.UserID = value;

    if (TryGet(source, out value, "Password", "PassWord", "PWD")) target.Password = value;

    return target.ConnectionString;
  }

  private static bool TryGet(DbConnectionStringBuilder builder, out string value, params string[] keys)
  {
    foreach (var key in keys)
    {
      if (!builder.TryGetValue(key, out var raw)) continue;
      value = raw.ToString() ?? string.Empty;
      if (!string.IsNullOrWhiteSpace(value)) return true;
    }

    value = string.Empty;
    return false;
  }

  private static int NormalizeCommandTimeoutSeconds(int timeout)
  {
    var clamped = Math.Clamp(timeout, 3, 120);
    return clamped;
  }

  private static int NormalizeMaxDisplayedRows(int rows)
  {
    var clamped = Math.Clamp(rows, 100, 5000);
    return clamped / 100 * 100;
  }

  private void DecreaseCommandTimeout()
  {
    CommandTimeoutSeconds -= 3;
  }

  private void IncreaseCommandTimeout()
  {
    CommandTimeoutSeconds += 3;
  }

  private void DecreaseMaxDisplayedRows()
  {
    MaxDisplayedRows -= 100;
  }

  private void IncreaseMaxDisplayedRows()
  {
    MaxDisplayedRows += 100;
  }

  private static string BuildExecuteStatusMessage(DateTime start, DateTime end, TimeSpan duration, string summary)
  {
    return string.Concat(
      "Start: ", start.ToString("HH:mm:ss.fff"), Environment.NewLine,
      "Ende: ", end.ToString("HH:mm:ss.fff"), Environment.NewLine,
      "Dauer: ", duration.Days, " Tage, ",
      duration.Hours, " Stunden, ",
      duration.Minutes, " Minuten, ",
      duration.Seconds, " Sekunden, ",
      duration.Milliseconds, " Millisekunden", Environment.NewLine,
      summary);
  }

  private void ClearResults()
  {
    SqlDefinition?.Clear();
    Results = null;
    ResultsGrid.SetRows([]);
    this.RaisePropertyChanged(nameof(ResultsGrid.Rows));
    StatusMessage = string.Empty;
    SqlText = string.Empty;
    RefreshTabs();
  }

  public void AddTableToDefinition(DataTreeNode node, bool addAllFields = true, bool clearTableBefore = false)
  {
    if (SqlDefinition == null || string.IsNullOrWhiteSpace(node.Name)) return;

    if (clearTableBefore) SqlDefinition.TablesClear();

    var table = SqlDefinition.TablesAdd(string.Empty, node.Name, string.Empty, node.Tag);
    if (table != null && addAllFields) table.FieldsAdd("*");
    RegenerateSqlFromDefinition();
    RefreshTabs();
  }

  public void AddTableToDefinition(string tableName, bool addAllFields = true)
  {
    if (SqlDefinition == null || string.IsNullOrWhiteSpace(tableName)) return;

    var table = SqlDefinition.TablesAdd(string.Empty, tableName, string.Empty);
    if (table != null && addAllFields) table.FieldsAdd("*");
    RegenerateSqlFromDefinition();
    RefreshTabs();
  }

  public void AddJoinFromRelation(JoinTypes joinType, DictRelation? relation, bool revert = false)
  {
    if (SqlDefinition == null || relation == null) return;

    var leftTable = revert ? relation.ForeignTableName : relation.TableName;
    var rightTable = revert ? relation.TableName : relation.ForeignTableName;

    if (string.IsNullOrWhiteSpace(leftTable) || string.IsNullOrWhiteSpace(rightTable)) return;

    var fromAlias = string.Empty;
    var toAlias = string.Empty;

    var countfrom = SqlDefinition.JoinsAll.Count(x => x.From == leftTable);
    if (countfrom > 0 && SqlDefinition.Tables.Any(x => x.Name == leftTable) is false) fromAlias = leftTable + countfrom;

    var countTo = SqlDefinition.JoinsAll.Count(x => x.To == rightTable);
    if (countTo > 0) toAlias = rightTable + countTo;

    var leftTabSuchen = fromAlias.Length > 0 ? fromAlias : leftTable;
    var rightTabSuchen = toAlias.Length > 0 ? toAlias : rightTable;

    var leftTableDef = SqlDefinition[leftTabSuchen] ?? SqlDefinition.TablesAdd(string.Empty, leftTable, fromAlias);

    var rightTableDef = SqlDefinition[rightTabSuchen] ?? SqlDefinition.TablesAdd(string.Empty, rightTable, toAlias);

    var leftKeyColumn = string.Empty;
    var rightKeyColumn = string.Empty;

    for (var i = 0; i < relation.ColumnsCount; i++)
    {
      var column = relation[i];
      if (column == null || string.IsNullOrWhiteSpace(column.ColName) ||
          string.IsNullOrWhiteSpace(column.ForeignColName)) continue;

      var fromCol = column.ColName;
      var toCol = column.ForeignColName;

      if (revert) (fromCol, toCol) = (toCol, fromCol);

      if (string.IsNullOrWhiteSpace(leftKeyColumn))
      {
        leftKeyColumn = fromCol;
        rightKeyColumn = toCol;
      }

      if (leftTableDef != null && leftTableDef[fromCol] == null) leftTableDef.FieldsAdd(fromCol);

      if (rightTableDef != null && rightTableDef[toCol] == null) rightTableDef.FieldsAdd(toCol);
    }

    SqlDefinition.JoinsAdd(joinType, leftTable, leftKeyColumn, rightTable, rightKeyColumn, string.Empty, string.Empty,
      fromAlias, string.Empty, toAlias, string.Empty);
    RegenerateSqlFromDefinition();
    RefreshTabs();
  }

  public void AddSortToDefinition(string tableName, string columnName)
  {
    if (SqlDefinition == null || string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(columnName)) return;

    SqlDefinition.SortsAdd(tableName, columnName, SortTypes.Asc);
    RegenerateSqlFromDefinition();
    RefreshTabs();
  }

  public void AddColumnToDefinition(string tableName, string columnName, object? tag = null)
  {
    if (SqlDefinition == null || string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(columnName)) return;

    var table = SqlDefinition[tableName];
    if (table == null)
    {
      AddTableToDefinition(tableName, false);
      table = SqlDefinition[tableName];
    }

    var alias = string.Empty;
    for (var t = 0; t < SqlDefinition.TablesCount; t++)
    {
      var otherTable = SqlDefinition[t];
      if (otherTable == null || string.Equals(otherTable.Key, tableName, StringComparison.OrdinalIgnoreCase)) continue;

      if (otherTable[columnName] == null) continue;
      alias = $"{tableName}_{columnName}";
      break;
    }

    if (string.IsNullOrWhiteSpace(alias))
      table?.FieldsAdd(columnName);
    else
      table?.FieldsAdd(columnName, alias);

    table?[table.FieldsCount - 1]?.Tag = tag;

    RegenerateSqlFromDefinition();
    RefreshTabs();
  }

  public void RegenerateSqlFromDefinition(bool isSyncFromConditions = false)
  {
    if (SqlDefinition == null) return;

    try
    {
      var generatedSql = SqlDefinition.TablesCount > 0 ? SqlDefinition.SqlString : string.Empty;
      if (string.IsNullOrWhiteSpace(generatedSql)) return;
      SqlText = generatedSql;
    }
    catch (Exception ex)
    {
      StatusMessage = $"SQL Generation Error: {ex.Message}";
    }
    finally
    {
      SqlTreeView.SyncFromDatDef(SqlDefinition);
    }
  }

  private sealed record QueryLoadResult(
    DataTable Table,
    List<Dictionary<string, object?>> Rows,
    int RowCount,
    bool LimitRows,
    int MaxRows,
    QueryCancellationReason CancellationReason);

  private enum QueryCancellationReason
  {
    None,
    Manual,
    Timeout
  }
}