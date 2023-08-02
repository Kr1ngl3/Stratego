using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

namespace Stratego.Models
{
    class Client
    {
        enum MessageType
        {
            ColorChoose,
            GetColor,
            EnemyField,
            OponentMove
        }

        private IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13);
        private bool _recieveMessage = false;
        private byte[] _buffer = new byte[1_024];
        private int _bufferLength = 0;

        public Client()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.ConnectAsync(_ipEndPoint).Wait();
            ThreadPool.QueueUserWorkItem(HandleClient, tcpClient);
        }

        public async Task<Piece.Color> GetColor()
        {
            await WaitForMessage();

            if ((MessageType)_buffer[0] != MessageType.GetColor && _bufferLength != 2)
                throw new Exception("wrong message recieved");

            _bufferLength = 0;
            return (Piece.Color)_buffer[1];
        }


        private async void HandleClient(object? client)
        {
            using NetworkStream stream = (client as TcpClient)!.GetStream();
            while ((client as TcpClient)!.Connected)
            {
                if (!_recieveMessage)
                    continue;

                await GetMessage(stream);

                _recieveMessage = false;
            }
        }

        private async Task WaitForMessage()
        {
            _recieveMessage = true;
            while (_bufferLength == 0)
                continue;
        }
        private async Task GetMessage(NetworkStream stream)
        {
            _buffer = new byte[1_024];
            _bufferLength = await stream.ReadAsync(_buffer);
        }

    }
}
