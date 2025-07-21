using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ConnectedClient
{
    public TcpClient TCPClient { get; set; }
    public string Name { get; set; }
}

class Server
{
    static TcpListener listener;
    static List<ConnectedClient> clients = new List<ConnectedClient>();

    static void Main()
    {
        int port = 5000;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] listening on port {port}...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();

            NetworkStream stream = client.GetStream();

            // Reading client name length
            byte[] lengthBytes = new byte[4];
            stream.Read(lengthBytes, 0, 4);
            int nameLength = BitConverter.ToInt32(lengthBytes, 0);

            // Reading client name
            byte[] nameBytes = new byte[nameLength];
            stream.Read(nameBytes, 0, nameLength);
            string clientName = Encoding.UTF8.GetString(nameBytes);

            ConnectedClient connectedClient = new ConnectedClient
            {
                TCPClient = client,
                Name = clientName
            };
            clients.Add(connectedClient);

            Console.WriteLine("[Server] new client connected");
            Thread t = new Thread(HandleClient);
            t.Start(connectedClient);
        }
    }

    static void HandleClient(object obj)
    {
        ConnectedClient connectedClient = (ConnectedClient)obj;
        TcpClient clientTCP = connectedClient.TCPClient;
        NetworkStream stream = clientTCP.GetStream();
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
                    if (c.TCPClient.Connected && c.TCPClient != clientTCP)
                    {
                        NetworkStream s = c.TCPClient.GetStream();
                        byte[] msgBuffer = Encoding.UTF8.GetBytes(connectedClient.Name + ": " + message);
                        s.Write(msgBuffer, 0, msgBuffer.Length);
                    }
                }
            }
        }
        catch { }
        finally
        {
            Console.WriteLine("[Server] client disconnected");
            clientTCP.Close();
            clients.Remove(connectedClient);
        }
    }
}
