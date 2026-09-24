using System.Data;
using System.Data.Common;
using System.Globalization;

namespace aDataLib.Schema;

public sealed class OracleSchemaProvider : ISchemaProvider
{
  public DatabaseSchema LoadSchema(DbConnection connection, SchemaLoadOptions options)
  {
    ArgumentNullException.ThrowIfNull(connection);
    ArgumentNullException.ThrowIfNull(options);

    if (connection.State != ConnectionState.Open)
      throw new InvalidOperationException("Database connection must be open to load schema.");

    var tables = LoadTables(connection, out var tableByName);
    var tableIndexes = LoadIndexes(connection, tableByName);
    var relations = LoadRelations(connection, tableByName);

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
    out Dictionary<string, TableSchema> tableByName)
  {
    var primList = new List<string>();

    const string primSql = """
                           SELECT  
                               cc.table_name, 
                               cc.constraint_name AS pk_name, 
                               cc.column_name, 
                               cc.position AS column_position 
                           FROM  
                               user_constraints c 
                           JOIN  
                               user_cons_columns cc ON c.constraint_name = cc.constraint_name 
                           WHERE  
                               c.constraint_type = 'P' -- 'P' steht für Primary Key 
                           ORDER BY  
                               cc.table_name,  
                               cc.position
                           """;
    using var cmdPrims = connection.CreateCommand();
    cmdPrims.CommandText = primSql;

    using var readerPrim = cmdPrims.ExecuteReader();
    while (readerPrim.Read())
    {
      var tableName = readerPrim.GetString(0);
      var colName = readerPrim.GetString(2);

      primList.Add(tableName + "_" + colName);
    }

    readerPrim.Close();

    const string sql = """
                       SELECT tc.table_name, tc.column_name, tc.data_type, tc.data_length, tc.data_precision, tc.data_scale, tc.nullable, ic.generation_type AS is_identity
                       FROM user_tab_columns tc 
                       LEFT JOIN user_tab_cols col ON tc.table_name = col.table_name AND tc.column_name = col.column_name 
                       LEFT JOIN user_tab_identity_cols ic ON tc.table_name = ic.table_name AND tc.column_name = ic.column_name 
                       ORDER BY tc.table_name, tc.column_id
                       """;

    var tables = new List<TableSchema>();
    tableByName = new Dictionary<string, TableSchema>(StringComparer.OrdinalIgnoreCase);

    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

    var saveTableName = string.Empty;

    {
      List<ColumnSchema> aktColumns = null!;
      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        var tableName = reader.GetString(0);
        var schema = string.Empty;

        if (saveTableName != tableName)
        {
          aktColumns = [];
          var aktTable = new TableSchema(tableName, schema, aktColumns, []);
          tables.Add(aktTable);
          tableByName[tableName] = aktTable;
          saveTableName = tableName;
        }

        var colName = reader.GetString(1);
        var isNullable = reader.GetBoolean(6);
        var dataType = reader.GetString(2);
        var isAutoIncrement = !reader.IsDBNull(7) && reader.GetString(7).Length > 0;
        var length = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
        var scale = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture);

        aktColumns.Add(new ColumnSchema(
          colName,
          MapOracleTypeToClr(dataType),
          isNullable,
          isAutoIncrement,
          primList.Any(x => x == tableName + "_" + colName),
          primList.Any(x => x == tableName + "_" + colName),
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
    Dictionary<string, TableSchema> tableByName)
  {
    const string sql = """
                       SELECT  
                           i.table_name, 
                           i.index_name, 
                           i.uniqueness, 
                           i.index_type, 
                           ic.column_name, 
                           ic.column_position,
                           CASE 
                               WHEN c.constraint_type = 'P' THEN 'YES' 
                               ELSE 'NO' 
                           END AS is_primary_key
                       FROM  
                           user_indexes i 
                       JOIN  
                           user_ind_columns ic ON i.index_name = ic.index_name 
                       LEFT JOIN 
                           user_constraints c ON i.index_name = c.index_name 
                                              AND i.table_name = c.table_name
                       ORDER BY  
                           i.table_name,  
                           i.index_name,  
                           ic.column_position
                       """;

    var result = new Dictionary<string, IList<IndexSchema>>(StringComparer.OrdinalIgnoreCase);
    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

    using var reader = cmd.ExecuteReader();
    var indexes =
      new Dictionary<(string Table, string Index), (bool Pk, bool Unique, List<(int Ord, string Col)> Cols)>();

    while (reader.Read())
    {
      var table = reader.GetString(0);
      if (!tableByName.ContainsKey(table)) continue;

      var index = reader.GetString(1);
      var pk = reader.GetBoolean(6);
      var unique = reader.GetString(2).Equals("UNIQUE", StringComparison.CurrentCultureIgnoreCase);
      var ord = Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture);
      var col = reader.GetString(4);

      var key = (table, index);
      if (!indexes.TryGetValue(key, out var state)) state = (pk, unique, []);

      state.Cols.Add((ord, col));
      indexes[key] = state;
    }

    foreach (var ((table, indexName), state) in indexes)
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
    Dictionary<string, TableSchema> tableByName)
  {
    const string sql = """
                       SELECT  
                           a.table_name AS child_table, 
                           a.constraint_name AS fk_name, 
                           a.column_name AS child_column, 
                           c_pk.table_name AS parent_table, 
                           b.column_name AS parent_column, 
                           a.position
                       FROM  
                           user_cons_columns a 
                       JOIN  
                           user_constraints c ON a.constraint_name = c.constraint_name 
                       JOIN  
                           user_constraints c_pk ON c.r_constraint_name = 
                       c_pk.constraint_name 
                       JOIN  
                           user_cons_columns b ON c_pk.constraint_name = b.constraint_name 
                       AND a.position = b.position 
                       WHERE  
                           c.constraint_type = 'R'

                       ORDER BY  
                           child_table,  
                           fk_name,  
                           a.position
                       """;

    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

    var grouped =
      new Dictionary<string, (string Principal, string Dependent, List<RelationColumnMapping> Cols)>(StringComparer
        .OrdinalIgnoreCase);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
      var relName = reader.GetString(1);
      var principalTable = reader.GetString(3);
      var principalColumn = reader.GetString(2);
      var dependentTable = reader.GetString(0);
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

  private static Type MapOracleTypeToClr(string oracleType)
  {
    return oracleType.ToLowerInvariant() switch
    {
      // Numerische Typen (NUMBER wird oft für alles verwendet)
      "number" or "numeric" or "decimal" => typeof(decimal),
      "float" or "double precision" => typeof(double),
      "binary_float" => typeof(float),
      "binary_double" => typeof(double),

      // Ganzzahlen (falls spezifisch als INTEGER/pls_integer in PL/SQL oder Aliase genutzt)
      "integer" or "int" or "smallint" => typeof(int),

      // Datums- und Zeitwerte
      "date" or "timestamp" or "timestamp(6)" or "timestamp with time zone" or "timestamp with local time zone" =>
        typeof(DateTime),
      "interval day to second" => typeof(TimeSpan),
      "interval year to month" => typeof(long), // Repräsentiert Monate als Zahl

      // Zeichenketten (Texte)
      "char" or "nchar" or "varchar2" or "nvarchar2" or "clob" or "nclob" or "long" => typeof(string),

      // Binärdaten (Large Objects und Raw)
      "raw" or "long raw" or "blob" or "bfile" => typeof(byte[]),

      // Oracle-spezifische Identifikatoren (z.B. RAW(16) für GUIDs)
      "rowid" or "urowid" => typeof(string),

      // Standard-Fallback
      _ => typeof(string)
    };
  }
}