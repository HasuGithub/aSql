using aSql.Converter;
using aSql.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DataGrid = Avalonia.Controls.DataGrid;
using DataGridTextColumn = Avalonia.Controls.DataGridTextColumn;

namespace aSql.Views;

public partial class ResultsGridView : UserControl
{
  private const double GridFontZoomStep = 0.5;
  private const double DefaultGridFontSize = 13.0;
  private const double MinGridFontSize = 6.0;

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
        if (vm.ColumnNames != null) RebuildColumns(grid, vm.ColumnNames);
      };

      if (vm.ColumnNames is { Count: > 0 }) RebuildColumns(grid, vm.ColumnNames);
    });
  }

  private static void RebuildColumns(DataGrid grid, IReadOnlyList<string> columnNames)
  {
    grid.Columns.Clear();

    foreach (var name in columnNames)
    {
      // TODO: Später nochmal prüfen... 
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
      grid.Columns.Add(new DataGridTextColumn
      {
        Header = name,
        Binding = new Binding($"[{name}]") { Converter = new NullToNullStringConverter() }
      });
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
    }
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
}