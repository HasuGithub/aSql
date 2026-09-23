using System.Text;

namespace aDataLib;

public class DatDefTable : IDisposable
{
  private readonly DatDef _dd;
  private readonly StringBuilder _sql;
  private DictTable? _dictTable;

  private Dictionary<string, int> _fieldIndex;
  private List<DatDefTableField> _uniqueIndexFields;
  internal bool isDisposing;

  public DatDefTable(string schema, string name, string alias, string key, int myIndex, DatDef myMotherDatDef)
  {
    Schema = schema;
    Name = name;
    Alias = alias;
    Key = key;
    MyIndex = myIndex;
    _dd = myMotherDatDef;
    Fields = [];
    _fieldIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    _uniqueIndexFields = [];
    _dictTable = _dd.DbConnect.DataDic[Name];
    if (myMotherDatDef.DebugMode && _dictTable == null)
      _dd.WriteDebugMessage($"<warning> <DatDefTable> Dictionary-Table is missing in database for -> {Name}\n");
    _sql = new StringBuilder();
  }

  public object? Tag { get; set; }
  public string Schema { get; }
  public string Name { get; }
  public string Alias { get; set; }
  public string Key { get; }
  public int MyIndex { get; }

  public int FieldsCount => Fields.Count;

  public List<DatDefTableField> Fields { get; private set; }

  public DatDefTableField? this[string key]
  {
    get
    {
      if (string.IsNullOrWhiteSpace(key)) return null;
      return _fieldIndex.TryGetValue(key, out var idx) ? this[idx] : null;
    }
  }

  public DatDefTableField? this[int index]
    => index < 0 || index >= Fields.Count ? null : Fields[index];

  public void Dispose()
  {
    isDisposing = true;
    FieldsClear();
    Fields = null!;
    _fieldIndex = null!;
    _uniqueIndexFields = null!;
    Tag = null;
    _dictTable = null;
    GC.SuppressFinalize(this);
  }

  private string GetDeleteSqlIntern(CondTypes condType)
  {
    _sql.Length = 0;
    _dd.BuildSqlAddSqlTypePart(_sql, SqlTypes.Delete);
    _dd.BuildSqlAddSqlTablePart(_sql, SqlTypes.Delete, true, Name, "");
    switch (condType)
    {
      case CondTypes.None:
      {
        var opType = OpTypes.Where;
        foreach (var f in _uniqueIndexFields)
        {
          _dd.BuildSqlAddWherePart(_sql, false, new DatDef.WhereConditionArgs(
            opType, "", f.MyTable.Name, f.FieldAgg, f.Name, "",
            f.FieldType, CompTypes.Equal, f.Value?.ToString() ?? "",
            "", AggregateTypes.Nothing, "", ""));
          opType = OpTypes.And;
        }

        break;
      }
      case CondTypes.DdCondition:
        _dd.BuildSqlAddWhere(_sql, false);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(condType), condType, null);
    }

    _sql.Append('\n');
    return _sql.ToString();
  }

  private string GetUpDateSqlIntern(CondTypes condType, bool takeOnlyChangedFields, bool useParamSyntax)
  {
    _sql.Length = 0;
    _dd.BuildSqlAddSqlTypePart(_sql, SqlTypes.Update);
    _dd.BuildSqlAddSqlTablePart(_sql, SqlTypes.Update, true, Name, "");
    _dd.BuildSqlAddUpDatePart(_sql, takeOnlyChangedFields, useParamSyntax, MyIndex);
    switch (condType)
    {
      case CondTypes.None:
      {
        var opType = OpTypes.Where;
        foreach (var f in _uniqueIndexFields)
        {
          var whereValue = f.ValueOld != f.Value
            ? f.ValueOld?.ToString() ?? ""
            : f.Value?.ToString() ?? "";
          _dd.BuildSqlAddWherePart(_sql, false, new DatDef.WhereConditionArgs(
            opType, "", f.MyTable.Name, f.FieldAgg, f.Name, "",
            f.FieldType, CompTypes.Equal, whereValue,
            "", AggregateTypes.Nothing, "", ""));
          opType = OpTypes.And;
        }

        break;
      }
      case CondTypes.DdCondition:
        _dd.BuildSqlAddWhere(_sql, false);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(condType), condType, null);
    }

    _sql.Append('\n');
    return _sql.ToString();
  }

  private string GetInsertSqlIntern(bool useParamSyntax)
  {
    _sql.Length = 0;
    _dd.BuildSqlAddSqlTypePart(_sql, SqlTypes.Insert);
    _dd.BuildSqlAddSqlTablePart(_sql, SqlTypes.Insert, true, Name, "");
    _dd.BuildSqlAddInsertPart(_sql, useParamSyntax, MyIndex);
    _sql.Append('\n');
    return _sql.ToString();
  }

  public int WriteUpDate()
  {
    return _dd.ExecuteWrite(() => GetUpDateSqlIntern(CondTypes.DdCondition, false, false), nameof(WriteUpDate));
  }

  public int WriteUpDate(CondTypes condType, bool takeOnlyChangedFields)
  {
    return _dd.ExecuteWrite(() => GetUpDateSqlIntern(condType, takeOnlyChangedFields, false), nameof(WriteUpDate));
  }

  // ReSharper disable once UnusedMember.Global
  public int WriteInsert()
  {
    return _dd.ExecuteWrite(() => GetInsertSqlIntern(false), nameof(WriteInsert));
  }

  public int WriteInsert(ref long newPrim)
  {
    if (this["prim"] != null) return 0;
    _dd.WriteDebugMessage("<error> : Prim-Feld nicht enthalten");
    return -1;
  }

  public int WriteDelete(CondTypes condType)
  {
    return _dd.ExecuteWrite(() => GetDeleteSqlIntern(condType), nameof(WriteDelete));
  }

  public int WriteDelete()
  {
    return _dd.ExecuteWrite(() => GetDeleteSqlIntern(CondTypes.DdCondition), nameof(WriteDelete));
  }

  private void BuildUniqueIndex()
  {
    _uniqueIndexFields.Clear();
    if (_dictTable == null || _dictTable.IndexesCount == 0) return;

    var found = false;
    for (var i = 0; !found && i < _dictTable.IndexesCount; i++)
    {
      if (_dictTable.Indexes(i) is not { Primary: true }) continue;
      found = true;
      for (var j = 0; j < _dictTable.Indexes(i)!.ColumnsCount; j++)
      {
        var f = GetFieldByName(_dictTable.Indexes(i)?[j]?.ColName);
        if (f == null)
        {
          found = false;
          _uniqueIndexFields.Clear();
          break;
        }

        _uniqueIndexFields.Add(f);
      }
    }

    if (found) return;

    for (var i = 0; !found && i < _dictTable.IndexesCount; i++)
    {
      var idx = _dictTable.Indexes(i);
      if (idx is { Primary: true } || idx is not { Unique: true }) continue;
      found = true;
      for (var j = 0; j < idx.ColumnsCount; j++)
      {
        var f = GetFieldByName(idx[j]?.ColName);
        if (f == null)
        {
          found = false;
          _uniqueIndexFields.Clear();
          break;
        }

        _uniqueIndexFields.Add(f);
      }
    }
  }

  public DatDefTableField? FieldsAdd(string name)
  {
    return FieldsAdd(AggregateTypes.Nothing, name, "");
  }

  public DatDefTableField? FieldsAdd(string name, string alias)
  {
    return FieldsAdd(AggregateTypes.Nothing, name, alias);
  }

  public DatDefTableField? FieldsAdd(AggregateTypes fieldAgg, string name, string alias, DbFieldType type)
  {
    if (_dictTable == null && name == "*")
    {
      _dd.WriteDebugMessage(
        $"<error> <DatDefTable> Cannot add by '*' Dictionary-Table is missing in database for -> {Name}\n");
      return null;
    }

    if (name == "*")
    {
      if (Fields.Count > 0) FieldsClear();
      if (_dictTable is not { ColumnsCount: > 0 }) return null;

      for (var i = 0; i < _dictTable.ColumnsCount; i++)
      {
        if (_dictTable[i] is { FieldType: DbFieldType.Binary }) continue;
        var colName = _dictTable[i]?.ColName;
        if (colName == null) continue;

        var fieldAlias = "";
        var fieldKey = colName;
        if (_dd.AllFields.ContainsKey(fieldKey))
        {
          fieldAlias = $"{Key}_{colName}";
          fieldKey = fieldAlias;
        }

        var newCol = new DatDefTableField(fieldAgg, colName, fieldAlias, fieldKey, this,
          _dictTable[i]!.FieldType, _dictTable[i]!.IsReadOnly)
        {
          Tag = _dictTable[i]
        };
        Fields.Add(newCol);
        _fieldIndex[fieldKey] = Fields.Count - 1;
      }

      BuildUniqueIndex();
      _dd.AllFieldsRefresh();
      return Fields.Count > 0 ? Fields[^1] : null;
    }

    var aliasText = alias;
    var resolvedKey = aliasText.Length == 0 ? name : aliasText;
    if (_dd.AllFields.ContainsKey(resolvedKey))
    {
      aliasText = $"{Key}_{resolvedKey}";
      resolvedKey = aliasText;
    }

    if (_fieldIndex.ContainsKey(resolvedKey)) return null;

    var dataType = type;
    if (_dictTable?[name] != null) dataType = _dictTable[name]!.FieldType;

    var newField = new DatDefTableField(fieldAgg, name, aliasText, resolvedKey, this, dataType, false);
    if (_dictTable != null)
    {
      newField.Tag = _dictTable[name];
      Fields.Add(newField);
      _fieldIndex[resolvedKey] = Fields.Count - 1;
      if (_dictTable[name] != null) BuildUniqueIndex();
    }

    _dd.AllFieldsRefresh();
    return Fields.Count > 0 ? Fields[^1] : null;
  }

  public DatDefTableField? FieldsAdd(AggregateTypes fieldAgg, string name, string alias)
  {
    return FieldsAdd(fieldAgg, name, alias, DbFieldType.Unknown);
  }

  public bool FieldsDelete(string key)
  {
    return _fieldIndex.TryGetValue(key, out var idx) && FieldsDelete(idx);
  }

  public bool FieldsDelete(int index)
  {
    if (index < 0 || index >= Fields.Count) return false;
    Fields.RemoveAt(index);
    _fieldIndex.Clear();
    for (var i = 0; i < Fields.Count; i++)
      _fieldIndex[Fields[i].Key] = i;
    _dd.AllFieldsRefresh();
    BuildUniqueIndex();
    return true;
  }

  public void FieldsClear()
  {
    Fields.Clear();
    _fieldIndex.Clear();
    _uniqueIndexFields.Clear();
    _dd.AllFieldsRefresh();
  }

  public DatDefTableField? GetFieldByName(string? fieldName)
  {
    return Fields.FirstOrDefault(f => f.Name == fieldName);
  }

  public class DatDefTableField(
    AggregateTypes fieldAgg,
    string name,
    string alias,
    string key,
    DatDefTable pMyTable,
    DbFieldType fieldType,
    bool isReadOnly)
    : IDisposable
  {
    public bool IsReadOnly { get; set; } = isReadOnly;
    public DbFieldType FieldType { get; } = fieldType;
    public DatDefTable MyTable { get; private set; } = pMyTable;
    public object? Value { get; set; }
    public object? ValueOld { get; set; }
    public object? Tag { get; set; }
    public AggregateTypes FieldAgg { get; set; } = fieldAgg;
    public string Name { get; } = name;
    public string Alias { get; set; } = alias;
    public string Key { get; } = key;
    public int MyAllIndex { get; set; }
    public bool IsInGroupBy { get; set; }

    public void Dispose()
    {
      Value = null;
      ValueOld = null;
      Tag = null;
      MyTable = null!;
      GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
      return Key;
    }
  }
}