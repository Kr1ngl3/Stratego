using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace StrategoServer
{
    class Simple
    {

    }

    class Start
    {
        static void Main(string[] args)
        {
            Go().Wait();
        }

        static async Task Go()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 13);
            TcpListener listener = new TcpListener(ipEndPoint);

            TcpClient player1, player2;

            try
            {
                Console.WriteLine("Listening");
                listener.Start();
                player1 = await listener.AcceptTcpClientAsync();
                await SendMessage(player1);
                player2 = await listener.AcceptTcpClientAsync();
                await SendMessage(player2);
            }
            finally
            {
                listener.Stop();
            }
        }

        static async Task SendMessage(TcpClient handler)
        {
            using (NetworkStream stream = handler.GetStream())
            {
                string message = $"📅 {DateTime.Now} 🕛";
                byte[] dateTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dateTimeBytes, 0, dateTimeBytes.Length);
            }
        }

    }
}
