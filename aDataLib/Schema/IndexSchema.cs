namespace aDataLib.Schema;

public sealed record IndexSchema(
  string Name,
  bool IsPrimaryKey,
  bool IsUnique,
  IReadOnlyList<string> Columns);