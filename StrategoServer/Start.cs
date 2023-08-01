using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

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

            NetworkStream player1, player2;

            try
            {
                Console.WriteLine("Listening");
                listener.Start();
                var temp = await listener.AcceptTcpClientAsync();
                player1 = temp.GetStream();

                var client = temp.Client;
                
                await SendMessage(player1);
                Thread.Sleep(10_000);
                await SendMessage(player1, 1);


                var temp2 = await listener.AcceptTcpClientAsync();
                player2 = temp2.GetStream();
                await SendMessage(player2);
                await SendMessage(player2, 10);
            }
            finally
            {
                listener.Stop();
            }
        }

        static async Task SendMessage(NetworkStream stream)
        {
            string message = $"📅 {DateTime.Now} 🕛";
            byte[] dateTimeBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(dateTimeBytes, 0, dateTimeBytes.Length);
        }

        static async Task SendMessage(NetworkStream stream, byte i)
        {
            byte[] dateTimeBytes = new byte[] { i , 2, 3, 4, 5};
            await stream.WriteAsync(dateTimeBytes, 0, dateTimeBytes.Length);
        }


        static async Task RecieveMessage(TcpClient handler)
        {
            using (NetworkStream stream = handler.GetStream())
            {
                byte[] buffer = new byte[2];
                await stream.ReadAsync(buffer, 0, 2);
                Console.WriteLine($"you recieved {buffer[0]} and {buffer[1]}");
            }
        }

    }
}
