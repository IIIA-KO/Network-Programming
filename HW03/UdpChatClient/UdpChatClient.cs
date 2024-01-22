using System.Net.Sockets;
using System.Net;
using System.Text;

namespace UdpChatClient
{
    internal class UdpChatClient
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client");

            try
            {
                Console.Write("Enter name: ");
                string name = Console.ReadLine();

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name), "Name cannot be null or empty");
                }

                UdpClient udpClient = new UdpClient();
                udpClient.Connect("127.0.0.1", 5000);

                Console.WriteLine($"Client connected to {udpClient.Client.LocalEndPoint}");

                udpClient.Send(Encoding.ASCII.GetBytes($"@Server {name}\r\n"), Encoding.ASCII.GetBytes($"@Server {name}\r\n").Length);

                Task.Run(() =>
                {
                    ReceiveMessages(udpClient);
                });

                udpClient.Send(Encoding.ASCII.GetBytes($"{name} joined the chat"));

                while (true)
                {
                    Console.Write("Enter message: ");
                    string message = Console.ReadLine() ?? string.Empty;

                    if (message.ToLowerInvariant() == "bye")
                    {
                        udpClient.Send(Encoding.ASCII.GetBytes($"{name}: left the chat\r\n"), Encoding.ASCII.GetBytes($"{name}: left the chat\r\n").Length);
                        udpClient.Close();
                        break;
                    }

                    if (message.StartsWith("@"))
                    {
                        int spaceIndex = message.IndexOf(' ');
                        if (spaceIndex != -1)
                        {
                            string recipientName = message.Substring(1, spaceIndex - 1);
                            string privateMessage = message.Substring(spaceIndex + 1).Trim();
                            udpClient.Send(Encoding.ASCII.GetBytes($"@{recipientName} {privateMessage}\r\n"), Encoding.ASCII.GetBytes($"@{recipientName} {privateMessage}\r\n").Length);
                        }
                        else
                        {
                            Console.WriteLine("Invalid private message format. Use '@recipient message'");
                        }
                    }
                    else
                    {
                        udpClient.Send(Encoding.ASCII.GetBytes(message), Encoding.ASCII.GetBytes(message).Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                Console.ResetColor();
                Console.ReadLine();
            }

            Console.WriteLine();
        }

        static void ReceiveMessages(UdpClient udpClient)
        {
            try
            {
                while (true)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receiveBytes = udpClient.Receive(ref endPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine($"Received: {returnData}");
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Server disconnected");
                Console.ResetColor();
            }
        }
    }
}
