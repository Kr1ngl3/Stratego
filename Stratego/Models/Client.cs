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

namespace Stratego.Models
{
    class Client
    {

        private IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13);
        private bool _recieveMessage = false;
        private bool _sendMessage = false;
        private byte[] _buffer = new byte[1_024];
        private int _bufferLength;

        public Client()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.ConnectAsync(_ipEndPoint).Wait();
            ThreadPool.QueueUserWorkItem(HandleClient, tcpClient);
        }

        public async Task<Piece.Color> GetColor()
        {
            await WaitForMessage();

            if (_bufferLength != 1)
                Trace.WriteLine("wrong message recieved");

            _bufferLength = 0;
            return (Piece.Color)_buffer[0];
        }

        public async Task<int> GetPlayer()
        {
            await WaitForMessage();

            if (_bufferLength != 1)
                Trace.WriteLine("wrong message recieved");

            _bufferLength = 0;
            return _buffer[0];
        }

        public void SendColor(Piece.Color c)
        {
            _buffer[0] = (byte)c;
            _bufferLength = 1;
            _sendMessage = true;
        }

        public void SendField(byte[] field)
        {
            _buffer = field;
            _bufferLength = field.Length;
            _sendMessage = true;
        }

        public async Task<byte[]> GetField()
        {
            await WaitForMessage();

            if (_bufferLength != 50)
                Trace.WriteLine("wrong message recieved");
            byte[] temp = new byte[50];

            for (int i = 0; i < 50; i++)
                temp[i] = _buffer[i];

            _bufferLength = 0;
            return temp;
        }

        private async void HandleClient(object? client)
        {
            using NetworkStream stream = (client as TcpClient)!.GetStream();
            while ((client as TcpClient)!.Connected)
            {
                if (_recieveMessage)
                {
                    await GetMessage(stream);
                    _recieveMessage = false;
                }
                else if (_sendMessage)
                {
                    await SendMessage(stream);
                    _sendMessage = false;
                }

                    

            }
        }

        private async Task WaitForMessage()
        {
            _recieveMessage = true;
            while (_bufferLength == 0)
                await Task.Delay(100);
        }
        private async Task GetMessage(NetworkStream stream)
        {
            _buffer = new byte[1_024];
            _bufferLength = await stream.ReadAsync(_buffer);
        }
        private async Task SendMessage(NetworkStream stream)
        {
            await stream.WriteAsync(_buffer, 0, _bufferLength);
            _buffer = new byte[1_024];
            _bufferLength = 0;
        }
    }
}
