using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;

namespace aSql.ViewModels;

public sealed record StatusMessageEntry(string Text, IBrush Foreground);

public sealed class StatusMessageViewModel : ViewModelBase
{
  private static readonly IBrush InfoBrush = new SolidColorBrush(Color.Parse("#FF6495ED"));
  private static readonly IBrush SuccessBrush = new SolidColorBrush(Color.Parse("#FF9BCB9B"));
  private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.Parse("#FFCC8F8F"));

  private string _message = string.Empty;

  public StatusMessageViewModel()
  {
    ClearMessageCommand = ReactiveCommand.Create(ClearMessage);
  }

  public ObservableCollection<StatusMessageEntry> Entries { get; } = [];

  public string Message
  {
    get => _message;
    set
    {
      if (_message == value) return;

      this.RaiseAndSetIfChanged(ref _message, value);
      this.RaisePropertyChanged(nameof(CanClearMessage));
    }
  }

  public bool CanClearMessage => Entries.Count > 0;

  public ICommand ClearMessageCommand { get; }

  public void AppendMessageLine(string message)
  {
    if (string.IsNullOrWhiteSpace(message)) return;

    var normalized = message.TrimEnd('\r', '\n');
    if (string.IsNullOrWhiteSpace(normalized)) return;

    Entries.Insert(0, new StatusMessageEntry(normalized, DetermineForegroundBrush(normalized)));

    Message = string.IsNullOrWhiteSpace(_message)
      ? normalized
      : normalized + Environment.NewLine + _message;
  }

  private static IBrush DetermineForegroundBrush(string message)
  {
    if (string.IsNullOrWhiteSpace(message)) return SuccessBrush;

    if (message.StartsWith("Führe Sql aus", StringComparison.InvariantCultureIgnoreCase)) return InfoBrush;

    if (message.IndexOf("fehler", StringComparison.OrdinalIgnoreCase) >= 0 ||
        message.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 ||
        message.IndexOf("abgebrochen", StringComparison.OrdinalIgnoreCase) >= 0)
      return ErrorBrush;

    return SuccessBrush;
  }

  private void ClearMessage()
  {
    Entries.Clear();
    Message = string.Empty;
  }
}