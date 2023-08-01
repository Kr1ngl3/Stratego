using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        private NetworkStream _stream;

        public Client()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.ConnectAsync(_ipEndPoint).Wait();
            _stream = tcpClient.GetStream();
        }



        public async Task<Piece.Color> GetColor()
        {
            byte[] data;
            do
                data = await GetMessage();
            while ((MessageType)data[0] != MessageType.GetColor && data.Length != 2);

            return (Piece.Color)data[1];
        }

        private async Task<byte[]> GetMessage()
        {
            byte[] buffer = new byte[1024];
            int received = await _stream.ReadAsync(buffer);

            return buffer;
        }

    }
}
