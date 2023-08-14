using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata;
using MessageBox.Avalonia;
using Avalonia.Threading;

namespace Stratego.Models
{
    class Client
    {
        #region private readonly fields
        // field to save the close window action
        private readonly Action _closeWindow;
        #endregion

        #region private fields
        // IPEndPoint used to connect to the server at the given port
        private IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13);

        //falgs that indicate if the client should send or recieve messages
        private bool _recieveMessage = false;
        private bool _sendMessage = false;

        // buffers for reading and writing to and from the server
        private byte[] _readBuffer = new byte[1_024];
        private byte[] _writeBuffer = new byte[1_024];

        // integers to store length of data in buffer
        private int _readBufferLength;
        private int _writeBufferLength;
        #endregion

        #region public methods
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="closeWindow"> the close action that is used when connection is lost to the server </param>
        public Client(Action closeWindow)
        {
            _closeWindow = closeWindow;
        }

        /// <summary>
        /// function that creates a tcp client and connects it to the server
        /// also starts a new thread for the handler, that handles communication with the server
        /// </summary>
        public void ConnectToServer()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.ConnectAsync(_ipEndPoint).Wait();
            ThreadPool.QueueUserWorkItem(HandleClient, tcpClient);
        }

        /// <summary>
        /// Tells the handler to recieve player number from the server
        /// </summary>
        /// <returns> the player number </returns>
        public async Task<int> GetPlayer()
        {
            await WaitForMessage();
            _readBufferLength = 0;
            return _readBuffer[0];
        }

        /// <summary>
        /// Tells the handler to recieve color from the server
        /// </summary>
        /// <returns> the color </returns>
        public async Task<Piece.Color> GetColor()
        {
            await WaitForMessage();
            _readBufferLength = 0;
            return (Piece.Color)_readBuffer[0];
        }

        /// <summary>
        /// Tells the handler to recieve field from the server
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetField()
        {
            await WaitForMessage();
            byte[] temp = new byte[_readBufferLength];

            // copies data from buffer
            for (int i = 0; i < _readBufferLength; i++)
                temp[i] = _readBuffer[i];

            _readBufferLength = 0;
            return temp;
        }
        
        /// <summary>
        /// Tells the handler to recieve move from server
        /// </summary>
        /// <returns> returns the move as byte[] </returns>
        public async Task<byte[]> GetMove()
        {
            await WaitForMessage();
            byte[] temp = new byte[_readBufferLength];

            for (int i = 0; i < _readBufferLength; i++)
                temp[i] = _readBuffer[i];

            _readBufferLength = 0;
            return temp;
        }
        
        /// <summary>
        /// Tells the handler to send the chosen color to the server
        /// </summary>
        /// <param name="c"></param>
        public void SendColor(Piece.Color c)
        {
            _writeBuffer[0] = (byte)c;
            _writeBufferLength = 1;
            _sendMessage = true;
        }

        /// <summary>
        /// Tells the handler to send players field to the server
        /// </summary>
        /// <param name="field"></param>
        public void SendField(byte[] field)
        {
            // copies the field data into the buffer
            for (int i = 0; i < field.Length; i++ )
                _writeBuffer[i] = field[i];

            _writeBufferLength = field.Length;
            _sendMessage = true;
        }

        /// <summary>
        /// Tells the handler to send players move to server
        /// </summary>
        /// <param name="move"></param>
        public void SendMove(byte[] move)
        {
            for (int i = 0; i < move.Length; i++)
                _writeBuffer[i] = move[i];
            _writeBufferLength = move.Length;
            _sendMessage = true;
        }
        #endregion

        #region private methods
        /// <summary>
        /// The handler for communicating between client and server, runs on another thread after connection has been established
        /// Uses flags set from other thread to determin when and what to do with the server
        /// When connection is lost to the server a message box will appear, and when clicked will close the window, and the application
        /// </summary>
        /// <param name="client"> The tcp client created when connection was established </param>
        private async void HandleClient(object? client)
        {
            // surrounded by try catch, to catch the exception thrown when connection is lost
            try
            {
                using NetworkStream stream = (client as TcpClient)!.GetStream();

                // loop for interacting with server
                while ((client as TcpClient)!.Connected)
                {
                    // when client is supposed to recieve message
                    if (_recieveMessage)
                    {
                        await GetMessage(stream);
                        _recieveMessage = false;
                    }
                    // when client is supposed to send message
                    else if (_sendMessage)
                    {
                        await SendMessage(stream);
                        _sendMessage = false;
                    }
                }
            }
            catch (Exception)
            {
                // creates the messagebox
                Dispatcher.UIThread.Post(ShowMessageBox);
            }
        }

        /// <summary>
        /// Creates the messagebox and closes the window when the messagebox is clicked on
        /// </summary>
        private async void ShowMessageBox()
        {
            var box = MessageBoxManager.GetMessageBoxStandardWindow("Unlucky", "Connection to server was lost", MessageBox.Avalonia.Enums.ButtonEnum.OkAbort);
            await box.Show();
            _closeWindow.Invoke();
        }

        /// <summary>
        /// semi blocking loop waiting for readbuffer to be filled, as in a message has been recieved from the server
        /// also tells the handler that a message should be recieved with _recieveMessage flag
        /// </summary>
        private async Task WaitForMessage()
        {
            _recieveMessage = true;
            while (_readBufferLength == 0)
                await Task.Delay(100);
        }

        /// <summary>
        /// Gets message from server and saves it in the read buffer, also sets the length
        /// </summary>
        /// <param name="stream"> stream from which to get the message </param>
        private async Task GetMessage(NetworkStream stream)
        {
            _readBuffer = new byte[1_024];
            _readBufferLength = await stream.ReadAsync(_readBuffer);
        }

        /// <summary>
        /// sends message to server and resets buffer after sending
        /// </summary>
        /// <param name="stream"> the stream to write to </param>
        /// <returns></returns>
        private async Task SendMessage(NetworkStream stream)
        {
            await stream.WriteAsync(_writeBuffer, 0, _writeBufferLength);
            _writeBuffer = new byte[1_024];
            _writeBufferLength = 0;
        }
        #endregion
    }
}
