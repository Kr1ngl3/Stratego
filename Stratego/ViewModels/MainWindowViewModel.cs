namespace Stratego.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ViewModelBase CurrentViewModel { get; set; } = new GameViewModel();
    }
}