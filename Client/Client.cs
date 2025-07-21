using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static void Main()
    {
        string serverIp = "127.0.0.1";
        int port = 5000;

        try
        {
            TcpClient client = new TcpClient(serverIp, port);
            NetworkStream stream = client.GetStream();

            // Sending client name to server
            Random random = new Random();
            int clientId = random.Next(1, 100);
            string clientName = "Klient" + clientId;
            byte[] nameBytes = Encoding.UTF8.GetBytes(clientName);
            
            byte[] lengthBytes = BitConverter.GetBytes(nameBytes.Length);
            stream.Write(lengthBytes, 0, lengthBytes.Length);

            stream.Write(nameBytes, 0, nameBytes.Length);

            Console.WriteLine("[Client] connected with server");

            //Thread receiving messages
            Thread receiveThread = new Thread(() =>
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    try
                    {
                        int bytes = stream.Read(buffer, 0, buffer.Length);
                        if (bytes == 0) break;

                        string message = Encoding.UTF8.GetString(buffer, 0, bytes);
                        Console.WriteLine(message);
                    }
                    catch { break; }
                }
            });
            receiveThread.Start();

            //Sending messages
            while (true)
            {
                Console.Write("Write message (or type /exit to end): ");
                string message = Console.ReadLine();
                if (message == "/exit") break;

                byte[] msgBuffer = Encoding.UTF8.GetBytes(message);
                stream.Write(msgBuffer, 0, msgBuffer.Length);
            }

            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Client] error: {e.Message}");
        }
    }
}
