namespace aDataLib;

public class DatDefSort(
  string table,
  AggregateTypes fieldAgg,
  string field,
  SortTypes type,
  string key,
  int myIndex)
{
  public object? Tag { get; set; }
  public string Table { get; } = table;
  public AggregateTypes FieldAgg { get; set; } = fieldAgg;
  public string Field { get; } = field;
  public SortTypes Type { get; set; } = type;
  public string Key { get; } = key;
  public int MyIndex { get; } = myIndex;

  public override string ToString()
  {
    return Key;
  }
}