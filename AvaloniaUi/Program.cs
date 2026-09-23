using Avalonia;
using Avalonia.ReactiveUI;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace aSql;

internal static class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    try
    {
      BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    catch (Exception e)
    {
      MessageBoxManager.GetMessageBoxStandard("Exception", e.Message, ButtonEnum.Ok, Icon.Error)
        .ShowWindowDialogAsync(null!);
    }
  }

  public static AppBuilder BuildAvaloniaApp()
  {
    return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .LogToTrace()
      .UseReactiveUI()
      ;
  }
}