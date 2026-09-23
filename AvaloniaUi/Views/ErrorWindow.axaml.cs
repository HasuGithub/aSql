using Avalonia.Controls;
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
}