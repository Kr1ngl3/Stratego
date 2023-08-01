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
        private TcpClient _client = new TcpClient();

        public Client()
        {
            _client.ConnectAsync(_ipEndPoint).Wait();
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
            await using NetworkStream stream = _client.GetStream();
            byte[] buffer = new byte[1024];
            int received = await stream.ReadAsync(buffer);

            return buffer;
        }

    }
}
