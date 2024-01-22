using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServer
{
    internal class TcpServer
    {
        internal class Program
        {
            static List<ClientInfo> clients = new List<ClientInfo>();

            static void Main(string[] args)
            {
                Console.WriteLine("TCP Server started!");

                var ip = IPAddress.Parse("127.0.0.1");
                var localEndPoint = new IPEndPoint(ip, 5000);
                var listener = new TcpListener(localEndPoint);

                try
                {
                    listener.Start();

                    while (true)
                    {
                        Console.WriteLine("Waiting for connection");
                        TcpClient client = listener.AcceptTcpClient();
                        var clientInfo = new ClientInfo(client, "");
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
                    var stream = clientInfo.Client.GetStream();
                    var reader = new StreamReader(stream, Encoding.ASCII);
                    var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                    var info = reader.ReadLine();
                    string[] nameMessageParts = info.Split(new[] { ' ' }, 2);
                    if (nameMessageParts.Length != 2 || !nameMessageParts[0].StartsWith("@Server"))
                    {
                        Console.WriteLine($"Invalid format. Disconnecting {clientInfo.Client.Client.RemoteEndPoint}");
                        return;
                    }

                    clientInfo.Name = nameMessageParts[1].Trim();
                    Console.WriteLine($"Client {clientInfo.Client.Client.RemoteEndPoint} connected with name: {clientInfo.Name}");

                    BroadcastMessage($"{clientInfo.Name} joined the chat", clientInfo);

                    while (true)
                    {
                        string data = reader.ReadLine();

                        if (data != null)
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
                                    writer.WriteLine("Invalid private message format. Use '@recipient message'");
                                }
                            }
                            else
                            {
                                BroadcastMessage($"{clientInfo.Name}: {data}", clientInfo);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Client {clientInfo.Client.Client.RemoteEndPoint} disconnected");
                    Console.ResetColor();
                    clients.Remove(clientInfo);
                }
            }

            static void BroadcastMessage(string message, ClientInfo sender)
            {
                foreach (var client in clients)
                {
                    if (client.Name != sender.Name)
                    {
                        var writer = new System.IO.StreamWriter(client.Client.GetStream(), Encoding.ASCII) { AutoFlush = true };
                        writer.WriteLine(message);
                    }
                }
            }

            static void SendPrivateMessage(string recipientName, string message, ClientInfo sender)
            {
                var recipient = clients.Find(c => c.Name == recipientName);
                if (recipient != null)
                {
                    var writer = new System.IO.StreamWriter(recipient.Client.GetStream(), Encoding.ASCII) { AutoFlush = true };
                    writer.WriteLine($"(Private) {sender.Name}: {message}");
                }
                else
                {
                    var writer = new System.IO.StreamWriter(sender.Client.GetStream(), Encoding.ASCII) { AutoFlush = true };
                    writer.WriteLine($"User '{recipientName}' not found");
                }
            }
        }
    }

    internal class ClientInfo
    {
        public TcpClient Client { get; }
        public string Name { get; set; }

        public ClientInfo(TcpClient client, string name)
        {
            Client = client;
            Name = name;
        }
    }
}
