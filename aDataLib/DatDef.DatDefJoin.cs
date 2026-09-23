namespace aDataLib;

public class DatDefJoin(
  string from,
  string fromAlias,
  string to,
  string toAlias,
  JoinTypes joinType,
  string key)
  : IDisposable
{
  private Dictionary<string, int> _fieldIndex = new(StringComparer.OrdinalIgnoreCase);

  private List<DatDefJoinField> _fields = [];

  public object? Tag { get; set; }
  public string From { get; } = from;
  public string FromAlias { get; } = fromAlias;
  public string To { get; } = to;
  public string ToAlias { get; } = toAlias;
  public JoinTypes JoinType { get; set; } = joinType;
  public string Key { get; } = key;

  public int FieldsCount => _fields.Count;

  public DatDefJoinField? this[int index]
    => index < 0 || index >= _fields.Count ? null : _fields[index];

  public void Dispose()
  {
    FieldsClear();
    _fields = null!;
    _fieldIndex = null!;
    Tag = null;
    GC.SuppressFinalize(this);
  }

  public void FieldsClear()
  {
    _fieldIndex.Clear();
    _fields.Clear();
  }

  public DatDefJoinField? FieldsAdd(string from, string fromAlias, CompTypes compType, string to, string toAlias)
  {
    var nameFrom = fromAlias.Length > 0 ? fromAlias : from;
    var nameTo = toAlias.Length > 0 ? toAlias : to;
    var joinFieldName = $"{nameFrom}_{nameTo}";
    if (_fieldIndex.ContainsKey(joinFieldName)) return null;
    var f = new DatDefJoinField(from, compType, to);
    _fields.Add(f);
    _fieldIndex[joinFieldName] = _fields.Count - 1;
    return _fields[^1];
  }

  public class DatDefJoinField(
    string from,
    CompTypes compType,
    string to)
    : IDisposable
  {
    public object? Tag { get; set; }
    public string From { get; } = from;
    public CompTypes CompType { get; } = compType;
    public string To { get; } = to;

    public void Dispose()
    {
      Tag = null;
      GC.SuppressFinalize(this);
    }
  }
}