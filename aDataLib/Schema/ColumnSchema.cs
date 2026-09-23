namespace aDataLib.Schema;

public sealed class ColumnSchema(
  string name,
  Type clrType,
  bool isNullable,
  bool isAutoIncrement,
  bool isPrimaryKey,
  bool isUnique,
  int length,
  int scale,
  string? description)
{
  public string Name { get; set; } = name;
  public Type ClrType { get; set; } = clrType;
  public bool IsNullable { get; set; } = isNullable;
  public bool IsAutoIncrement { get; set; } = isAutoIncrement;
  public bool IsPrimaryKey { get; set; } = isPrimaryKey;
  public bool IsUnique { get; set; } = isUnique;
  public int Length { get; set; } = length;
  public int Scale { get; set; } = scale;
  public string? Description { get; set; } = description;
}