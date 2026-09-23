using System.Collections;

namespace aDataLib;

public class DictTable(string tableName, bool sortCols)
{
  private readonly SortedList _lSlColumns = new();
  private readonly SortedList _lSlColumnsOrdinal = new();
  private readonly SortedList _lSlForeignKeys = new();

  private readonly SortedList _lSlIndexes = new();
  private readonly SortedList _lSlReferencedBy = new();
  internal readonly Hashtable lhtColums = new();
  public string TableName { get; set; } = tableName;

  public string Schema { get; set; } = string.Empty;

  public bool SortCols { get; set; } = sortCols;

  public int ForeignKeysCount => _lSlForeignKeys.Count;

  public int ReferencedByCount => _lSlReferencedBy.Count;

  public int ColumnsCount => _lSlColumns.Count;

  public DictColumn? this[string columnName]
  {
    get
    {
      if (string.IsNullOrWhiteSpace(columnName)) return null;
      return _lSlColumns[columnName.ToLowerInvariant()] as DictColumn;
    }
  }

  public DictColumn? this[int index]
  {
    get
    {
      if (index < 0) return null;
      if (SortCols)
      {
        if (index >= _lSlColumns.Count) return null;
        return _lSlColumns.GetByIndex(index) as DictColumn;
      }

      if (index >= _lSlColumnsOrdinal.Count) return null;
      return _lSlColumnsOrdinal.GetByIndex(index) as DictColumn;
    }
  }

  public DictIndex? PrimKey { get; private set; }

  public int IndexesCount => _lSlIndexes.Count;

  public List<DictColumn> AllColumns { get; set; } = [];

  public void Clear()
  {
    ColumnsClear();
    IndexesClear();
    ReferencedByClear();
    ForeignKeysClear();
  }

  public DictRelation? ForeignKeysAdd(DictRelation rel)
  {
    var key = rel.RelationName.ToLowerInvariant();
    if (_lSlForeignKeys.ContainsKey(key)) return _lSlForeignKeys[key] as DictRelation;
    _lSlForeignKeys.Add(key, rel);
    return _lSlForeignKeys[key] as DictRelation;
  }

  public DictRelation? ForeignKeys(int index)
  {
    if (index < 0 || index >= _lSlForeignKeys.Count) return null;
    return _lSlForeignKeys.GetByIndex(index) as DictRelation;
  }

  public void ForeignKeysClear()
  {
    _lSlForeignKeys.Clear();
    _lSlForeignKeys.TrimToSize();
  }

  public DictRelation? ReferencedByAdd(DictRelation rel)
  {
    var key = rel.RelationName.ToLowerInvariant();
    if (_lSlReferencedBy.ContainsKey(key)) return _lSlReferencedBy[key] as DictRelation;
    _lSlReferencedBy.Add(key, rel);
    return _lSlReferencedBy[key] as DictRelation;
  }

  public DictRelation? ReferencedBy(int index)
  {
    if (index < 0 || index >= _lSlReferencedBy.Count) return null;
    return _lSlReferencedBy.GetByIndex(index) as DictRelation;
  }

  public void ReferencedByClear()
  {
    _lSlReferencedBy.Clear();
    _lSlReferencedBy.TrimToSize();
  }

  public DictColumn? ColumnsAdd(string colName, bool required, DbFieldType fieldType, int scale, int length,
    int ordinalPos, string description, bool isAutoIncrement, bool isUnique)
  {
    if (string.IsNullOrWhiteSpace(colName)) return null;
    var key = colName.ToLowerInvariant();
    if (_lSlColumns.ContainsKey(key)) return _lSlColumns[key] as DictColumn;
    var value = new DictColumn(colName, required, fieldType, this)
    {
      IsAutoIncrement = isAutoIncrement,
      IsUnique = isUnique
    };
    _lSlColumnsOrdinal[ordinalPos] = value;
    _lSlColumns[key] = value;
    AllColumns.Add(value);
    return _lSlColumns[key] as DictColumn;
  }

  internal void ColumnsClear()
  {
    for (var i = 0; i < _lSlColumns.Count; i++)
      if (_lSlColumns.GetByIndex(i) is DictColumn dictColumn)
        dictColumn.Dispose();

    _lSlColumns.Clear();
    _lSlColumns.TrimToSize();
    AllColumns.Clear();
    _lSlColumnsOrdinal.Clear();
    _lSlColumnsOrdinal.TrimToSize();
    lhtColums.Clear();
  }

  internal void IndexesClear()
  {
    for (var i = 0; i < _lSlIndexes.Count; i++)
      if (_lSlIndexes.GetByIndex(i) is DictIndex dictIndex)
        dictIndex.Clear();

    _lSlIndexes.Clear();
    _lSlIndexes.TrimToSize();
    PrimKey = null;
  }

  public DictIndex? IndexesAdd(string indexName, bool primary, bool unique)
  {
    if (string.IsNullOrWhiteSpace(indexName)) return null;
    var key = indexName.ToLowerInvariant();
    if (_lSlIndexes.ContainsKey(key))
    {
      var existing = _lSlIndexes[key] as DictIndex;
      if (primary && existing != null) PrimKey = existing;
      return existing;
    }

    var created = new DictIndex(indexName, primary, unique, this);
    _lSlIndexes.Add(key, created);
    if (primary) PrimKey = created;
    return created;
  }

  public DictIndex? Indexes(string indexName)
  {
    if (string.IsNullOrWhiteSpace(indexName)) return null;
    return _lSlIndexes[indexName.ToLowerInvariant()] as DictIndex;
  }

  public DictIndex? Indexes(int index)
  {
    if (index < 0 || index >= _lSlIndexes.Count) return null;
    return _lSlIndexes.GetByIndex(index) as DictIndex;
  }

  public class DictColumn(
    string colName,
    bool required,
    DbFieldType fieldType,
    DictTable myMotherTable)
  {
    public DictTable? MyMotherTable { get; private set; } = myMotherTable;
    public string ColName { get; set; } = colName;
    public bool Required { get; set; } = required;
    public DbFieldType FieldType { get; set; } = fieldType;
    public bool IsAutoIncrement { get; set; }
    public bool IsUnique { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsPrimaryKeyColumn { get; set; }
    public object? Tag { get; set; }

    ~DictColumn()
    {
      Dispose();
    }

    public void Dispose()
    {
      MyMotherTable = null;
      GC.SuppressFinalize(this);
    }
  }
}