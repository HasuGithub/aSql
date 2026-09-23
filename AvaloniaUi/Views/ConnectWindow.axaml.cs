using aSql.ViewModels;
using Avalonia.Controls;

namespace aSql.Views;

public partial class ConnectWindow : Window
{
  public ConnectWindow() : this(new ConnectViewModel())
  {
  }

  public ConnectWindow(ConnectViewModel vm)
  {
    InitializeComponent();
    DataContext = vm;

    vm.ConnectRequested += (_, _) =>
    {
      DialogResult = true;
      Close();
    };

    vm.CancelRequested += (_, _) =>
    {
      DialogResult = false;
      Close();
    };
  }

  public bool? DialogResult { get; private set; }
}