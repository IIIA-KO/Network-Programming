using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    internal class Program
    {
        static List<ClientInfo> clients = new List<ClientInfo>();

        static void Main(string[] args)
        {
            Console.WriteLine("Server started!");

            var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            var localEndPoint = new IPEndPoint(ip, 5000);
            var listener = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(1000);

                while (true)
                {
                    Console.WriteLine("Waiting for connection");
                    Socket handle = listener.Accept();
                    var clientInfo = new ClientInfo(handle, "");
                    clients.Add(clientInfo);

                    Console.WriteLine("Connected");

                    Task.Run(() =>
                    {
                        HandleClient(clientInfo);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        static void HandleClient(ClientInfo clientInfo)
        {
            try
            {
                var bytes = new byte[1024];
                int bytesRec = clientInfo.Socket.Receive(bytes);
                string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                string[] nameMessageParts = data.Split(new[] { ' ' }, 2);
                if (nameMessageParts.Length != 2 || !nameMessageParts[0].StartsWith("@Server"))
                {
                    Console.WriteLine($"Invalid format. Disconnecting {clientInfo.Socket.RemoteEndPoint}");
                    return;
                }

                clientInfo.Name = nameMessageParts[1].Trim();
                Console.WriteLine($"Client {clientInfo.Socket.RemoteEndPoint} connected with name: {clientInfo.Name}");

                while (true)
                {
                    bytes = new byte[1024];
                    bytesRec = clientInfo.Socket.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (data.IndexOf("\r\n") > -1)
                    {
                        Console.WriteLine($"Message received from {clientInfo.Name}: {data}");

                        if (data.StartsWith("@"))
                        {
                            int spaceIndex = data.IndexOf(' ');
                            if (spaceIndex != -1)
                            {
                                string recipientName = data.Substring(1, spaceIndex - 1);
                                string message = data.Substring(spaceIndex + 1).Trim();
                                SendPrivateMessage(recipientName, message, clientInfo);
                            }
                            else
                            {
                                var errorMsg = Encoding.ASCII.GetBytes($"Invalid private message format. Use '@recipient message'\r\n");
                                clientInfo.Socket.Send(errorMsg);
                            }
                        }
                        else
                        {
                            BroadcastMessage(data, clientInfo);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Client {clientInfo.Socket.RemoteEndPoint} disconnected");
                Console.ResetColor();
                clients.Remove(clientInfo);
            }
        }

        static void BroadcastMessage(string message, ClientInfo sender)
        {
            foreach (var client in clients)
            {
                if (client != sender)
                {
                    var msg = Encoding.ASCII.GetBytes(message);
                    client.Socket.Send(msg);
                }
            }
        }

        static void SendPrivateMessage(string recipientName, string message, ClientInfo sender)
        {
            var recipient = clients.Find(c => c.Name == recipientName);
            if (recipient != null)
            {
                var msg = Encoding.ASCII.GetBytes($"(Private) {sender.Name}: {message}\r\n");
                recipient.Socket.Send(msg);
            }
            else
            {
                var msg = Encoding.ASCII.GetBytes($"User '{recipientName}' not found\r\n");
                sender.Socket.Send(msg);
            }
        }
    }

    internal class ClientInfo
    {
        public Socket Socket { get; }
        public string Name { get; set; }

        public ClientInfo(Socket socket, string name)
        {
            Socket = socket;
            Name = name;
        }
    }
}