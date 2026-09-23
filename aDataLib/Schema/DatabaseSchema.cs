namespace aDataLib.Schema;

public sealed record DatabaseSchema(
  IReadOnlyList<TableSchema> Tables,
  IReadOnlyList<RelationSchema> Relations);