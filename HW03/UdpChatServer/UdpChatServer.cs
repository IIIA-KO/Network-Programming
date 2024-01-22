using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpChatServer
{
    internal class UdpChatServer
    {
        static List<ClientInfo> clients = new List<ClientInfo>();

        static void Main(string[] args)
        {
            Console.WriteLine("Server started!");

            UdpClient udpClient = new UdpClient(5000);

            while (true)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);
                byte[] receiveBytes = udpClient.Receive(ref endPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                var clientInfo = clients.FirstOrDefault(c => c.EndPoint.Equals(endPoint));

                string[] nameMessageParts = returnData.Split(new[] { ' ' }, 2);

                if (clientInfo == null)
                {
                    clientInfo = new ClientInfo(endPoint, nameMessageParts[1].Trim());
                    clients.Add(clientInfo);
                    Console.WriteLine($"Client {clientInfo.EndPoint} connected with name: {clientInfo.Name}");

                    if (nameMessageParts.Length != 2 || !nameMessageParts[0].StartsWith("@Server"))
                    {
                        Console.WriteLine($"Invalid format. Disconnecting {clientInfo.EndPoint.Address}");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"Message received from {clientInfo.Name}: {returnData}");

                    if (returnData.StartsWith("@"))
                    {
                        int spaceIndex = returnData.IndexOf(' ');
                        if (spaceIndex != -1)
                        {
                            string recipientName = returnData.Substring(1, spaceIndex - 1);
                            string message = returnData.Substring(spaceIndex + 1).Trim();
                            SendPrivateMessage(recipientName, message, clientInfo, udpClient);
                        }
                        else
                        {
                            var errorMsg = Encoding.ASCII.GetBytes($"Invalid private message format. Use '@recipient message'\r\n");
                            udpClient.Send(errorMsg, errorMsg.Length, clientInfo.EndPoint);
                        }
                    }
                    else
                    {
                        BroadcastMessage(returnData, clientInfo, udpClient);
                    }
                }
            }
        }

        static void BroadcastMessage(string message, ClientInfo sender, UdpClient udpClient)
        {
            foreach (var client in clients)
            {
                if (client != sender)
                {
                    var msg = Encoding.ASCII.GetBytes($"{sender.Name}: {message}");
                    udpClient.Send(msg, msg.Length, client.EndPoint);
                }
            }
        }

        static void SendPrivateMessage(string recipientName, string message, ClientInfo sender, UdpClient udpClient)
        {
            var recipient = clients.Find(c => c.Name == recipientName);
            if (recipient != null)
            {
                var msg = Encoding.ASCII.GetBytes($"(Private) {sender.Name}: {message}\r\n");
                udpClient.Send(msg, msg.Length, recipient.EndPoint);
            }
            else
            {
                var msg = Encoding.ASCII.GetBytes($"User '{recipientName}' not found\r\n");
                udpClient.Send(msg, msg.Length, sender.EndPoint);
            }
        }
    }

    internal class ClientInfo
    {
        public IPEndPoint EndPoint { get; }
        public string Name { get; set; }

        public ClientInfo(IPEndPoint endPoint, string name)
        {
            EndPoint = endPoint;
            Name = name;
        }
    }
}