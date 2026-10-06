using System.Data;
using System.Data.Common;
using System.Globalization;

namespace aDataLib.Schema;

/// <summary>
///   Native SQL Server schema reader based on Microsoft.Data.SqlClient.
/// </summary>
public sealed class SqlServerSchemaProvider : ISchemaProvider
{
  public DatabaseSchema LoadSchema(DbConnection connection, SchemaLoadOptions options)
  {
    ArgumentNullException.ThrowIfNull(connection);
    ArgumentNullException.ThrowIfNull(options);

    if (connection.State != ConnectionState.Open)
      throw new InvalidOperationException("Database connection must be open to load schema.");

    var schemaName = string.IsNullOrWhiteSpace(options.SchemaName) ? null : options.SchemaName;

    var tables = LoadTables(connection, schemaName, options, out var tableByName);
    var tableIndexes = LoadIndexes(connection, schemaName, tableByName);
    var relations = LoadRelations(connection, schemaName, tableByName);

    var updatedTables = new List<TableSchema>(tables.Count);
    foreach (var table in tables)
    {
      tableIndexes.TryGetValue(table.Name, out var indexes);
      updatedTables.Add(table with { Indexes = indexes ?? [] });
    }

    return new DatabaseSchema(updatedTables, relations);
  }

  private static List<TableSchema> LoadTables(
    DbConnection connection,
    string? schemaName,
    SchemaLoadOptions options,
    out Dictionary<string, TableSchema> tableByName)
  {
    const string sql = """
                       SELECT
                           t.TABLE_SCHEMA,
                           t.TABLE_NAME
                       FROM INFORMATION_SCHEMA.TABLES t
                       WHERE (@schema IS NULL OR t.TABLE_SCHEMA = @schema)
                         AND (
                             t.TABLE_TYPE = 'BASE TABLE'
                             OR (@includeViews = 1 AND t.TABLE_TYPE = 'VIEW')
                         )
                       ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME;
                       """;

    var tables = new List<TableSchema>();
    tableByName = new Dictionary<string, TableSchema>(StringComparer.OrdinalIgnoreCase);
    var discoveredTables = new List<(string Schema, string Name)>();

    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

    var param1 = cmd.CreateParameter();
    param1.ParameterName = "@schema";
    param1.IsNullable = true;
    param1.Value = (object?)schemaName ?? DBNull.Value;
    param1.DbType = DbType.String;
    cmd.Parameters.Add(param1);

    var param2 = cmd.CreateParameter();
    param2.ParameterName = "@includeViews";
    param2.Value = options.IncludeViews;
    param2.DbType = DbType.Boolean;
    cmd.Parameters.Add(param2);

    {
      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        var schema = reader.GetString(0);
        var tableName = reader.GetString(1);
        discoveredTables.Add((schema, tableName));
      }
    }

    tables.AddRange(LoadColumnsNeu(connection, discoveredTables));

    foreach (var tab in tables) tableByName[tab.Name] = tab;

    return tables;
  }


  private static List<TableSchema> LoadColumnsNeu(DbConnection connection,
    List<(string Schema, string Name)> discoveredTables)
  {
    const string sql = """
                       SELECT
                           c.COLUMN_NAME,
                           c.IS_NULLABLE,
                           c.DATA_TYPE,
                           COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY,
                           c.CHARACTER_MAXIMUM_LENGTH,
                           c.NUMERIC_SCALE,
                           CASE WHEN pk.COLUMN_NAME IS NULL THEN 0 ELSE 1 END AS IS_PRIMARY_KEY,
                           CASE WHEN uq.COLUMN_NAME IS NULL THEN 0 ELSE 1 END AS IS_UNIQUE,
                           CASE WHEN ui.COLUMN_NAME IS NULL THEN 0 ELSE 1 END AS IS_UNIQUE_INDEX,
                           c.TABLE_NAME,
                           c.TABLE_SCHEMA
                           
                       FROM INFORMATION_SCHEMA.COLUMNS c
                       LEFT JOIN (
                           SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                           FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                           JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                             ON ku.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                            AND ku.TABLE_SCHEMA = tc.TABLE_SCHEMA
                            AND ku.TABLE_NAME = tc.TABLE_NAME
                           WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                       ) pk
                         ON pk.TABLE_SCHEMA = c.TABLE_SCHEMA
                        AND pk.TABLE_NAME = c.TABLE_NAME
                        AND pk.COLUMN_NAME = c.COLUMN_NAME
                       LEFT JOIN (
                           SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                           FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                           JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                             ON ku.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                            AND ku.TABLE_SCHEMA = tc.TABLE_SCHEMA
                            AND ku.TABLE_NAME = tc.TABLE_NAME
                           WHERE tc.CONSTRAINT_TYPE = 'UNIQUE'
                       ) uq
                         ON uq.TABLE_SCHEMA = c.TABLE_SCHEMA
                        AND uq.TABLE_NAME = c.TABLE_NAME
                        AND uq.COLUMN_NAME = c.COLUMN_NAME
                       LEFT JOIN (
                           SELECT
                               s.name AS TABLE_SCHEMA,
                               t.name AS TABLE_NAME,
                               col.name AS COLUMN_NAME
                           FROM sys.tables t
                           JOIN sys.schemas s
                             ON s.schema_id = t.schema_id
                           JOIN sys.indexes i
                             ON i.object_id = t.object_id
                            AND i.is_unique = 1
                            AND i.is_hypothetical = 0
                            AND i.type IN (1, 2)
                            AND i.is_primary_key = 0
                           JOIN sys.index_columns ic
                             ON ic.object_id = i.object_id
                            AND ic.index_id = i.index_id
                            AND ic.key_ordinal > 0
                           JOIN sys.columns col
                             ON col.object_id = t.object_id
                            AND col.column_id = ic.column_id
                           GROUP BY s.name, t.name, i.index_id, col.name
                           HAVING COUNT(*) = 1
                       ) ui
                         ON ui.TABLE_SCHEMA = c.TABLE_SCHEMA
                        AND ui.TABLE_NAME = c.TABLE_NAME
                        AND ui.COLUMN_NAME = c.COLUMN_NAME
                       ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION;
                       """;

    var result = new List<TableSchema>();
    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

    var saveTableName = string.Empty;

    var table = new TableSchema("", "", [], []);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
      var colName = reader.GetString(0);
      var isNullable = string.Equals(reader.GetString(1), "YES", StringComparison.OrdinalIgnoreCase);
      var dataType = reader.GetString(2);
      var isAutoIncrement =
        !reader.IsDBNull(3) && Convert.ToInt32(reader.GetValue(3), CultureInfo.InvariantCulture) == 1;
      var length = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
      var scale = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture);
      var isPrimaryKey = Convert.ToInt32(reader.GetValue(6), CultureInfo.InvariantCulture) == 1;
      var isUniqueConstraint = Convert.ToInt32(reader.GetValue(7), CultureInfo.InvariantCulture) == 1;
      var isUniqueIndex = Convert.ToInt32(reader.GetValue(8), CultureInfo.InvariantCulture) == 1;
      var isUnique = isUniqueConstraint || isUniqueIndex || isPrimaryKey;
      var tabName = reader.GetString(9);
      var schema = reader.GetString(10);

      if (discoveredTables.Any(x => x.Name == tabName) is false) continue;

      if (saveTableName != tabName)
      {
        table = new TableSchema(tabName, schema, [], []);
        result.Add(table);
        saveTableName = tabName;
      }

      table.Columns.Add(new ColumnSchema(
        colName,
        MapSqlServerTypeToClr(dataType),
        isNullable,
        isAutoIncrement,
        isPrimaryKey,
        isUnique,
        length,
        scale,
        null));
    }

    return result;
  }

  private static Dictionary<string, IList<IndexSchema>> LoadIndexes(
    DbConnection connection,
    string? schemaName,
    Dictionary<string, TableSchema> tableByName)
  {
    const string sql = """
                       SELECT
                           s.name AS schema_name,
                           t.name AS table_name,
                           i.name AS index_name,
                           i.is_primary_key,
                           i.is_unique,
                           ic.key_ordinal,
                           c.name AS column_name
                       FROM sys.tables t
                       JOIN sys.schemas s ON s.schema_id = t.schema_id
                       JOIN sys.indexes i ON i.object_id = t.object_id AND i.index_id > 0
                       JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                       JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
                       WHERE (@schema IS NULL OR s.name = @schema)
                       ORDER BY s.name, t.name, i.name, ic.key_ordinal;
                       """;

    var result = new Dictionary<string, IList<IndexSchema>>(StringComparer.OrdinalIgnoreCase);
    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

    var param1 = cmd.CreateParameter();
    param1.ParameterName = "@schema";
    param1.IsNullable = true;
    param1.Value = (object?)schemaName ?? DBNull.Value;
    param1.DbType = DbType.String;
    cmd.Parameters.Add(param1);

    using var reader = cmd.ExecuteReader();
    var grouped =
      new Dictionary<(string Table, string Index), (bool Pk, bool Unique, List<(int Ord, string Col)> Cols)>();

    while (reader.Read())
    {
      var table = reader.GetString(1);
      if (!tableByName.ContainsKey(table)) continue;

      var index = reader.GetString(2);
      var pk = reader.GetBoolean(3);
      var unique = reader.GetBoolean(4);
      var ord = Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture);
      var col = reader.GetString(6);

      var key = (table, index);
      if (!grouped.TryGetValue(key, out var state)) state = (pk, unique, []);

      state.Cols.Add((ord, col));
      grouped[key] = state;
    }

    foreach (var ((table, indexName), state) in grouped)
    {
      state.Cols.Sort((a, b) => a.Ord.CompareTo(b.Ord));
      var cols = new List<string>(state.Cols.Count);
      cols.AddRange(state.Cols.Select(c => c.Col));

      if (!result.TryGetValue(table, out var tableIndexes))
      {
        tableIndexes = new List<IndexSchema>();
        result[table] = tableIndexes;
      }

      ((List<IndexSchema>)tableIndexes).Add(new IndexSchema(indexName, state.Pk, state.Unique, cols));
    }

    return result;
  }

  private static List<RelationSchema> LoadRelations(
    DbConnection connection,
    string? schemaName,
    Dictionary<string, TableSchema> tableByName)
  {
    const string sql = """
                       SELECT
                           fk.name AS relation_name,
                           pt.name AS principal_table,
                           pc.name AS principal_column,
                           rt.name AS dependent_table,
                           rc.name AS dependent_column,
                           fkc.constraint_column_id AS ordinal
                       FROM sys.foreign_keys fk
                       JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                       JOIN sys.tables pt ON pt.object_id = fk.referenced_object_id
                       JOIN sys.columns pc ON pc.object_id = pt.object_id AND pc.column_id = fkc.referenced_column_id
                       JOIN sys.tables rt ON rt.object_id = fk.parent_object_id
                       JOIN sys.columns rc ON rc.object_id = rt.object_id AND rc.column_id = fkc.parent_column_id
                       JOIN sys.schemas ps ON ps.schema_id = pt.schema_id
                       JOIN sys.schemas rs ON rs.schema_id = rt.schema_id
                       WHERE (@schema IS NULL OR ps.name = @schema OR rs.name = @schema)
                       ORDER BY fk.name, fkc.constraint_column_id;
                       """;

    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

    var param1 = cmd.CreateParameter();
    param1.ParameterName = "@schema";
    param1.IsNullable = true;
    param1.Value = (object?)schemaName ?? DBNull.Value;
    param1.DbType = DbType.String;
    cmd.Parameters.Add(param1);

    var grouped =
      new Dictionary<string, (string Principal, string Dependent, List<RelationColumnMapping> Cols)>(StringComparer
        .OrdinalIgnoreCase);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
      var relName = reader.GetString(0);
      var principalTable = reader.GetString(1);
      var principalColumn = reader.GetString(2);
      var dependentTable = reader.GetString(3);
      var dependentColumn = reader.GetString(4);
      var ordinal = Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture);

      if (!tableByName.ContainsKey(principalTable) || !tableByName.ContainsKey(dependentTable)) continue;

      if (!grouped.TryGetValue(relName, out var state)) state = (principalTable, dependentTable, []);

      state.Cols.Add(new RelationColumnMapping(principalColumn, dependentColumn, ordinal));
      grouped[relName] = state;
    }

    var relations = new List<RelationSchema>(grouped.Count);
    foreach (var (name, state) in grouped)
    {
      state.Cols.Sort((a, b) => a.Ordinal.CompareTo(b.Ordinal));
      relations.Add(new RelationSchema(name, state.Principal, state.Dependent, state.Cols));
    }

    return relations;
  }

  private static Type MapSqlServerTypeToClr(string sqlType)
  {
    return sqlType.ToLowerInvariant() switch
    {
      "bigint" => typeof(long),
      "int" => typeof(int),
      "smallint" => typeof(short),
      "tinyint" => typeof(byte),
      "bit" => typeof(bool),
      "decimal" or "numeric" or "money" or "smallmoney" => typeof(decimal),
      "float" => typeof(double),
      "real" => typeof(float),
      "date" or "datetime" or "datetime2" or "smalldatetime" or "datetimeoffset" => typeof(DateTime),
      "time" => typeof(TimeSpan),
      "char" or "nchar" or "varchar" or "nvarchar" or "text" or "ntext" or "xml" => typeof(string),
      "binary" or "varbinary" or "image" or "rowversion" or "timestamp" => typeof(byte[]),
      "uniqueidentifier" => typeof(Guid),
      _ => typeof(string)
    };
  }
}