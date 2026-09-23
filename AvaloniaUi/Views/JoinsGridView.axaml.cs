using aSql.ViewModels;
using Avalonia.Input;
using Avalonia.Interactivity;
using UserControl = Avalonia.Controls.UserControl;

namespace aSql.Views;

public partial class JoinsGridView : UserControl
{
  public JoinsGridView()
  {
    InitializeComponent();
    AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    AddHandler(KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
  }

  public void OnKeyDown(object? sender, KeyEventArgs e)
  {
    if (DataContext is not JoinsGridViewModel vm) return;
    // 1. Möglichkeit: Der Nutzer drückt gerade aktiv die Strg-Taste herunter
    if (e.Key is Key.LeftCtrl or Key.RightCtrl)
      vm.IsKeyCtrlDown = true;
    // 2. Möglichkeit: Der Nutzer hält Strg gedrückt und drückt eine andere Taste (z.B. Strg + A)
    else if ((e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control) vm.IsKeyCtrlDown = true;
  }

  public void OnKeyUp(object? sender, KeyEventArgs e)
  {
    if (DataContext is not JoinsGridViewModel vm) return;
    // Sobald die Strg-Taste wieder losgelassen wird, den Zustand zurücksetzen
    if (e.Key is Key.LeftCtrl or Key.RightCtrl) vm.IsKeyCtrlDown = false;
  }
}