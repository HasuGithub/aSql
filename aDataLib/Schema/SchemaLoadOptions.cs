namespace aDataLib.Schema;

public sealed record SchemaLoadOptions(
    DataBaseTypes DatabaseType,
    string? SchemaName,
    bool IncludeViews = true,
    bool SortColumns = false);
