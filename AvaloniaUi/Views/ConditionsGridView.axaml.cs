using aSql.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DataGrid = Avalonia.Controls.DataGrid;
using DataGridCell = Avalonia.Controls.DataGridCell;
using TextBox = Avalonia.Controls.TextBox;

namespace aSql.Views;

public partial class ConditionsGridView : UserControl
{
  public ConditionsGridView()
  {
    InitializeComponent();
    CondsGrid.SelectionChanged += CondsGrid_SelectionChanged;

    CondsGrid.CurrentCellChanged += CondsGrid_CurrentCellChanged;
  }

  private void CondsGrid_CurrentCellChanged(object? sender, EventArgs e)
  {
    if (DataContext is ConditionsGridViewModel vm)
      // 3 = Linker Operand , 6 = rechter Operand
      // Vorsicht: Wenn sich das Grid noch erweitern sollte, passen die Indize nicht mehr!
      vm.CurrentColNumber = CondsGrid.Columns.IndexOf(CondsGrid.CurrentColumn);
  }

  private void CondsGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    if (DataContext is ConditionsGridViewModel vm) vm.CurrentRowNumber = CondsGrid.SelectedIndex;
  }

  private void CondsGrid_OnGotFocus(object? sender, FocusChangedEventArgs e)
  {
    if (e.Source is DataGrid or DataGridCell or TextBox or DataGridCell)
      Dispatcher.UIThread.Post(() => { CondsGrid.BeginEdit(); }, DispatcherPriority.Background);
  }
}