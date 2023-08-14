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
        #region private readonly fields
        // main window view model, used to change view when connection is complete
        private readonly MainWindowViewModel _mainWindowViewModel;
        
        // client to connect and get/send color from/to other player
        private readonly Client _client;
        #endregion

        #region private backing fields
        // indicates what player number this player is, -1 = uninitialized, 0 = player who chooses color and 1 = other player
        private int _player = -1;

        // status message to display on screen, starts out as Connecting to server
        private string _status = "Connecting to server";

        // starts as false but is set to true, when client.Initialize throws a error
        private bool _failedToConnect = false;
        #endregion

        #region public properties
        // determines if options should be shown to player, when player number is 0
        public bool ShowOptions => _player == 0;

        // determines if client connection failed
        public bool FailedToConnect { get => _failedToConnect; private set => this.RaiseAndSetIfChanged(ref _failedToConnect, value, nameof(FailedToConnect)); }
        
        // gets the status message
        public string Status { get => _status; private set => this.RaiseAndSetIfChanged(ref _status, value, nameof(Status)); }
        #endregion

        #region public methods
        /// <summary>
        /// Constructor creates a new client with close window action
        /// and start new thread for GetPlayer
        /// </summary>
        /// <param name="mainWindowViewModel"> Reference to main window view model </param>
        /// <param name="closeWindow"> Action to close window </param>
        public ConnectViewModel(MainWindowViewModel mainWindowViewModel, Action closeWindow)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _client = new Client(closeWindow);

            ThreadPool.QueueUserWorkItem(_ => GetPlayer());
        }
        
        /// <summary>
        /// button to try connecting again if connection failed using get player method
        /// </summary>
        public void TryAgain()
        {
            Status = "Connecting to server";
            FailedToConnect = false;
            ThreadPool.QueueUserWorkItem(_ => GetPlayer());
        }

        /// <summary>
        /// Called from color buttons, and uses client to send color to other player and changes view model to game
        /// </summary>
        /// <param name="color"> the chosed color </param>
        public void SendColor(int color)
        {
            _client.SendColor((Piece.Color)color);
            _mainWindowViewModel.CurrentViewModel = new GameViewModel((Piece.Color)color, _client, _player);
        }
        #endregion

        #region private methods
        /// <summary>
        /// First tries to connect to server, and if it does not fail it will then try and get a player number from the server
        /// if it fails it throws an error and updates status and flag accordingly
        /// </summary>
        private async void GetPlayer()
        {
            try
            {
                _client.ConnectToServer();
            }
            catch (Exception)
        {
                // if it fails to connect to the server
                Status = "Failed to connect to server ";
                FailedToConnect = true;
                this.RaisePropertyChanged(nameof(FailedToConnect));
                return;
            }
            // if it succeeds
            Status = "Waiting for other Player";
            _player = await _client.GetPlayer();
            this.RaisePropertyChanged(nameof(ShowOptions));
            
            // if the player number is 1, options won't be shown, and will instead await GetColor fuction
            if (_player == 1)
                await GetColor();
            // updates status for player 0 who is choosing color
            else
                Status = "Choose color";
        }

        /// <summary>
        /// Gets color from the client and uses it to create the game view model
        /// </summary>
        private async Task GetColor()
        {
            Piece.Color color = await _client.GetColor();
            _mainWindowViewModel.CurrentViewModel = new GameViewModel(color, _client, _player);
        }
        #endregion
    }
}