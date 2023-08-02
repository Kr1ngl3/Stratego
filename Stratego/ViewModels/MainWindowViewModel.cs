using ReactiveUI;

namespace Stratego.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel { get => _currentViewModel; set => this.RaiseAndSetIfChanged(ref _currentViewModel, value, nameof(CurrentViewModel)); }

        public MainWindowViewModel()
        {
            _currentViewModel = new ConnectViewModel(this);
        }
    }
}