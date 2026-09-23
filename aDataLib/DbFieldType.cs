namespace aDataLib;

public enum DbFieldType
{
  Unknown = 0,
  String,
  DateTime,
  DateOnly,
  TimeOnly,
  DateTimeOffset,

  Double,
  Int16,
  Int32,
  Int64,
  Decimal,
  Boolean,
  Binary,
  Guid
}