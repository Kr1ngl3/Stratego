using MessageBox.Avalonia;
using ReactiveUI;
using System;

namespace Stratego.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // property and backingfield for the current view model, changes what is shown based on view model and the viewlocator
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel { get => _currentViewModel; set => this.RaiseAndSetIfChanged(ref _currentViewModel, value, nameof(CurrentViewModel)); }

        public MainWindowViewModel(Action closeWindow)
        {
            _currentViewModel = new ConnectViewModel(this, closeWindow);
        }
    }
}