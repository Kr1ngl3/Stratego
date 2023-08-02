using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;

namespace StrategoServer
{
    class Simple
    {

    }

    class Start
    {

        private static Queue _queue0 = new Queue();
        private static Queue _queue1 = new Queue();

        static void Main(string[] args)
        {
            Go().Wait();
        }

        static async Task Go()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 13);
            TcpListener listener = new TcpListener(ipEndPoint);
            TcpClient client;
            listener.Start();
            Console.WriteLine("Listening");

            for (int i = 0; i < 2; i++)
            {
                client = await listener.AcceptTcpClientAsync();
                if (i == 0)
                    ThreadPool.QueueUserWorkItem(Player0Handler, client);
                else
                    ThreadPool.QueueUserWorkItem(Player1Handler, client);
                Console.WriteLine($"Player {i} connected");
            }

            while (true)
            {
                int temp = int.Parse(Console.ReadLine());
                if (temp == 0)
                    _queue0.Enqueue(1);
                else
                    _queue1.Enqueue(1);
            }
        }

        private static async void Player0Handler(object client)
        {
            using (NetworkStream stream = (client as TcpClient).GetStream())
            {
                while (true)
                {
                    if (_queue0.Count == 0)
                        continue;
                    switch ((int)_queue0.Dequeue())
                    {
                        case 1:
                            await SendColor(stream);
                            break;
                        case 2:
                            await SendMessage(stream, "from other client");
                            break;
                    }
                    await RecieveMessage(stream);
                }
            }
        }

        private static async void Player1Handler(object client)
        {
            using (NetworkStream stream = (client as TcpClient).GetStream())
            {
                while (true)
                {
                    if (_queue1.Count == 0)
                        continue;
                    switch ((int)_queue1.Dequeue())
                    {
                        case 1:
                            await SendMessage(stream, "from server");
                            break;
                        case 2:
                            await SendMessage(stream, "from other client");
                            break;
                    }
                }
            }
        }

        static async Task SendColor(NetworkStream stream)
        {
            Random random = new Random();
            byte[] message = new byte[] { 1, (byte)random.Next(2)};
            await SendMessage(stream, message);
        }

        static async Task SendMessage(NetworkStream stream, string message)
        {
            byte[] dateTimeBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(dateTimeBytes, 0, dateTimeBytes.Length);
        }

        static async Task SendMessage(NetworkStream stream, byte[] message)
        {
            await stream.WriteAsync(message, 0, message.Length);
        }



        static async Task RecieveMessage(NetworkStream stream)
        {
            var buffer = new byte[1_024];
            int received = await stream.ReadAsync(buffer, 0, 1_024);

            var message = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Message received: \"{message}\"");
        }
    }
}
