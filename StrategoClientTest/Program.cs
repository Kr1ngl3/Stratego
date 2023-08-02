
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.Mail;

var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13);

{ 
    using TcpClient client = new();
    await client.ConnectAsync(ipEndPoint);
    await using NetworkStream stream = client.GetStream();


    while (true)
    {
        var buffer = new byte[1_024];
        int received = await stream.ReadAsync(buffer);

        var message = Encoding.UTF8.GetString(buffer, 0, received);
        Console.WriteLine($"Message received: \"{message}\"");

        byte[] dateTimeBytes = Encoding.UTF8.GetBytes("Hello this is a response");
        await stream.WriteAsync(dateTimeBytes, 0, dateTimeBytes.Length);
        Console.WriteLine("Message sent");
    }
}

