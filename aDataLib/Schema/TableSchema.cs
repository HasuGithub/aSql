namespace aDataLib.Schema;

public sealed record TableSchema(
    string Name,
    string? Schema,
    IList<ColumnSchema> Columns,
    IList<IndexSchema> Indexes);
