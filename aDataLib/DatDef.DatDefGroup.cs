namespace aDataLib;

public class DatDefGroup(string table, string field, string key)
{
  public object? Tag { get; set; }
  public string Table { get; } = table;
  public string Field { get; } = field;
  public string Key { get; } = key;

  public override string ToString()
  {
    return Key;
  }
}