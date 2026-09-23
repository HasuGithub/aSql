using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace aSql.Views;

public partial class StatusMessageView : UserControl
{
  private const double FontZoomStep = 0.5;
  private const double MinFontSize = 6.0;
  private ItemsControl? _statusMessageItems;

  public StatusMessageView()
  {
    InitializeComponent();
    InitializeTextZoom();
  }

  private void InitializeTextZoom()
  {
    _statusMessageItems = this.FindControl<ItemsControl>("StatusMessageItems");
    var scrollViewer = this.FindControl<ScrollViewer>("StatusScrollViewer");
    scrollViewer?.AddHandler(
      PointerWheelChangedEvent,
      OnTextPointerWheelChanged,
      RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
      true);
  }

  private void OnTextPointerWheelChanged(object? sender, PointerWheelEventArgs e)
  {
    if (sender is not ScrollViewer || e.KeyModifiers.HasFlag(KeyModifiers.Control) is false) return;

    switch (e.Delta.Y)
    {
      case > 0:
        _statusMessageItems?.FontSize += FontZoomStep;
        e.Handled = true;
        break;
      case < 0:
        _statusMessageItems?.FontSize = Math.Max(MinFontSize, _statusMessageItems.FontSize - FontZoomStep);
        e.Handled = true;
        break;
    }
  }
}