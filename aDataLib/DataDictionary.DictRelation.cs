using System.Collections;

namespace aDataLib;

public class DictRelation(
  RelationTypes relationType,
  string relationName,
  string tableName,
  string foreignTableName,
  bool onUpDateCascade,
  bool onDeleteCascade,
  DictTable motherTable)
{
  private readonly SortedList _lSlColumnsOrdinal = new();

  private readonly SortedList _lSlRelationColumns = new();

  public RelationTypes RelationType { get; set; } = relationType;

  public string RelationName { get; set; } = relationName;

  public bool OnDeleteCascade { get; set; } = onDeleteCascade;

  public bool OnUpDateCascade { get; set; } = onUpDateCascade;

  public string ForeignTableName { get; set; } = foreignTableName;
  public string TableName { get; set; } = tableName;

  public DictTable MyMotherTable { get; set; } = motherTable;

  public int ColumnsCount => _lSlRelationColumns.Count;

  public DictRelationColumn? this[int index]
  {
    get
    {
      if (index < 0 || index >= _lSlColumnsOrdinal.Count) return null;
      return _lSlColumnsOrdinal.GetByIndex(index) as DictRelationColumn;
    }
  }

  public DictRelation Clone()
  {
    var clonedRelation = new DictRelation(
      RelationType,
      RelationName,
      TableName,
      ForeignTableName,
      OnUpDateCascade,
      OnDeleteCascade,
      MyMotherTable // Achtung: Referenz bleibt gleich, falls tiefe Kopie benötigt wird, muss MyMotherTable ebenfalls geklont werden.
    );
    foreach (DictRelationColumn column in _lSlRelationColumns.Values)
      clonedRelation.ColumnsAdd(column.ColName, column.ForeignColName, column.MyIndex);
    return clonedRelation;
  }

  public void Clear()
  {
    ColumnsClear();
    MyMotherTable = null!;
  }

  public DictRelationColumn? ColumnsAdd(string colName, string foreignColName, int ordinalPos)
  {
    var key = colName.ToLower();
    if (_lSlRelationColumns.ContainsKey(key) || _lSlColumnsOrdinal.ContainsKey(ordinalPos)) return null;
    var relationColumn = new DictRelationColumn(colName, foreignColName, this, ordinalPos);
    _lSlRelationColumns.Add(key, relationColumn);
    _lSlColumnsOrdinal.Add(ordinalPos, relationColumn);
    return relationColumn;
  }

  public void ColumnsClear()
  {
    _lSlRelationColumns.Clear();
    _lSlRelationColumns.TrimToSize();
    _lSlColumnsOrdinal.Clear();
    _lSlColumnsOrdinal.TrimToSize();
  }

  public class DictRelationColumn(string colName, string foreignColName, DictRelation myMotherRelation, int ordinalPos)
  {
    public DictRelation? MyMotherRelation { get; private set; } = myMotherRelation;

    public string ColName { get; set; } = colName;

    public string ForeignColName { get; set; } = foreignColName;

    public int MyIndex { get; } = ordinalPos;
  }
}