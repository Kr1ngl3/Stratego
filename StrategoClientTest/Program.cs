
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.Mail;

var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13);

{ 
    using TcpClient client = new();
    await client.ConnectAsync(ipEndPoint);
    await using NetworkStream stream = client.GetStream();
    var buffer = new byte[1_024];
    int received = await stream.ReadAsync(buffer);

    var message = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine($"Message received: \"{message}\"");

    byte[] buffer2 = new byte[10];
    received = await stream.ReadAsync(buffer2);

    foreach (byte b in buffer2)
        Console.Write(b);

    //await stream.WriteAsync(new byte[] {25, 13 }, 0, 2);
    Console.Read();
}

