using System.Data;
using System.Data.Common;
using System.Globalization;

namespace aDataLib.Schema;

public sealed class MySqlSchemaProvider : ISchemaProvider
{
  public DatabaseSchema LoadSchema(DbConnection connection, SchemaLoadOptions options)
  {
    ArgumentNullException.ThrowIfNull(connection);
    ArgumentNullException.ThrowIfNull(options);

    if (connection.State != ConnectionState.Open)
      throw new InvalidOperationException("Database connection must be open to load schema.");

    var schemaName = string.IsNullOrWhiteSpace(options.SchemaName) ? connection.Database : options.SchemaName;

    var tables = LoadTables(connection, schemaName, out var tableByName);
    var tableIndexes = LoadIndexes(connection, schemaName, tableByName);
    var relations = LoadRelations(connection, schemaName, tableByName);

    foreach (var (key, values) in tableIndexes)
    foreach (var val in values)
    {
      if (val.IsPrimaryKey || !val.IsUnique) continue;
      var tab = tables.FirstOrDefault(x => x.Name == key);
      if (tab == null) continue;
      foreach (var col in val.Columns)
      {
        var tabCol = tab.Columns.FirstOrDefault(z => z.Name == col);
        tabCol?.IsUnique = true;
      }
    }

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
    string schemaName,
    out Dictionary<string, TableSchema> tableByName)
  {
    var primList = new List<string>();

    const string primSql = """
                           SELECT 
                               TABLE_NAME, 
                               COLUMN_NAME
                           FROM 
                               INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                           WHERE 
                               TABLE_SCHEMA = @schema AND CONSTRAINT_NAME = 'PRIMARY'
                           ORDER BY 
                               TABLE_NAME, ORDINAL_POSITION
                           """;

    using var cmdPrims = connection.CreateCommand();
    cmdPrims.CommandText = primSql;
    var paramDbPrim = cmdPrims.CreateParameter();
    paramDbPrim.ParameterName = "@schema";
    paramDbPrim.Value = schemaName;
    cmdPrims.Parameters.Add(paramDbPrim);

    using var readerPrim = cmdPrims.ExecuteReader();
    while (readerPrim.Read())
    {
      var tableName = readerPrim.GetString(0);
      var colName = readerPrim.GetString(1);
      primList.Add(tableName + "_" + colName);
    }

    readerPrim.Close();

    const string sql = """
                       SELECT 
                           TABLE_NAME, 
                           COLUMN_NAME, 
                           DATA_TYPE, 
                           CHARACTER_MAXIMUM_LENGTH, 
                           NUMERIC_PRECISION, 
                           NUMERIC_SCALE, 
                           IS_NULLABLE, 
                           EXTRA
                       FROM 
                           INFORMATION_SCHEMA.COLUMNS
                       WHERE 
                           TABLE_SCHEMA = @schema
                       ORDER BY 
                           TABLE_NAME, ORDINAL_POSITION
                       """;

    var tables = new List<TableSchema>();
    tableByName = new Dictionary<string, TableSchema>(StringComparer.OrdinalIgnoreCase);

    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;
    var paramDb = cmd.CreateParameter();
    paramDb.ParameterName = "@schema";
    paramDb.Value = schemaName;
    cmd.Parameters.Add(paramDb);

    var saveTableName = string.Empty;

    {
      List<ColumnSchema> aktColumns = null!;
      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        var tableName = reader.GetString(0);
        var schema = schemaName;

        if (saveTableName != tableName)
        {
          aktColumns = [];
          var aktTable = new TableSchema(tableName, schema, aktColumns, []);
          tables.Add(aktTable);
          tableByName[tableName] = aktTable;
          saveTableName = tableName;
        }

        var colName = reader.GetString(1);
        var isNullable = reader.GetString(6).Equals("YES", StringComparison.OrdinalIgnoreCase);
        var dataType = reader.GetString(2);

        var isAutoIncrement = reader.GetString(7).Contains("auto_increment", StringComparison.OrdinalIgnoreCase);

        var length = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3), CultureInfo.InvariantCulture);
        var scale = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture);

        var isPrimaryKey = primList.Any(x => x == tableName + "_" + colName);

        aktColumns.Add(new ColumnSchema(
          colName,
          MapMySqlTypeToClr(dataType),
          isNullable,
          isAutoIncrement,
          isPrimaryKey,
          isPrimaryKey,
          length,
          scale,
          null));
      }

      reader.Close();
    }

    return tables;
  }

  private static Dictionary<string, IList<IndexSchema>> LoadIndexes(
    DbConnection connection,
    string schemaName,
    Dictionary<string, TableSchema> tableByName)
  {
    const string sql = """
                       SELECT  
                           TABLE_NAME, 
                           INDEX_NAME, 
                           NON_UNIQUE, 
                           COLUMN_NAME, 
                           SEQ_IN_INDEX
                       FROM  
                           INFORMATION_SCHEMA.STATISTICS
                       WHERE  
                           TABLE_SCHEMA = @schema
                       ORDER BY  
                           TABLE_NAME,  
                           INDEX_NAME,  
                           SEQ_IN_INDEX
                       """;

    var result = new Dictionary<string, IList<IndexSchema>>(StringComparer.OrdinalIgnoreCase);
    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;
    var paramDb = cmd.CreateParameter();
    paramDb.ParameterName = "@schema";
    paramDb.Value = schemaName;
    cmd.Parameters.Add(paramDb);

    using var reader = cmd.ExecuteReader();
    var indexes =
      new Dictionary<(string Table, string Index), (bool Pk, bool Unique, List<(int Ord, string Col)> Cols)>();

    while (reader.Read())
    {
      var table = reader.GetString(0);
      if (!tableByName.ContainsKey(table)) continue;

      var indexName = reader.GetString(1);
      var isPk = indexName.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase);

      var isUnique = reader.GetInt32(2) == 0;
      var ord = Convert.ToInt32(reader.GetValue(4), CultureInfo.InvariantCulture);
      var col = reader.GetString(3);

      var key = (table, indexName);
      if (!indexes.TryGetValue(key, out var state)) state = (isPk, isUnique, []);

      state.Cols.Add((ord, col));
      indexes[key] = state;
    }

    foreach (var ((table, indexName), state) in indexes)
    {
      state.Cols.Sort((a, b) => a.Ord.CompareTo(b.Ord));
      var cols = state.Cols.Select(c => c.Col).ToList();

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
    string schemaName,
    Dictionary<string, TableSchema> tableByName)
  {
    const string sql = """
                       SELECT  
                           kcu.TABLE_NAME AS child_table, 
                           kcu.CONSTRAINT_NAME AS fk_name, 
                           kcu.COLUMN_NAME AS child_column, 
                           kcu.REFERENCED_TABLE_NAME AS parent_table, 
                           kcu.REFERENCED_COLUMN_NAME AS parent_column, 
                           kcu.ORDINAL_POSITION AS position
                       FROM  
                           INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                       WHERE  
                           kcu.TABLE_SCHEMA = @schema 
                           AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
                       ORDER BY  
                           child_table,  
                           fk_name,  
                           position
                       """;

    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;
    var paramDb = cmd.CreateParameter();
    paramDb.ParameterName = "@schema";
    paramDb.Value = schemaName;
    cmd.Parameters.Add(paramDb);

    var grouped =
      new Dictionary<string, (string Principal, string Dependent, List<RelationColumnMapping> Cols)>(StringComparer
        .OrdinalIgnoreCase);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
      var relName = reader.GetString(1);
      var dependentTable = reader.GetString(0); // child
      var dependentColumn = reader.GetString(2); // child_col
      var principalTable = reader.GetString(3); // parent
      var principalColumn = reader.GetString(4); // parent_col
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

  private static Type MapMySqlTypeToClr(string mySqlType)
  {
    return mySqlType.ToLowerInvariant() switch
    {
      // Ganzzahlen
      "tinyint" => typeof(sbyte),
      "smallint" => typeof(short),
      "mediumint" or "int" or "integer" => typeof(int),
      "bigint" => typeof(long),

      // Numerische Gleitkommatypen
      "decimal" or "numeric" => typeof(decimal),
      "float" => typeof(float),
      "double" or "real" => typeof(double),

      // Wahrheitswerte (MySQL nutzt intern tinyint(1) für bool)
      "bit" or "boolean" => typeof(bool),

      // Datums- und Zeitwerte
      "date" or "datetime" or "timestamp" => typeof(DateTime),
      "time" => typeof(TimeSpan),
      "year" => typeof(int),

      // Zeichenketten (Texte)
      "char" or "varchar" or "tinytext" or "text" or "mediumtext" or "longtext" or "json" => typeof(string),

      // Binärdaten (Blobs)
      "binary" or "varbinary" or "tinyblob" or "blob" or "mediumblob" or "longblob" => typeof(byte[]),

      // Standard-Fallback
      _ => typeof(string)
    };
  }
}