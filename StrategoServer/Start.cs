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
        // enum for handler types
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

        #region fields for cross communication
        // queues for each player, to tell each handler thread what they should do based on handler type
        private static Queue[] _queues = new Queue[] { new Queue(), new Queue()};

        //buffer to store field and move for each player
        private static byte[][] _crossBuffers = new byte[2][];

        // int to save what color player one chose
        private static int _firstColor = -1;

        // flag that tells main thread that connection to a client was lost, and that the program can now end
        private static bool _connectionLost = false;
        #endregion

        #region methods for main thread
        /// <summary>
        /// entry point starts async go function and waits on it
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Go().Wait();
        }

        /// <summary>
        /// async function that establishes connection with the two players and enqueues the first handler for each player
        /// then waits on connection lost flag to close the program
        /// </summary>
        static async Task Go()
        {
            // creates and starts server/listener
            TcpListener listener = new TcpListener(IPAddress.Any, 13);
            listener.Start();

            Console.WriteLine("Listening");

            // loops over each player
            for (byte i = 0; i < 2; i++)
            {
                // waits for client
                TcpClient client = await listener.AcceptTcpClientAsync();

                // creates new thread for handling each player
                ThreadPool.QueueUserWorkItem(PlayerHandler, new Tuple<TcpClient, byte>(client, i));

                Console.WriteLine($"Player {i} connected");
            }

            // enqueues send player number handlertype for each player
            for (int i = 0; i < 2; i++)
                _queues[i].Enqueue(HandlerType.SendPlayerNumber);

            // waites for connection lost flag, while everthing else runs on the 2 new playerhandler threads
            while (!_connectionLost)
                continue;

            // closes the program, if connection lost flag is set
            Console.ReadKey();
        }
        #endregion

        #region methods for playerhandler threads
        /// <summary>
        /// handles messages to and from client using queue of what player number it recieves
        /// </summary>
        /// <param name="clientAndPlayer"> contains the tcpclient and number of player </param>
        static async void PlayerHandler(object clientAndPlayer)
        {
            // gets variables from the tuple
            TcpClient client = (clientAndPlayer as Tuple<TcpClient, byte>).Item1;
            byte playerNumber = (clientAndPlayer as Tuple<TcpClient, byte>).Item2;

            using (NetworkStream stream = client.GetStream())
            {
                // try catch to catch when connection to client is lost
                try
                {
                    while (true)
                    {
                        // does nothing if corrosponding queue is empty
                        if (_queues[playerNumber].Count == 0)
                            continue;
                        //switches on the handler type in the queue
                        switch ((HandlerType)_queues[playerNumber].Dequeue())
                        {
                            // sends player number to clients, used by both threads
                            case HandlerType.SendPlayerNumber:
                                await SendMessage(stream, new byte[] { playerNumber });
                                Console.WriteLine($"SendPlayerNumber to {playerNumber}");
                                // when second player has gotten their number, will enqueue to recieve color from fist player 
                                if (playerNumber == 1)
                                    _queues[0].Enqueue(HandlerType.WaitForColor);
                                break;
                            // gets color from client, only called from first player thread
                            case HandlerType.WaitForColor:
                                byte[] temp = await RecieveMessage(stream);
                                Console.WriteLine($"Recieved {playerNumber}'s color");
                                // saves color in field, so that it can be accessed from other thread
                                _firstColor = temp[0];
                                // enqueues to send color to other player
                                _queues[1].Enqueue(HandlerType.SendOtherColor);
                                break;
                            // sends opposite color of first player to the other player, only called from second player
                            case HandlerType.SendOtherColor:
                                await SendMessage(stream, new byte[] { (byte)OtherNumber(_firstColor) });
                                Console.WriteLine($"SendOtherColor to {playerNumber}");

                                // now both players have gotten their color and can begin setting up their field, both thread will now wait for field
                                for (int i = 0; i < 2; i++)
                                    _queues[i].Enqueue(HandlerType.WaitForField);
                                break;
                            // recieves field from players and saves them in field to send later
                            case HandlerType.WaitForField:
                                Console.WriteLine($"Waiting for field from {playerNumber}");
                                // recieve and save field
                                _crossBuffers[OtherNumber(playerNumber)] = await RecieveMessage(stream);
                                Console.WriteLine($"Recieved field from {playerNumber}");
                                // enqueue that other player thread should send opponent field to client
                                _queues[OtherNumber(playerNumber)].Enqueue(HandlerType.SendField);
                                break;
                            // sends field to client, called by both threads
                            case HandlerType.SendField:
                                await SendMessage(stream, _crossBuffers[playerNumber]);
                                Console.WriteLine($"Send Field to {playerNumber}");

                                // enqueues that first player thread should wait for move from client
                                if (playerNumber == 0)
                                    _queues[0].Enqueue(HandlerType.WaitForMove);
                                break;
                            // waits for move from player and then makes them send it to the other player
                            case HandlerType.WaitForMove:
                                Console.WriteLine($"Waiting for move from {playerNumber}");
                                _crossBuffers[OtherNumber(playerNumber)] = await RecieveMessage(stream);
                                Console.WriteLine($"Recieved move from {playerNumber}");
                                _queues[OtherNumber(playerNumber)].Enqueue(HandlerType.SendMove);
                                break;
                            // sends opponent move to client and then waits for the clients own move
                            case HandlerType.SendMove:
                                await SendMessage(stream, _crossBuffers[playerNumber]);
                                Console.WriteLine($"Send move to {playerNumber}");

                                _queues[playerNumber].Enqueue(HandlerType.WaitForMove);
                                break;
                        }
                    }
                }
                catch (System.IO.IOException)
                {
                    // sets flag when exception is thrown
                    _connectionLost = true;
                    Console.WriteLine($"Connection lost to player {playerNumber} press any key to end program");
                    return;
                }
            }
        }

        /// <summary>
        /// sends message to client with its given stream
        /// </summary>
        /// <param name="stream"> stream of specific client </param>
        /// <param name="message"> message to send </param>
        /// <returns></returns>
        static async Task SendMessage(NetworkStream stream, byte[] message)
        {
             await stream.WriteAsync(message, 0, message.Length);
        }

        /// <summary>
        /// helper method to get other color or other player
        /// </summary>
        /// <param name="n"> number to get other number of </param>
        /// <returns> the other number </returns>
        static int OtherNumber(int n)
        {
            return 1 & ~n;
        }

        /// <summary>
        ///  revieves message from client given by stream
        /// </summary>
        /// <param name="stream"> stream of specific client </param>
        /// <returns> the message </returns>
        static async Task<byte[]> RecieveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[1_024];

            int received = await stream.ReadAsync(buffer, 0, 1_024);
            byte[] temp = new byte[received];
            // copies message out of buffer
            for (int i = 0; i < received; i++)
                temp[i] = buffer[i];
            return temp;
        }
        #endregion
    }
}
