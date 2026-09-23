using System.Data.Common;

namespace aDataLib.Schema;

public interface ISchemaProvider
{
  DatabaseSchema LoadSchema(DbConnection connection, SchemaLoadOptions options);
}