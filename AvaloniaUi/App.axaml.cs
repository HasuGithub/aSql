using System.Reactive;
using aSql.ViewModels;
using aSql.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;
using ConnectWindow = aSql.Views.ConnectWindow;
using MainWindow = aSql.Views.MainWindow;

namespace aSql;

public class App : Application
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ShowFinalExceptionDialog);

    Dispatcher.UIThread.UnhandledException += (_, e) =>
    {
      ShowFinalExceptionDialog(e.Exception);
      e.Handled = true;
    };

    Dispatcher.UIThread.UnhandledExceptionFilter += (_, e) =>
    {
      // Prevent certain exceptions from reaching UnhandledException
      if (e.Exception is TaskCanceledException) e.RequestCatch = false;
    };

    TaskScheduler.UnobservedTaskException += (_, e) =>
    {
      // Prevent the exception from terminating the process
      e.SetObserved();
    };

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      try
      {
        var mainVm = new MainWindowViewModel();
        var mainWindow = new MainWindow(mainVm);
        desktop.MainWindow = mainWindow;
        mainVm.IsLoading = true;
        mainWindow.Show();

        var connectVm = new ConnectViewModel();
        var connectWindow = new ConnectWindow(connectVm);

        connectWindow.Unloaded += (_, _) =>
        {
          if (connectVm.Connection != null)
            Dispatcher.UIThread.Post(() =>
            {
              mainVm.DbConnect = connectVm.Connection;
              mainVm.UseTopEnabled = true;
              mainVm.UseUpperCaseSql = true;
              mainVm.UseSimpleDate = true;
            });
          else
            desktop.Shutdown();
        };

        connectWindow.Show(desktop.MainWindow);
      }
      catch (Exception ex)
      {
        ShowFinalExceptionDialog(ex);
        return;
      }

    base.OnFrameworkInitializationCompleted();
  }

  private void ShowFinalExceptionDialog(Exception ex)
  {
    Dispatcher.UIThread.Post(async void () =>
    {
      try
      {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
          var errorWindow = new ErrorWindow(ex.Message, ex + ex.StackTrace);
          if (desktop.MainWindow != null) await errorWindow.ShowDialog(desktop.MainWindow);
          desktop.Shutdown(1);
          return;
        }

        Environment.Exit(1);
      }
      catch (Exception)
      {
        // Ignored 
      }
    });
  }
}