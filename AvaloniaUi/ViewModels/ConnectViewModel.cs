using System.Collections.ObjectModel;
using System.Windows.Input;

using aDataLib;

using aSql.Models;
using aSql.Services;

using ReactiveUI;

namespace aSql.ViewModels;

public sealed class ConnectViewModel : ViewModelBase
{
  private DbConnectionEntry? _selectedConnection;

  public ConnectViewModel()
  {
    ConnectCommand = ReactiveCommand.Create(OnConnect);
    CancelCommand = ReactiveCommand.Create(OnCancel);
    ResetCommand = ReactiveCommand.Create(OnReset);

    foreach (var entry in ConnectionStore.Load()!)
      RecentConnections.Add(entry);

    if (RecentConnections.Count > 0)
      SelectedConnection = RecentConnections[0];
  }

  public IReadOnlyList<string> AvailableProviders { get; } =
    ["SQL Server", "Oracle", "MySQL", "MariaDB"];

  public IReadOnlyList<string> AvailableAuthTypes { get; } =
    ["SQL Server-Authentifizierung", "Windows-Authentifizierung"];

  public bool Encrypt { get; set; }

  public ObservableCollection<DbConnectionEntry> RecentConnections { get; } = [];

  public DbConnectionEntry? SelectedConnection
  {
    get => _selectedConnection;
    set
    {
      this.RaiseAndSetIfChanged(ref _selectedConnection, value);
      if (value != null)
        PopulateFromEntry(value);
    }
  }

  public string ConnectionAlias
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanConnect));
    }
  } = string.Empty;

  public string SelectedProvider
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(IsAuthTypeVisible));
    }
  } = "SQL Server";

  public string ServerName
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanConnect));
    }
  } = string.Empty;

  public string Port
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanConnect));
    }
  } = string.Empty;


  public string SelectedAuthType
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = "SQL Server-Authentifizierung";

  public bool IsAuthTypeVisible => SelectedProvider == "SQL Server";

  public bool IsPortVisible => SelectedProvider == "MariaDB";

  public string UserName
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanConnect));
    }
  } = string.Empty;

  public string Password
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanConnect));
    }
  } = string.Empty;

  public bool SavePassword
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public string DatabaseName
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(CanConnect));
    }
  } = string.Empty;

  public bool TrustServerCertificate
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = true;

  public string StatusMessage
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = string.Empty;

  public DbConnect? Connection { get; private set; }

  public bool CanConnect =>
    !string.IsNullOrWhiteSpace(ConnectionAlias) &&
    !string.IsNullOrWhiteSpace(ServerName) &&
    !string.IsNullOrWhiteSpace(UserName) &&
    !string.IsNullOrWhiteSpace(DatabaseName);

  public ICommand ConnectCommand { get; }
  public ICommand CancelCommand { get; }
  public ICommand ResetCommand { get; }

  public event EventHandler? ConnectRequested;
  public event EventHandler? CancelRequested;

  private void PopulateFromEntry(DbConnectionEntry entry)
  {
    ConnectionAlias = entry.Alias;
    SelectedProvider = entry.Provider switch
    {
      DbProvider.Oracle => "Oracle",
      DbProvider.MySql => "MySQL",
      DbProvider.MariaDb => "MariaDB",
      _ => "SQL Server"
    };
    ServerName = entry.ServerName;
    Port = entry.Port;
    UserName = entry.UserName;
    DatabaseName = entry.DatabaseName;
    Encrypt = entry.Encrypt;
    TrustServerCertificate = entry.TrustServerCertificate;
    SavePassword = entry.SavePassword;
    Password = entry.SavePassword
      ? ConnectionStore.DecryptPassword(entry.PasswordEncrypted)
      : string.Empty;
  }

  private void OnConnect()
  {
    StatusMessage = "Stelle Verbindung her …";
    try
    {
      var dbType = SelectedProvider switch
      {
        "Oracle" => DataBaseTypes.Oracle,
        "MySQL" => DataBaseTypes.MySql,
        "MariaDB" => DataBaseTypes.Maria,
        _ => DataBaseTypes.MsSqlServer
      };

      var entry = BuildCurrentEntry();
      var connStr = ConnectionStore.BuildConnectionString(entry, Password);

      Connection = DbConnect.CreateWithConnectionString(
        connStr,
        dbType,
        false,
        null,
        dbType == DataBaseTypes.Oracle ? entry.DatabaseName : "");

      PersistEntry(entry);
      StatusMessage = "Verbindung erfolgreich.";
      ConnectRequested?.Invoke(this, EventArgs.Empty);
    }
    catch (Exception ex)
    {
      StatusMessage = "Verbindung fehlgeschlagen: " + ex.Message;
    }
  }

  private DbConnectionEntry BuildCurrentEntry()
  {
    return new DbConnectionEntry
    {
      Alias = ConnectionAlias,
      Provider = MapProvider(SelectedProvider),
      ServerName = ServerName,
      Port = Port,
      UserName = UserName,
      DatabaseName = DatabaseName,
      Encrypt = Encrypt,
      TrustServerCertificate = TrustServerCertificate,
      SavePassword = SavePassword,
      PasswordEncrypted = SavePassword
        ? ConnectionStore.EncryptPassword(Password)
        : string.Empty,
      LastUsed = DateTime.UtcNow
    };
  }

  private void PersistEntry(DbConnectionEntry entry)
  {
    var existing = RecentConnections.FirstOrDefault(e =>
      string.Equals(e.Alias, entry.Alias, StringComparison.OrdinalIgnoreCase));
    if (existing != null)
      RecentConnections.Remove(existing);

    RecentConnections.Insert(0, entry);

    ConnectionStore.Save(RecentConnections);
  }

  private static DbProvider MapProvider(string display)
  {
    return display switch
    {
      "Oracle" => DbProvider.Oracle,
      "MySQL" => DbProvider.MySql,
      "MariaDB" => DbProvider.MariaDb,
      _ => DbProvider.SqlServer
    };
  }

  private void OnCancel()
  {
    CancelRequested?.Invoke(this, EventArgs.Empty);
  }

  private void OnReset()
  {
    _selectedConnection = null;
    this.RaisePropertyChanged(nameof(SelectedConnection));
    ConnectionAlias = string.Empty;
    SelectedProvider = "SQL Server";
    ServerName = string.Empty;
    Port = string.Empty;
    SelectedAuthType = "SQL Server-Authentifizierung";
    UserName = string.Empty;
    Password = string.Empty;
    SavePassword = false;
    DatabaseName = string.Empty;
    Encrypt = false;
    TrustServerCertificate = true;
    StatusMessage = string.Empty;
  }
}