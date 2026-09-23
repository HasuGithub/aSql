using System.Data.Common;

namespace aDataLib.Schema;

internal sealed class SchemaProviderContext(ISchemaProvider provider, DbConnection connection, bool ownsConnection)
  : IDisposable
{
  public ISchemaProvider Provider { get; } = provider ?? throw new ArgumentNullException(nameof(provider));

    public DbConnection Connection { get; } = connection ?? throw new ArgumentNullException(nameof(connection));

    public void Dispose()
    {
        if (ownsConnection)
        {
            Connection.Dispose();
        }
    }
}
