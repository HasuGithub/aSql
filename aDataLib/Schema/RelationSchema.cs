namespace aDataLib.Schema;

public sealed record RelationSchema(
  string Name,
  string PrincipalTable,
  string DependentTable,
  IReadOnlyList<RelationColumnMapping> Columns);

public sealed record RelationColumnMapping(
  string PrincipalColumn,
  string DependentColumn,
  int Ordinal);