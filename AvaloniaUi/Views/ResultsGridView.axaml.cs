using aDataLib;
using aSql.Converter;
using aSql.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using DataGrid = Avalonia.Controls.DataGrid;
using DataGridTextColumn = Avalonia.Controls.DataGridTextColumn;
using Key = Avalonia.Input.Key;

namespace aSql.Views;

public partial class ResultsGridView : UserControl
{
  private const double GridFontZoomStep = 0.5;
  private const double DefaultGridFontSize = 13.0;
  private const double MinGridFontSize = 6.0;
  private bool _isUpdateRunning;

  private object? _originalCellValueBackup;

  public ResultsGridView()
  {
    InitializeComponent();
    InitializeColumns();
    InitializeGridZoom();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }

  private void InitializeGridZoom()
  {
    if (this.FindControl<DataGrid>("ResultsGrid") is not { } grid) return;

    grid.AddHandler(
      PointerWheelChangedEvent,
      OnResultsGridPointerWheelChanged,
      RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
      true);
  }

  private static void OnResultsGridPointerWheelChanged(object? sender, PointerWheelEventArgs e)
  {
    if (sender is not DataGrid grid || e.KeyModifiers.HasFlag(KeyModifiers.Control) is false) return;

    switch (e.Delta.Y)
    {
      case > 0:
        ApplyGridFontSize(grid, grid.FontSize + GridFontZoomStep);
        e.Handled = true;
        break;
      case < 0:
        ApplyGridFontSize(grid, Math.Max(MinGridFontSize, grid.FontSize - GridFontZoomStep));
        e.Handled = true;
        break;
    }
  }

  private static void ApplyGridFontSize(DataGrid grid, double fontSize)
  {
    grid.FontSize = fontSize;
  }

  private void StandardSettingsRestoreButton_OnClick(object? sender, RoutedEventArgs e)
  {
    if (this.FindControl<DataGrid>("ResultsGrid") is not { } grid) return;

    ApplyGridFontSize(grid, DefaultGridFontSize);
  }

  private void InitializeColumns()
  {
    if (this.FindControl<DataGrid>("ResultsGrid") is not { } grid) return;

    this.GetObservable(DataContextProperty).Subscribe(dc =>
    {
      if (dc is not SqlEditorViewModel sqlEditorVm) return;

      if (sqlEditorVm.ResultsGrid is not { } vm) return;

      vm.PropertyChanged += (_, args) =>
      {
        if (args.PropertyName != nameof(ResultsGridViewModel.ColumnNames)) return;
        if (vm.ColumnNames != null) RebuildColumns(grid, vm.ColumnNames, vm.ReaderColumnNames);
      };

      if (vm.ColumnNames is { Count: > 0 }) RebuildColumns(grid, vm.ColumnNames, vm.ReaderColumnNames);
    });
  }

  private static void RebuildColumns(DataGrid grid, IReadOnlyList<DatDefTable.DatDefTableField> columnNames, string[]? readerColumnNames)
  {
    grid.Columns.Clear();

    if (readerColumnNames is null) return;

    foreach (var r in readerColumnNames)
    {
      var t = columnNames.FirstOrDefault(c => c.Name == r || c.Alias == r);
      grid.Columns.Add(new DataGridTextColumn
      {
        Header = r,
        IsReadOnly = t is null || t.MyTable.HasUniqueIndex is false,
        Binding = new Binding($"[{r}]")
        {
          Converter = new NullToNullStringConverter(),
          Mode = BindingMode.TwoWay
        }
      });
      var header = GetHeaderFromColumn(grid, r);
      header?.Foreground = t?.MyTable.HasUniqueIndex == true
        ? new SolidColorBrush(Colors.YellowGreen)
        : new SolidColorBrush(Colors.OrangeRed);
    }
  }

  public static DataGridColumnHeader? GetHeaderFromColumn(DataGrid myDataGrid, string bez)
  {
    var headers = myDataGrid.GetVisualDescendants()
      .OfType<DataGridColumnHeader>();

    return headers.FirstOrDefault(h => h.Content?.ToString() == bez);
  }

  private async void ResultsGrid_OnKeyDown(object? sender, KeyEventArgs e)
  {
    try
    {
      var modifiers = e.KeyModifiers;
      var isCtrlPressed = modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Meta);

      if (!isCtrlPressed || e.Key != Key.C) return;
      if (sender is not DataGrid dataGrid || dataGrid.CurrentColumn == null || dataGrid.SelectedItem == null) return;
      e.Handled = true;

      var currentColumn = dataGrid.CurrentColumn;
      var selectedItem = dataGrid.SelectedItem;

      var cellContent = currentColumn.GetCellContent(selectedItem);
      if (cellContent is not TextBlock textBlock) return;
      var textToCopy = textBlock.Text ?? string.Empty;

      var topLevel = TopLevel.GetTopLevel(dataGrid);
      if (topLevel?.Clipboard != null) await topLevel.Clipboard.SetTextAsync(textToCopy);
    }
    catch
    {
      // ignored
    }
  }

  private void ResultsGrid_OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
  {
    HandleUpdateCell(e);
  }

  private void HandleUpdateCell(DataGridCellEditEndingEventArgs e)
  {
    if (e.EditAction == DataGridEditAction.Cancel) return;

    try
    {
      if (_isUpdateRunning) return;
      if (this.FindControl<DataGrid>("ResultsGrid") is not { } grid) return;
      if (DataContext is not SqlEditorViewModel { ResultsGrid: { } resultsGridVm } ||
          e.Row?.DataContext is not DynamicRowWrapper dRow || grid.CurrentColumn is null) return;
      _isUpdateRunning = true;
      var fieldName = grid.CurrentColumn.Header?.ToString() ?? string.Empty;
      var editingTextBox = e.EditingElement as TextBox;
      var newValue = editingTextBox?.Text ?? string.Empty;
      if (resultsGridVm.UpdateCell(fieldName, newValue, dRow)) return;
      e.Cancel = true;
      dRow[fieldName] = _originalCellValueBackup;
      editingTextBox?.Text = _originalCellValueBackup?.ToString() ?? string.Empty;
      grid.CancelEdit(DataGridEditingUnit.Cell);
    }
    catch (Exception ex)
    {
      if (DataContext is SqlEditorViewModel sqleditVm) sqleditVm.StatusMessage = $"Error updating cell: {ex.Message}";
    }
    finally
    {
      _isUpdateRunning = false;
    }
  }

  private void ResultsGrid_OnBeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
  {
    if (e.Row.DataContext is not DynamicRowWrapper row) return;
    var columnName = e.Column.Header?.ToString() ?? "";
    _originalCellValueBackup = row[columnName];
  }
}