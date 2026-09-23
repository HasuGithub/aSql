using System.Data.Common;
using System.Text;

using aDataLib.Formatter;

using Microsoft.Data.SqlClient;

namespace aDataLib;

public sealed class DatDef
{
  private List<DatDefTable.DatDefTableField> _allFields = null!;
  private string _brackClose = string.Empty;
  private string _brackOpen = string.Empty;
  private List<DatDefCond> _conds = null!;
  private bool _debugMode;
  private string _debugString = "";
  private List<DatDefCond> _groupConds = null!;
  private Dictionary<string, int> _groupIndex = null!;
  private List<DatDefGroup> _groups = null!;
  private Dictionary<string, int> _joinIndex = null!;
  private Dictionary<string, int> _sortIndex = null!;
  private StringBuilder _sql = null!;
  private SqlTypes _sqlType;
  private Dictionary<string, int> _tableIndex = null!;
  internal DbCommand? dbCmd;

  public DatDef(DbConnect dbConnect)
  {
    Init(dbConnect);
  }

  public DatDef(string schema, string firstTable, DbConnect dbConnect)
  {
    Init(dbConnect);
    TablesAdd(schema, firstTable, "");
  }

  public bool UseExceptionsOnWrite { get; set; }

  public bool DebugMode
  {
    get => _debugMode;
    set
    {
      _debugMode = value;
      _debugString = "";
    }
  }

  public Dictionary<string, DatDefTable.DatDefTableField> AllFields { get; private set; } = null!;

  public bool SelectIsDistinct { get; set; }

  public string ParseErrText { get; private set; } = string.Empty;
  public int ParseErrPos { get; private set; }

  public bool WithNoLock { get; set; }
  public bool UseTopEnabled { get; set; }

  public bool UseSimpleDate { get; set; }

  public int MaxDisplayedRows { get; set; } = 300;

  public string SqlString
  {
    get
    {
      BuildSql();
      WriteDebugMessage("<debug mode> SqlString:\n" + _sql + "\n");
      return _sql.ToString();
    }
  }

  public DbConnect DbConnect { get; set; } = null!;

  public int TablesCount => Tables.Count;

  public DatDefTable? this[string tableKey]
  {
    get
    {
      if (string.IsNullOrWhiteSpace(tableKey)) return null;
      return _tableIndex.TryGetValue(tableKey, out var idx) ? Tables[idx] : null;
    }
  }

  public DatDefTable? this[int index]
    => index < 0 || index >= Tables.Count ? null : Tables[index];

  public int JoinsCount => JoinsAll.Count;

  public int CondsCount => _conds.Count;

  public int SortsCount => SortsList.Count;

  public int GroupsCount => _groups.Count;

  public int GroupCondsCount => _groupConds.Count;

  public IReadOnlyDictionary<string, int> LHtTables => _tableIndex;

  public List<DatDefTable> Tables { get; private set; } = null!;

  public List<DatDefSort> SortsList { get; private set; } = null!;

  public List<DatDefJoin> JoinsAll { get; private set; } = null!;

  private void Init(DbConnect dbConnect)
  {
    _sqlType = SqlTypes.Select;
    DbConnect = dbConnect;
    dbCmd?.Dispose();
    dbCmd = DbConnect.Connection!.CreateCommand();
    _brackOpen = DbConnect.SqlBrackOpenSign;
    _brackClose = DbConnect.SqlBrackCloseSign;
    Tables = [];
    _tableIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    _allFields = [];
    AllFields = new Dictionary<string, DatDefTable.DatDefTableField>(StringComparer.OrdinalIgnoreCase);
    JoinsAll = [];
    _joinIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    SortsList = [];
    _sortIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    _groups = [];
    _groupIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    _conds = [];
    _groupConds = [];
    _sql = new StringBuilder();
    _debugMode = false;
  }

  public void Clear()
  {
    _sql.Length = 0;
    TablesClear();
    JoinsClear();
    GroupsClear();
    SortsClear();
    CondsClear();
    GroupCondsClear();
    ParseErrText = "";
    ParseErrPos = 0;
    SelectIsDistinct = false;
  }

  internal int ExecuteWrite(Func<string> sqlFactory, string operationName)
  {
    ArgumentNullException.ThrowIfNull(sqlFactory);
    try
    {
      var sql = sqlFactory();
      return Utility.DbExecute(dbCmd!, sql, DebugMode, ref _debugString);
    }
    catch (SqlException ex)
    {
      WriteDebugMessage("<error> " + operationName + ": " + ex.Message);
      if (UseExceptionsOnWrite) throw;
      return -1;
    }
    catch (InvalidOperationException ex)
    {
      WriteDebugMessage("<error> " + operationName + ": " + ex.Message);
      if (UseExceptionsOnWrite) throw;
      return -1;
    }
  }

  internal void WriteDebugMessage(string message)
  {
    if (!_debugMode || string.IsNullOrWhiteSpace(message)) return;
    _debugString += message;
    if (!message.EndsWith("\n", StringComparison.Ordinal)) _debugString += "\n";
  }

  internal void AllFieldsRefresh()
  {
    _allFields.Clear();
    AllFields.Clear();
    var num = 0;
    for (var i = 0; i < TablesCount; i++)
    {
      var table = this[i];
      if (table == null || table.isDisposing) continue;
      for (var j = 0; j < table.FieldsCount; j++)
      {
        var field = table[j];
        if (field == null) continue;
        field.MyAllIndex = num;
        _allFields.Add(field);
        AllFields.TryAdd(field.Key, field);
        num++;
      }
    }
  }

  private void BuildSql()
  {
    _brackOpen = DbConnect.SqlBrackOpenSign;
    _brackClose = DbConnect.SqlBrackCloseSign;
    _sql.Length = 0;
    BuildSqlAddSqlTypePart(_sql, _sqlType);
    if (_sqlType == SqlTypes.Select) BuildSqlAddSelectFields(_sql);
    if (JoinsCount == 0 && TablesCount == 0) return;
    BuildSqlFromClause(_sql);
    BuildSqlWriteClause(_sql);
    if (_sqlType != SqlTypes.Insert) BuildSqlAddWhere(_sql, false);
    if (_sqlType == SqlTypes.Select) BuildSqlAddGroupAndOrder(_sql);
    _sql.Append(UseTopEnabled && DbConnect.DbType is DataBaseTypes.Oracle
      ? $"FETCH FIRST {MaxDisplayedRows} ROWS ONLY"
      : string.Empty);
    _sql.Append(UseTopEnabled && DbConnect.DbType is DataBaseTypes.MySql or DataBaseTypes.Maria
      ? $"LIMIT {MaxDisplayedRows}"
      : string.Empty);
  }

  private void BuildSqlFromClause(StringBuilder sql)
  {
    if (JoinsCount > 0)
      BuildSqlAddJoinSections(sql);
    else
      for (var i = 0; i < TablesCount; i++)
        BuildSqlAddSqlTablePart(sql, _sqlType, i == 0, this[i]!.Name, this[i]!.Alias);
  }

  private void BuildSqlWriteClause(StringBuilder sql)
  {
    switch (_sqlType)
    {
      case SqlTypes.Insert:
        BuildSqlAddInsertPart(sql, false, 0);
        break;
      case SqlTypes.Update:
        BuildSqlAddUpDatePart(sql, false, false, 0);
        break;
      case SqlTypes.Select:
      case SqlTypes.Delete:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private void BuildSqlAddJoinSections(StringBuilder sql)
  {
    BuildSqlAddJoinSectionsForSqlServerOrAccess(sql);
  }

  private void BuildSqlAddJoinSectionsForSqlServerOrAccess(StringBuilder sql)
  {
    if (JoinsCount <= 0) return;
    sql.Append(DbConnect.UseUpperCaseSql ? "FROM\n" : "From\n");
    sql.Append(' ', JoinsCount - 1);
    sql.Append(' ', JoinsCount - 1);

    var text = "";
    var b = "";

    for (var k = 0; k < JoinsCount; k++)
    {
      var fromTable = this[JoinsAll[k].From];
      var text4 = fromTable?.Schema.Length > 0
        ? $"{_brackOpen}{fromTable.Schema}{_brackClose}."
        : "";

      var toTable = this[JoinsAll[k].To];
      var text5 = toTable?.Schema.Length > 0
        ? $"{_brackOpen}{toTable.Schema}{_brackClose}."
        : "";

      string text6, text7;
      if (JoinsAll[k].FromAlias.Length > 0)
      {
        text6 = $"{text4}{_brackOpen}{JoinsAll[k].From}{_brackClose} {_brackOpen}{JoinsAll[k].FromAlias}{_brackClose}";
        text7 = $"{_brackOpen}{JoinsAll[k].FromAlias}{_brackClose}";
      }
      else
      {
        text6 = $"{text4}{_brackOpen}{JoinsAll[k].From}{_brackClose}";
        text7 = $"{_brackOpen}{JoinsAll[k].From}{_brackClose}";
      }

      string text8, text9;
      if (JoinsAll[k].ToAlias.Length > 0)
      {
        text8 = $"{text5}{_brackOpen}{JoinsAll[k].To}{_brackClose} {_brackOpen}{JoinsAll[k].ToAlias}{_brackClose}";
        text9 = $"{_brackOpen}{JoinsAll[k].ToAlias}{_brackClose}";
      }
      else
      {
        text8 = $"{text5}{_brackOpen}{JoinsAll[k].To}{_brackClose}";
        text9 = $"{_brackOpen}{JoinsAll[k].To}{_brackClose}";
      }

      if (WithNoLock)
      {
        text6 += " WITH(NOLOCK)";
        text8 += " WITH(NOLOCK)";
      }

      var text3 = GetJoinKeyword(JoinsAll[k].JoinType, true);

      if (text9 == b)
      {
        text = $"{text} {text3} {text8} ON ";
        b = text7;
      }
      else if (k == 0)
      {
        text = $"{text}{text6} {text3} {text8} ON ";
        b = text9;
      }
      else
      {
        var flag = false;
        for (var l = 0; l < k; l++)
        {
          var a = JoinsAll[l].FromAlias.Length > 0
            ? $"{_brackOpen}{JoinsAll[l].FromAlias}{_brackClose}"
            : $"{_brackOpen}{JoinsAll[l].From}{_brackClose}";
          if (a != text9) continue;
          flag = true;
          break;
        }

        text3 = GetJoinKeyword(JoinsAll[k].JoinType, true);
        text = flag
          ? $"{text} {text3} {text6} ON "
          : $"{text} {text3} {text8} ON ";
        b = flag ? text7 : text9;
      }

      for (var m = 0; m < JoinsAll[k].FieldsCount; m++)
      {
        if (m > 0) text += " AND ";
        var text2 = GetComparisonOperator(JoinsAll[k][m]!.CompType, false);
        text +=
          $" ( {text4}{text7}.{_brackOpen}{JoinsAll[k][m]!.From}{_brackClose} {text2} {text5}{text9}.{_brackOpen}{JoinsAll[k][m]!.To}{_brackClose} ) ";
      }

      if (k < JoinsCount - 1) text += " ";
      text += "\n";
    }

    sql.Append(DbConnect.UseUpperCaseSql ? text.ToUpper() : text);
  }

  private static string GetComparisonOperator(CompTypes compType, bool accessStyleUnequal)
  {
    return compType switch
    {
      CompTypes.Equal => " = ",
      CompTypes.Unequal => accessStyleUnequal ? " <> " : " != ",
      CompTypes.Greater => " > ",
      CompTypes.Smaller => " < ",
      CompTypes.GreaterEqual => " >= ",
      CompTypes.SmallerEqual => " <= ",
      CompTypes.Like => " Like ",
      _ => " = "
    };
  }

  private static string GetJoinKeyword(JoinTypes joinType, bool compact)
  {
    if (compact)
      return joinType switch
      {
        JoinTypes.RightJoin => "RIGHT JOIN",
        JoinTypes.LeftJoin => "LEFT JOIN",
        _ => "INNER JOIN"
      };
    return joinType switch
    {
      JoinTypes.LeftJoin => " LEFT OUTER JOIN ",
      JoinTypes.RightJoin => " RIGHT OUTER JOIN ",
      _ => " NATURAL JOIN "
    };
  }

  private void BuildSqlAddSelectFields(StringBuilder sql)
  {
    var isFirstField = true;
    for (var i = 0; i < TablesCount; i++)
    {
      var table = this[i]!;
      for (var j = 0; j < table.FieldsCount; j++)
      {
        if (DbConnect.UseUpperCaseSql)
          BuildSqlAddFieldPart(sql, isFirstField, table.Schema.ToUpper(), table.Name.ToUpper(), table.Alias.ToUpper(),
            table[j]!.FieldAgg, table[j]!.Name.ToUpper(), table[j]!.Alias.ToUpper());
        else
          BuildSqlAddFieldPart(sql, isFirstField, table.Schema, table.Name, table.Alias, table[j]!.FieldAgg,
            table[j]!.Name, table[j]!.Alias);
        isFirstField = false;
      }
    }
  }

  private void BuildSqlAddGroupAndOrder(StringBuilder sql)
  {
    if (GroupsCount > 0)
    {
      sql.Append(DbConnect.UseUpperCaseSql ? "GROUP BY\n" : "Group By\n");
      for (var i = 0; i < GroupsCount; i++)
      {
        sql.Append(i > 0 ? ", " : "  ");
        var table = this[_groups[i].Table];
        if (table?.Schema.Length > 0)
          sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? table.Schema.ToUpper() : table.Schema)
            .Append(_brackClose).Append('.');
        sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? _groups[i].Table.ToUpper() : _groups[i].Table)
          .Append(_brackClose)
          .Append('.').Append(_brackOpen)
          .Append(DbConnect.UseUpperCaseSql ? _groups[i].Field.ToUpper() : _groups[i].Field).Append(_brackClose)
          .Append('\n');
      }
    }

    BuildSqlAddWhere(sql, true);

    if (SortsCount <= 0) return;

    sql.Append(DbConnect.UseUpperCaseSql ? "ORDER BY\n" : "Order By\n");
    for (var i = 0; i < SortsCount; i++)
    {
      sql.Append(i > 0 ? ", " : "  ");
      var sort = SortsList[i];
      AppendAggregatePrefix(sql, sort.FieldAgg);
      var table = this[sort.Table];
      if (table?.Schema.Length > 0)
        sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? table.Schema.ToUpper() : table.Schema)
          .Append(_brackClose).Append('.');
      sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? sort.Table.ToUpper() : sort.Table).Append(_brackClose)
        .Append('.').Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? sort.Field.ToUpper() : sort.Field)
        .Append(_brackClose);
      if (sort.FieldAgg != AggregateTypes.Nothing) sql.Append(')');
      var sType = sort.Type == SortTypes.Asc ? " Asc\n" : " Desc\n";
      sql.Append(DbConnect.UseUpperCaseSql ? sType.ToUpper() : sType);
    }
  }

  private void AppendAggregatePrefix(StringBuilder sql, AggregateTypes aggregateType)
  {
    var sqpPre = new StringBuilder();
    switch (aggregateType)
    {
      case AggregateTypes.Count: sqpPre.Append("Count("); break;
      case AggregateTypes.Sum: sqpPre.Append("Sum("); break;
      case AggregateTypes.Min: sqpPre.Append("Min("); break;
      case AggregateTypes.Max: sqpPre.Append("Max("); break;
      case AggregateTypes.Avg: sqpPre.Append("Avg("); break;
      case AggregateTypes.Nothing:
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(aggregateType), aggregateType, null);
    }

    if (DbConnect.UseUpperCaseSql)
      sql.Append(sqpPre.ToString().ToUpper());
    else
      sql.Append(sqpPre);
  }

  internal void BuildSqlAddWhere(StringBuilder sql, bool isGroup)
  {
    var count = isGroup ? GroupCondsCount : CondsCount;
    if (count <= 0) return;
    for (var i = 0; i < count; i++)
    {
      var cond = isGroup ? _groupConds[i] : _conds[i];
      if (!cond.UseMe) continue;
      BuildSqlAddWherePart(sql, isGroup, new WhereConditionArgs(
        cond.OpType, cond.BracksOpen, cond.Table, cond.FieldAgg,
        cond.Field, cond.LeftCompValue, ResolveConditionFieldType(cond),
        cond.CompType, cond.CompValue, cond.CompTable, cond.CompFieldAgg,
        cond.CompField, cond.BracksClose, cond.CompValueList));
    }
  }

  private DbFieldType ResolveConditionFieldType(DatDefCond datDefCond)
  {
    var dbFieldType = datDefCond.FieldType;
    if (dbFieldType != DbFieldType.Unknown) return dbFieldType;

    var datDefTable = datDefCond.Table.Length > 0 ? this[datDefCond.Table] : this[datDefCond.CompTable];
    var datDefField = datDefCond.Field.Length > 0
      ? datDefTable?[datDefCond.Field]
      : datDefTable?[datDefCond.CompField];
    if (datDefField != null) dbFieldType = datDefField.FieldType;

    var dictTable = datDefCond.Table.Length > 0
      ? DbConnect.DataDic[datDefCond.Table]
      : DbConnect.DataDic[datDefCond.CompTable];
    if (dictTable == null && datDefTable != null)
      dictTable = DbConnect.DataDic[datDefTable.Name];
    if (dictTable == null) return dbFieldType;

    var dictColumn = datDefCond.Field.Length > 0
      ? dictTable[datDefCond.Field]
      : dictTable[datDefCond.CompField];
    return dictColumn?.FieldType ?? dbFieldType;
  }

  internal void BuildSqlAddUpDatePart(StringBuilder sql, bool takeOnlyChangedFields, bool useParameterSyntax,
    int tabIndex)
  {
    var num = 0;
    sql.Append("Set \n");
    for (var i = 0; i < this[tabIndex]!.FieldsCount; i++)
    {
      var field = this[tabIndex]![i]!;
      if (field.IsReadOnly || (takeOnlyChangedFields && Equals(field.Value, field.ValueOld))) continue;
      sql.Append(num == 0 ? "  " : ", ");
      if (useParameterSyntax)
      {
        sql.Append(_brackOpen).Append(this[tabIndex]!.Name).Append(_brackClose)
          .Append('.').Append(_brackOpen).Append(field.Name).Append(_brackClose)
          .Append(" = ?");
      }
      else
      {
        var t = this[this[tabIndex]!.Name];
        if (t?.Schema.Length > 0)
          sql.Append($"{_brackOpen}{t.Schema}{_brackClose}.");
        sql.Append(_brackOpen).Append(this[tabIndex]!.Name).Append(_brackClose)
          .Append('.').Append(_brackOpen).Append(field.Name).Append(_brackClose)
          .Append(" = ");
        BuildSqlAppendFieldValue(sql, field.FieldType, field.Value, UseSimpleDate);
        sql.Append('\n');
      }

      num++;
    }

    sql.Append('\n');
  }

  internal void BuildSqlAddInsertPart(StringBuilder sql, bool useParameterSyntax, int tabIndex)
  {
    sql.Append('(');
    for (var i = 0; i < this[tabIndex]!.FieldsCount; i++)
    {
      var field = this[tabIndex]![i]!;
      if (field.IsReadOnly) continue;
      sql.Append(i == 0 ? "  " : ", ");
      if (useParameterSyntax)
      {
        sql.Append(_brackOpen).Append(field.Name).Append(_brackClose);
      }
      else
      {
        if (DbConnect.DbType != DataBaseTypes.MsAccess)
        {
          var t = this[this[tabIndex]!.Name];
          if (t?.Schema.Length > 0) sql.Append($"{_brackOpen}{t.Schema}{_brackClose}.");
          sql.Append(_brackOpen).Append(this[tabIndex]!.Name).Append(_brackClose).Append('.');
        }

        sql.Append(_brackOpen).Append(field.Name).Append(_brackClose).Append('\n');
      }
    }

    sql.Append(")\nValues (");
    for (var j = 0; j < this[tabIndex]!.FieldsCount; j++)
    {
      var field = this[tabIndex]![j]!;
      if (field.IsReadOnly) continue;
      sql.Append(j == 0 ? "  " : ", ");
      if (useParameterSyntax)
      {
        sql.Append('?');
      }
      else
      {
        BuildSqlAppendFieldValue(sql, field.FieldType, field.Value, UseSimpleDate);
        sql.Append('\n');
      }
    }

    sql.Append(')');
  }

  private void BuildSqlAppendFieldValue(StringBuilder sql, DbFieldType fieldDbType, object? fieldValue,
    bool useSimpleDate = false)
  {
    var sqlVal = SqlValueFormatter.FormatUserValueForWhere(fieldValue, DbConnect.DbType, fieldDbType, useSimpleDate);

    sql.Append(sqlVal);
  }

  internal void BuildSqlAddSqlTablePart(StringBuilder sql, SqlTypes sqlType, bool isFirstTable, string tableName,
    string tableAlias)
  {
    var preSql = new StringBuilder();
    switch (sqlType)
    {
      case SqlTypes.Select:
      case SqlTypes.Delete:
        preSql.Append(isFirstTable ? "From\n  " : ", ");
        break;
      case SqlTypes.Insert:
        preSql.Append(isFirstTable ? "Into  " : ", ");
        break;
      case SqlTypes.Update:
        preSql.Append(isFirstTable ? "  " : ", ");
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
    }

    var datDefTable = this[tableName];
    if (datDefTable?.Schema.Length > 0)
      preSql.Append($"{_brackOpen}{datDefTable.Schema}{_brackClose}.");
    preSql.Append(_brackOpen).Append(tableName).Append(_brackClose);
    if (tableAlias.Length > 0) preSql.Append($" {tableAlias}");
    if (WithNoLock) preSql.Append(" WITH(NOLOCK)");
    preSql.Append('\n');

    sql.Append(DbConnect.UseUpperCaseSql ? preSql.ToString().ToUpper() : preSql.ToString());
  }

  private void BuildSqlAddFieldPart(StringBuilder sql, bool isFirstField, string tableSchema, string tableName,
    string tableAlias, AggregateTypes fieldAgg, string fieldName, string fieldAlias)
  {
    sql.Append(isFirstField ? "  " : ", ");

    if (fieldAgg != AggregateTypes.Nothing)
      AppendAggregatePrefix(sql, fieldAgg);
    else if (GroupsCount > 0 && DbConnect.DbType == DataBaseTypes.MsAccess)
      sql.Append("Last(");

    if (tableSchema.Length > 0)
      sql.Append(_brackOpen).Append(tableSchema).Append(_brackClose).Append('.');

    sql.Append(_brackOpen)
      .Append(tableAlias.Length == 0 ? tableName : tableAlias)
      .Append(_brackClose).Append('.')
      .Append(_brackOpen).Append(fieldName).Append(_brackClose);

    if (fieldAgg != AggregateTypes.Nothing ||
        (GroupsCount > 0 && DbConnect.DbType == DataBaseTypes.MsAccess))
      sql.Append(')');

    if (fieldAlias.Length > 0)
      sql.Append($" {_brackOpen}{fieldAlias}{_brackClose}");

    sql.Append('\n');
  }

  internal void BuildSqlAddSqlTypePart(StringBuilder sql, SqlTypes sqlType)
  {
    switch (sqlType)
    {
      case SqlTypes.Select:
        if (SelectIsDistinct)
        {
          sql.Append(DbConnect.UseUpperCaseSql ? "SELECT DITINCT" : "Select Distinct");
          return;
        }

        sql.Append(DbConnect.UseUpperCaseSql ? "SELECT" : "Select");
        sql.Append(
          UseTopEnabled && DbConnect.DbType is DataBaseTypes.MsSqlServer ? $" TOP({MaxDisplayedRows})\n" : "\n");
        return;
      case SqlTypes.Insert:
        sql.Append("Insert\n");
        return;
      case SqlTypes.Update:
        sql.Append("UpDate\n");
        return;
      case SqlTypes.Delete:
        sql.Append("Delete\n");
        return;
      default:
        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
    }
  }

  internal void BuildSqlAddWherePart(StringBuilder sql, bool isGroup, WhereConditionArgs a)
  {
    var opy = a.OpType switch
    {
      OpTypes.Where => isGroup ? "Having  " : "Where   ",
      OpTypes.And => "And     ",
      OpTypes.Or => "Or      ",
      OpTypes.AndNot => "And Not ",
      OpTypes.OrNot => "Or Not  ",
      OpTypes.WhereNot => isGroup ? "Having Not " : "Where  Not ",
      _ => ""
    };
    sql.Append(DbConnect.UseUpperCaseSql ? opy.ToUpper() : opy);

    sql.Append(a.BracksOpen).Append(' ');

    if (a.OpType is OpTypes.Where or OpTypes.And or OpTypes.Or
        && a is { CompType: CompTypes.Unequal, CompValue.Length: > 0 }
        && string.Equals(a.CompValue, "null", StringComparison.OrdinalIgnoreCase))
      sql.Append("Not ");

    if (a.FieldAgg != AggregateTypes.Nothing) AppendAggregatePrefix(sql, a.FieldAgg);

    var datDefTable = this[a.Table];
    if (datDefTable != null)
    {
      if (datDefTable.Schema.Length > 0)
        sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? datDefTable.Schema.ToUpper() : datDefTable.Schema)
          .Append(_brackClose).Append('.');
      sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? a.Table.ToUpper() : a.Table).Append(_brackClose)
        .Append('.').Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? a.Field.ToUpper() : a.Field)
        .Append(_brackClose);
    }
    else
    {
      BuildSqlAppendFieldValue(sql, a.FieldDbType, a.LeftCompValue, UseSimpleDate);
    }

    if (a.FieldAgg != AggregateTypes.Nothing) sql.Append(')');

    var isNullCompare = string.Equals(a.CompValue, "null", StringComparison.OrdinalIgnoreCase);
    switch (a.CompType)
    {
      case CompTypes.Equal:
        sql.Append(isNullCompare ? "  IS  " : "  =   ");
        break;
      case CompTypes.Unequal:
        sql.Append(isNullCompare ? "  IS  "
          : DbConnect.DbType == DataBaseTypes.MsAccess ? "  <>  " : "  !=  ");
        break;
      case CompTypes.Greater: sql.Append("  >   "); break;
      case CompTypes.Smaller: sql.Append("  <   "); break;
      case CompTypes.GreaterEqual: sql.Append("  >=  "); break;
      case CompTypes.SmallerEqual: sql.Append("  <=  "); break;
      case CompTypes.Like: sql.Append(" LIKE "); break;
      case CompTypes.In: sql.Append(" In ( "); break;
      case CompTypes.IsNull:
        sql.Append(" IS NULL ");
        sql.Append(a.BracksClose).Append('\n');
        return;
      case CompTypes.IsNotNull:
        sql.Append(" IS NOT NULL ");
        sql.Append(a.BracksClose).Append('\n');
        return;
      case CompTypes.Nothing:
      case CompTypes.Exists:
      default:
        throw new ArgumentOutOfRangeException();
    }

    if (a is { CompType: CompTypes.In, CompValueList: not null })
    {
      for (var i = 0; i < a.CompValueList.Count; i++)
      {
        if (i > 0) sql.Append(", ");
        BuildSqlAppendFieldValue(sql, a.FieldDbType, a.CompValueList[i].ToString(), UseSimpleDate);
      }

      sql.Append(')');
    }
    else
    {
      if (a.CompFieldAgg != AggregateTypes.Nothing) AppendAggregatePrefix(sql, a.CompFieldAgg);
      if (a.CompTable.Length > 0)
      {
        datDefTable = this[a.CompTable];
        if (datDefTable?.Schema.Length > 0)
          sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? datDefTable.Schema.ToUpper() : datDefTable.Schema)
            .Append(_brackClose).Append('.');
        sql.Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? a.CompTable.ToUpper() : a.CompTable)
          .Append(_brackClose)
          .Append('.').Append(_brackOpen).Append(DbConnect.UseUpperCaseSql ? a.CompField.ToUpper() : a.CompField)
          .Append(_brackClose);
        if (a.CompFieldAgg != AggregateTypes.Nothing) sql.Append(')');
      }
      else
      {
        BuildSqlAppendFieldValue(sql, a.FieldDbType, a.CompValue, UseSimpleDate);
      }
    }

    sql.Append(a.BracksClose).Append('\n');
  }

  public DatDefTable? TablesAdd(string schema, string tableName, string? tableAlias, object? tag = null)
  {
    if (string.IsNullOrWhiteSpace(tableName)) return null;
    tableAlias ??= string.Empty;
    var key = tableAlias.Length == 0 ? tableName : tableAlias;
    if (_tableIndex.ContainsKey(key)) return null;
    var datDefTable = new DatDefTable(schema, tableName, tableAlias, key, Tables.Count, this)
    {
      Tag = tag
    };
    Tables.Add(datDefTable);
    _tableIndex[key] = Tables.Count - 1;
    return datDefTable;
  }

  public void TablesClear()
  {
    _allFields.Clear();
    AllFields.Clear();
    foreach (var t in Tables) t.Dispose();
    Tables.Clear();
    _tableIndex.Clear();
  }

  public void JoinsClear()
  {
    foreach (var j in JoinsAll) j.Dispose();
    _joinIndex.Clear();
    JoinsAll.Clear();
  }

  public DatDefJoin JoinsAdd(JoinTypes joinType, string from, string fromField, string to, string toField,
    string fromSchema, string toSchema, string fromAlias = "", string fromFieldAlias = "", string toAlias = "",
    string toFieldAlias = "")
  {
    var datDefJoin = AddJoin(from, fromAlias, to, toAlias, joinType, fromSchema, toSchema);
    datDefJoin.FieldsAdd(fromField, fromFieldAlias, CompTypes.Equal, toField, toFieldAlias);
    return datDefJoin;
  }

  private DatDefJoin AddJoin(string from, string fromAlias, string to, string toAlias, JoinTypes joinType,
    string fromSchema, string toSchema)
  {
    TablesAdd(fromSchema, from, fromAlias);
    TablesAdd(toSchema, to, toAlias);

    var baseName = fromAlias.Length > 0 ? fromAlias : from;
    baseName += joinType switch
    {
      JoinTypes.LeftJoin => " -> ",
      JoinTypes.RightJoin => " <- ",
      _ => " -- "
    };
    baseName += toAlias.Length > 0 ? toAlias : to;

    var key = baseName;

    if (_joinIndex.ContainsKey(key))
    {
      var suffix = 2;
      while (_joinIndex.ContainsKey($"{baseName}_{suffix}")) suffix++;
      key = $"{baseName}_{suffix}";
    }

    JoinsAll.Add(new DatDefJoin(from, fromAlias, to, toAlias, joinType, key));
    _joinIndex[key] = JoinsAll.Count - 1;
    return JoinsAll[^1];
  }

  public DatDefJoin? Joins(int index)
  {
    return index < 0 || index >= JoinsAll.Count ? null : JoinsAll[index];
  }

  public void CondsClear()
  {
    _conds.Clear();
  }

  public DatDefCond CondsAdd(CondArgs a)
  {
    var fieldType = a.IsString == true ? DbFieldType.String : DbFieldType.Unknown;
    if (a.IsString != true)
    {
      var tbl = a.Table.Length > 0 ? DbConnect.DataDic[a.Table] : DbConnect.DataDic[a.CompTable];
      var col = a.Table.Length > 0 ? tbl?[a.Field] : tbl?[a.CompField];
      if (col != null)
        fieldType = col.FieldType;
      else if (AllFields.TryGetValue(a.Field, out var fallback))
        fieldType = fallback.FieldType;
    }

    _conds.Add(new DatDefCond(a.OpType, a.BracksOpen, a.Table, a.Field, a.LeftCompValue,
      a.FieldAgg, a.CompType, a.CompValue, a.CompTable, a.CompFieldAgg, a.CompField,
      a.BracksClose, _conds.Count, fieldType));
    return _conds[^1];
  }

  public DatDefCond CondsAdd(OpTypes opType, string bracksOpen, string table, AggregateTypes fieldAgg, string field,
    string leftCompValue, CompTypes compType, string compValue, string compTable, AggregateTypes compFieldAgg,
    string compField, string bracksClose, bool isString)
  {
    return CondsAdd(new CondArgs(table, field, compType, compValue, opType, bracksOpen, bracksClose, fieldAgg,
      leftCompValue, compTable, compFieldAgg, compField, isString));
  }

  public DatDefCond CondsAdd(OpTypes opType, string bracksOpen, string table, string field, CompTypes compType,
    string compValue, string bracksClose)
  {
    return CondsAdd(new CondArgs(table, field, compType, compValue, opType, bracksOpen, bracksClose));
  }

  public DatDefCond? Conds(int index)
  {
    return index < 0 || index >= _conds.Count ? null : _conds[index];
  }

  public void SortsClear()
  {
    SortsList.Clear();
    _sortIndex.Clear();
  }

  public DatDefSort? SortsAdd(string table, string field, SortTypes type)
  {
    return SortsAdd("", table, AggregateTypes.Nothing, field, type);
  }

  public DatDefSort? SortsAdd(string schema, string table, AggregateTypes fieldAgg, string field, SortTypes type)
  {
    _ = this[table] ?? TablesAdd(schema, table, "");
    string resolvedTable;
    if (table.Length == 0)
    {
      if (Tables.Count == 0) return null;
      var first = this[0];
      if (first == null) return null;
      resolvedTable = first.Name;
    }
    else
    {
      resolvedTable = table;
    }

    var key = $"{resolvedTable}_{field}";
    if (_sortIndex.ContainsKey(key)) return null;
    SortsList.Add(new DatDefSort(resolvedTable, fieldAgg, field, type, key, SortsList.Count));
    _sortIndex[key] = SortsList.Count - 1;
    return SortsList[^1];
  }

  public DatDefSort? Sorts(int index)
  {
    return index < 0 || index >= SortsList.Count ? null : SortsList[index];
  }

  public void GroupsClear()
  {
    _groups.Clear();
    _groupIndex.Clear();
  }

  public DatDefGroup? GroupsAdd(string table, string field)
  {
    string resolvedTable;
    if (table.Length == 0)
    {
      if (Tables.Count == 0) return null;
      var first = this[0];
      if (first == null) return null;
      resolvedTable = first.Name;
    }
    else
    {
      resolvedTable = table;
    }

    var key = $"{resolvedTable}_{field}";
    if (_groupIndex.ContainsKey(key)) return null;
    _groups.Add(new DatDefGroup(resolvedTable, field, key));
    _groupIndex[key] = _groups.Count - 1;
    return _groups[^1];
  }

  public DatDefGroup? Groups(int index)
  {
    return index < 0 || index >= _groups.Count ? null : _groups[index];
  }

  public void GroupCondsClear()
  {
    _groupConds.Clear();
  }

  public DatDefCond GroupCondsAdd(OpTypes opType, string bracksOpen, string table, string field,
    AggregateTypes fieldAgg, CompTypes compType, string compValue, string compTable, AggregateTypes compFieldAgg,
    string compField, string bracksClose)
  {
    var fieldType = DbFieldType.Unknown;
    {
      var tbl = DbConnect.DataDic[table];
      var col = tbl?[field];
      if (col != null) fieldType = col.FieldType;
    }
    _groupConds.Add(new DatDefCond(opType, bracksOpen, table, field, "", fieldAgg, compType, compValue, compTable,
      compFieldAgg, compField, bracksClose, _conds.Count, fieldType));
    return _groupConds[^1];
  }

  public DatDefCond? GroupConds(int index)
  {
    return index < 0 || index >= _groupConds.Count ? null : _groupConds[index];
  }

  public sealed record CondArgs(
    string Table,
    string Field,
    CompTypes CompType = CompTypes.Equal,
    string CompValue = "",
    OpTypes OpType = OpTypes.Where,
    string BracksOpen = "",
    string BracksClose = "",
    AggregateTypes FieldAgg = AggregateTypes.Nothing,
    string LeftCompValue = "",
    string CompTable = "",
    AggregateTypes CompFieldAgg = AggregateTypes.Nothing,
    string CompField = "",
    bool? IsString = null);

  public sealed record WhereConditionArgs(
    OpTypes OpType,
    string BracksOpen,
    string Table,
    AggregateTypes FieldAgg,
    string Field,
    string LeftCompValue,
    DbFieldType FieldDbType,
    CompTypes CompType,
    string CompValue,
    string CompTable,
    AggregateTypes CompFieldAgg,
    string CompField,
    string BracksClose,
    IList<object>? CompValueList = null);
}