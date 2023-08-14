using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Stratego.ViewModels;
using Stratego.Views;
using System;

namespace Stratego
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // creates new window
                desktop.MainWindow = new MainWindow();
                // sets datacontext of window to a new MainWindowViewModel, which needs a action that closes the window which we get from GetCloseAction on the window
                desktop.MainWindow.DataContext = new MainWindowViewModel((desktop.MainWindow as MainWindow)!.GetCloseAction());
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}