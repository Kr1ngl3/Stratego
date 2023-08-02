using ReactiveUI;
using Stratego.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stratego.ViewModels
{
    class ConnectViewModel : ViewModelBase
    {

        private readonly MainWindowViewModel _mainWindowViewModel;
        private Client _client;
        private int _player = -1;

        public bool ShowOptions => _player == 0;

        public ConnectViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _client = new Client();

            ThreadPool.QueueUserWorkItem(GetPlayer, null);
        }

        public void SendColor(int color)
        {
            _client.SendColor((Piece.Color)color);
            _mainWindowViewModel.CurrentViewModel = new GameViewModel((Piece.Color)color, _client);
        }

        private async void GetPlayer(object? obj)
        {
            _player = await _client.GetPlayer();
            this.RaisePropertyChanged(nameof(ShowOptions));
            if (_player == 1)
                await GetColor();
        }

        private async Task GetColor()
        {
            Piece.Color color = await _client.GetColor();
            _mainWindowViewModel.CurrentViewModel = new GameViewModel(color, _client);
        }
    }
}