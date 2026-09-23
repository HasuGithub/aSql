using System.Collections;

namespace aDataLib;

public class DictIndex(string indexName, bool primary, bool unique, DictTable myMotherTable)
{
  private readonly SortedList _lSlColsOrdinal = new();
  private readonly SortedList _lSlIndexColumns = new();

  public int ColumnsCount => _lSlIndexColumns.Count;

  public DictIndexColumn? this[int index]
  {
    get
    {
      if (index < 0 || index >= _lSlColsOrdinal.Count) return null;
      return _lSlColsOrdinal.GetByIndex(index) as DictIndexColumn;
    }
  }

  public string IndexName { get; set; } = indexName;
  public bool Primary { get; set; } = primary;
  public bool Unique { get; set; } = unique;
  public DictTable? MyMotherTable { get; set; } = myMotherTable;

  public void Clear()
  {
    ColumnsClear();
    MyMotherTable = null;
  }

  public DictIndexColumn? ColumnsAdd(string colName, int ordinalPos)
  {
    if (string.IsNullOrWhiteSpace(colName)) return null;
    var key = colName.ToLowerInvariant();
    if (_lSlIndexColumns.ContainsKey(key)) return _lSlIndexColumns[key] as DictIndexColumn;
    var value = new DictIndexColumn(colName, this);
    _lSlColsOrdinal[ordinalPos] = value;
    _lSlIndexColumns[key] = value;
    return _lSlIndexColumns[key] as DictIndexColumn;
  }

  public void ColumnsClear()
  {
    for (var i = 0; i < _lSlIndexColumns.Count; i++)
      if (_lSlIndexColumns.GetByIndex(i) is DictIndexColumn col)
        col.Dispose();

    _lSlIndexColumns.Clear();
    _lSlColsOrdinal.Clear();
  }

  public class DictIndexColumn(string colName, DictIndex myMotherIndex)
  {
    public DictIndex? MyMotherIndex { get; private set; } = myMotherIndex;
    public string ColName { get; set; } = colName;

    public void Dispose()
    {
      MyMotherIndex = null;
      GC.SuppressFinalize(this);
    }
  }
}