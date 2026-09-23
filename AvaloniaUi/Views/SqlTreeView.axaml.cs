using aSql.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using AControl = Avalonia.Controls.Control;

namespace aSql.Views;

public partial class SqlTreeView : UserControl
{
  public SqlTreeView()
  {
    InitializeComponent();
  }

  private void OnNodeDoubleTapped(object? sender, TappedEventArgs e)
  {
    var hlp = TopLevel.GetTopLevel(this);

    if (hlp is not Window { DataContext: MainWindowViewModel }) return;

    // Sender ist der Header-Container im ItemTemplate.
    if (sender is not AControl source) return;

    if (source.DataContext is not DataTreeNode) return;

    // Event als behandelt markieren, damit der TreeView den Doppelklick
    // nicht mehr für Expand/Collapse auswertet.
    e.Handled = true;
  }
}