using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace aSql.Views;

public partial class ErrorWindow : Window
{
  public ErrorWindow()
  {
    InitializeComponent();
  }

  public ErrorWindow(string message, string stackTrace) : this()
  {
    ErrorMessageText.Text = message;
    StackTraceText.Text = stackTrace;
  }

  private void CloseButton_Click(object? sender, RoutedEventArgs e)
  {
    Close();
  }

  private void CopyErrorTextButton_OnClick(object? sender, RoutedEventArgs e)
  {
    try
    {
      var clipboard = GetTopLevel(this)?.Clipboard;
      if (clipboard is null) return;

      var errMsg = ErrorMessageText.Text + Environment.NewLine + StackTraceText.Text;

      clipboard.SetTextAsync(errMsg);
    }
    catch
    {
      // Ignored
    }

  }
}