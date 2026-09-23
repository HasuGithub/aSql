using System.Collections;
using System.Data;
using System.Text;

using aDataLib.Schema;

namespace aDataLib;

public class DataDictionary(DbConnect dbConnect, bool sortCols)
{
  private readonly SortedList _lSlRelations = new();

  private readonly List<DictTable> _tables = [];

  internal readonly SortedList lSlTables = new();

  internal StringBuilder lSql = new();

  internal Hashtable lhtTables = new();

  public DataBaseTypes DbType { get; private set; } = dbConnect.DbType;

  public DbConnect DbCon { get; } = dbConnect;

  public string DbName { get; private set; } = dbConnect.DbName;

  public bool Error { get; private set; }

  public string ErrorText { get; private set; } = "";

  public IReadOnlyList<DictTable> Tables => _tables;
  public int TablesCount => lSlTables.Count;

  public DictTable? this[string tableName]
  {
    get
    {
      var key = NormalizeKey(tableName);
      if (key.Length == 0 || !lSlTables.ContainsKey(key)) return null;

      return lSlTables[key] as DictTable;
    }
  }

  public DictTable? this[int index]
  {
    get
    {
      if (index < 0 || index >= lSlTables.Count) return null;

      return lSlTables.GetByIndex(index) as DictTable;
    }
  }

  public DictTable? this[long prim]
  {
    get
    {
      if (!lhtTables.ContainsKey(prim)) return null;

      if (lhtTables[prim] is not int tableIndex) return null;

      if (tableIndex < 0 || tableIndex >= lSlTables.Count) return null;

      return lSlTables.GetByIndex(tableIndex) as DictTable;
    }
  }

  public bool Build()
  {
    if (DbCon == null) throw new InvalidOperationException("DbConnect is not initialized.");

    if (DbCon.Connection is not { State: ConnectionState.Open }) DbCon.Connection?.Open();

    var effectiveSchema = string.IsNullOrWhiteSpace(DbCon.DbSchema)
      ? null
      : DbCon.DbSchema;
    var options = new SchemaLoadOptions(DbCon.DbType, effectiveSchema, true, sortCols);
    var schemaContext = SchemaProviderFactory.Resolve(DbCon);
    var schema = schemaContext.Provider.LoadSchema(schemaContext.Connection, options);

    ResetBuildState();
    PopulateFromSchema(schema);
    RefreshTypedCaches();

    if (DbCon.Connection is not { State: ConnectionState.Closed }) DbCon.Connection?.Close();

    return true;
  }

  private void ResetBuildState()
  {
    Clear();
    DbName = DbCon.DbName;
    DbType = DbCon.DbType;
    Error = false;
    ErrorText = string.Empty;
  }

  private void PopulateFromSchema(DatabaseSchema schema)
  {
    PopulateTables(schema.Tables);
    PopulateRelations(schema.Relations);
  }

  private void PopulateTables(IReadOnlyList<TableSchema> tables)
  {
    foreach (var table in tables)
    {
      var dictTable = TablesAdd(table.Name);
      if (dictTable == null) continue;
      dictTable.Schema = table.Schema ?? string.Empty;

      var ordinal = 1;
      foreach (var col in table.Columns)
      {
        var fieldType = GetDbFieldTypeFromClr(col.ClrType);
        dictTable.ColumnsAdd(
          col.Name,
          !col.IsNullable,
          fieldType,
          col.Scale,
          col.Length,
          ordinal++,
          col.Description ?? string.Empty,
          col.IsAutoIncrement,
          col.IsUnique);

        var dictCol = dictTable[col.Name];
        if (dictCol == null) continue;
        dictCol.IsPrimaryKeyColumn = col.IsPrimaryKey;
        dictCol.IsReadOnly = col.IsAutoIncrement;
        dictCol.IsAutoIncrement = col.IsAutoIncrement;
        dictCol.IsUnique = col.IsUnique || col.IsPrimaryKey;
        dictCol.Tag = col;
      }

      foreach (var index in table.Indexes)
      {
        var dictIndex = dictTable.Indexes(index.Name) ??
                        dictTable.IndexesAdd(index.Name, index.IsPrimaryKey, index.IsUnique);
        if (dictIndex == null) continue;
        var colOrdinal = 1;
        foreach (var colName in index.Columns) dictIndex.ColumnsAdd(colName, colOrdinal++);
      }
    }
  }

  private void PopulateRelations(IReadOnlyList<RelationSchema> relations)
  {
    foreach (var rel in relations)
    {
      if (this[rel.PrincipalTable] == null || this[rel.DependentTable] == null) continue;

      var dictRel = Relations(rel.Name) ?? RelationsAdd(rel.Name, rel.PrincipalTable, rel.DependentTable, false, false);
      if (dictRel == null) continue;
      foreach (var mapping in rel.Columns)
        dictRel.ColumnsAdd(mapping.PrincipalColumn, mapping.DependentColumn, mapping.Ordinal);

      var relFKey = dictRel.Clone();
      relFKey.RelationType = RelationTypes.FKey;

      this[dictRel.ForeignTableName]?.ForeignKeysAdd(relFKey);
      this[dictRel.TableName]?.ReferencedByAdd(dictRel);
    }
  }

  private void RefreshTypedCaches()
  {
    _tables.Clear();
    for (var tableIndex = 0; tableIndex < lSlTables.Count; tableIndex++)
      if (lSlTables.GetByIndex(tableIndex) is DictTable t)
        _tables.Add(t);
  }

  private static DbFieldType GetDbFieldTypeFromClr(Type clrType)
  {
    if (clrType == typeof(string)) return DbFieldType.String;

    if (clrType == typeof(int)) return DbFieldType.Int32;

    if (clrType == typeof(short)) return DbFieldType.Int16;

    if (clrType == typeof(long)) return DbFieldType.Int64;

    if (clrType == typeof(decimal)) return DbFieldType.Decimal;

    if (clrType == typeof(double) || clrType == typeof(float)) return DbFieldType.Double;

    if (clrType == typeof(DateTime)) return DbFieldType.DateTime;

    if (clrType == typeof(bool)) return DbFieldType.Boolean;

    // ReSharper disable once ConvertIfStatementToReturnStatement
    if (clrType == typeof(byte[])) return DbFieldType.Binary;

    return DbFieldType.Unknown;
  }

  public void Clear()
  {
    Error = false;
    ErrorText = "";
    lSql.Length = 0;
    TablesClear();
    RelationsClear();
    _tables.Clear();
  }

  private static string NormalizeKey(string value)
  {
    return string.IsNullOrWhiteSpace(value) ? string.Empty : value.ToLowerInvariant();
  }

  public void RelationsClear()
  {
    for (var i = 0; i < _lSlRelations.Count; i++)
      if (_lSlRelations.GetByIndex(i) is DictRelation dictRelation)
        dictRelation.Clear();

    _lSlRelations.Clear();
    _lSlRelations.TrimToSize();
  }

  public DictRelation? RelationsAdd(string relationName, string tableName, string foreignTableName,
    bool onUpDateCascade, bool onDeleteCascade)
  {
    var key = NormalizeKey(relationName);
    if (key.Length == 0 || _lSlRelations.ContainsKey(key)) return null;

    var motherTable = this[tableName];
    if (motherTable == null) return null;
    var relation = new DictRelation(RelationTypes.RefBy, relationName, tableName, foreignTableName, onUpDateCascade,
      onDeleteCascade, motherTable);
    _lSlRelations.Add(key, relation);
    return relation;
  }

  public DictRelation? Relations(string relationName)
  {
    var key = NormalizeKey(relationName);
    if (key.Length == 0 || !_lSlRelations.ContainsKey(key)) return null;

    return _lSlRelations[key] as DictRelation;
  }

  public DictTable? TablesAdd(string tableName)
  {
    var key = NormalizeKey(tableName);
    if (key.Length == 0 || lSlTables.ContainsKey(key)) return null;

    var table = new DictTable(tableName, sortCols);
    lSlTables.Add(key, table);
    return table;
  }

  public void TablesClear()
  {
    for (var i = 0; i < lSlTables.Count; i++)
      if (lSlTables.GetByIndex(i) is DictTable dictTable)
        dictTable.Clear();

    lSlTables.Clear();
    lSlTables.TrimToSize();
    lhtTables.Clear();
  }
}