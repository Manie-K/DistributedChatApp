using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static TcpListener listener;
    static List<TcpClient> clients = new List<TcpClient>();

    static void Main()
    {
        int port = 5000;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] listening on port {port}...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(client);
            Console.WriteLine("[Server] new client connected");
            Thread t = new Thread(HandleClient);
            t.Start(client);
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);
                if (bytes == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytes);
                Console.WriteLine($"[Server] recived message: {message}");

                //Sending message to all connected clients
                foreach (var c in clients)
                {
                    if (c.Connected)
                    {
                        NetworkStream s = c.GetStream();
                        byte[] msgBuffer = Encoding.UTF8.GetBytes(message);
                        s.Write(msgBuffer, 0, msgBuffer.Length);
                    }
                }
            }
        }
        catch { }
        finally
        {
            Console.WriteLine("[Server] client disconnected");
            client.Close();
            clients.Remove(client);
        }
    }
}
