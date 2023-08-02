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
    class Start
    {
        enum HandlerType
        {
            SendPlayerNumber,
            WaitForColor,
            SendOtherColor,
            WaitForField,
            SendField,
            WaitForMove,
            SendMove
        }


        private static Queue[] _queues = new Queue[] { new Queue(), new Queue()};
        private static byte[][] _fields = new byte[][] { new byte[50], new byte[50] };
        private static int _firstColor = -1;
        const int FIELD_SIZE = 50;

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

            for (byte i = 0; i < 2; i++)
            {
                client = await listener.AcceptTcpClientAsync();
                ThreadPool.QueueUserWorkItem(PlayerHandler, new Tuple<TcpClient, byte>(client, i));
                Console.WriteLine($"Player {i} connected");
            }
            for (int i = 0; i < 2; i++)
                _queues[i].Enqueue(HandlerType.SendPlayerNumber);

            Console.ReadLine();
        }

        private static async void PlayerHandler(object clientAndPlayer)
        {
            TcpClient client = (clientAndPlayer as Tuple<TcpClient, byte>).Item1;
            byte playerNumber = (clientAndPlayer as Tuple<TcpClient, byte>).Item2;

            using (NetworkStream stream = client.GetStream())
            {
                while (true)
                {
                    if (_queues[playerNumber].Count == 0)
                        continue;
                    switch ((HandlerType)_queues[playerNumber].Dequeue())
                    {
                        case HandlerType.SendPlayerNumber:
                            await SendMessage(stream, new byte[] { playerNumber });
                            Console.WriteLine($"SendPlayerNumber to {playerNumber}");
                            if (playerNumber == 1)
                                _queues[0].Enqueue(HandlerType.WaitForColor);
                            break;
                        case HandlerType.WaitForColor:
                            Console.WriteLine($"Waiting for {playerNumber}'s color");
                            byte[] temp = await RecieveMessage(stream);
                            _firstColor = temp[0];
                            _queues[1].Enqueue(HandlerType.SendOtherColor);
                            break;
                        case HandlerType.SendOtherColor:
                            await SendMessage(stream, new byte[] { (byte)OtherNumber(_firstColor) });
                            Console.WriteLine($"SendOtherColor to {playerNumber}");

                            for (int i = 0; i < 2; i++)
                                _queues[i].Enqueue(HandlerType.WaitForField);
                            break;
                        case HandlerType.WaitForField:
                            Console.WriteLine($"waiting for field from {playerNumber}");
                            _fields[OtherNumber(playerNumber)] = await RecieveMessage(stream);
                            _queues[OtherNumber(playerNumber)].Enqueue(HandlerType.SendField);
                            break;
                        case HandlerType.SendField:
                            await SendMessage(stream, _fields[playerNumber]);
                            Console.WriteLine($"Send Field to {playerNumber}");

                            if (playerNumber == 0)
                                _queues[0].Enqueue(HandlerType.WaitForMove);
                            break;
                        default:
                            break;
                    }
                }
            }
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

        static int OtherNumber(int n)
        {
            return 1 & ~_firstColor;
        }

        static async Task<byte[]> RecieveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[1_024];
            int received = await stream.ReadAsync(buffer, 0, 1_024);

            byte[] temp = new byte[received];
            for (int i = 0; i < received; i++)
                temp[i] = buffer[i];
            return temp;
        }
    }
}
