
using System.Net.Sockets;
using System.Net;
using System.Text;


    var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13);

    using TcpClient client = new();
    await client.ConnectAsync(ipEndPoint);
    await using NetworkStream stream = client.GetStream();
    var buffer = new byte[1_024];
    int received = await stream.ReadAsync(buffer);

    var message = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine($"Message received: \"{message}\"");
// Sample output:
//     Message received: "📅 8/22/2022 9:07:17 AM 🕛"
Console.Read();
