using aSql.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace aSql.Views;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
  }

  public MainWindow(MainWindowViewModel viewModel)
  {
    InitializeComponent();
    DataContext = viewModel;
  }

  private void ButtonNewInstance_OnClick(object? sender, RoutedEventArgs e)
  {
    var mainVm = new MainWindowViewModel
    {
      UseTopEnabled = true,
      UseUpperCaseSql = true,
      UseSimpleDate = true
    };

    if (DataContext is MainWindowViewModel { DbConnect: not null } myVm)
      mainVm.DbConnectRefresh(myVm.DbConnect, myVm.DbTreeView);

    var newMainWindow = new MainWindow(mainVm)
    {
      Owner = this,
      WindowStartupLocation = WindowStartupLocation.Manual,
      Position = new PixelPoint(
        Position.X + 35,
        Position.Y + 35
      )
    };

    // so könnte man dann alles schließen
    // this.Closed += (s, eventArgs) => newMainWindow.Close();

    newMainWindow.Show();
  }

  private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
  {
    // Minimiert das aktuelle Fenster
    WindowState = WindowState.Minimized;
  }

  private void CloseButton_Click(object? sender, RoutedEventArgs e)
  {
    // Schließt das aktuelle Fenster 
    Close();
  }
}