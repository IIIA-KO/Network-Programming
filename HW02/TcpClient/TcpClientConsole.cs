using System.Net;
using System.Text;

namespace TcpClient
{
    internal class TcpClientConsole
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

                var ip = IPAddress.Parse("127.0.0.1");
                var localEndPoint = new IPEndPoint(ip, 5000);
                var client = new System.Net.Sockets.TcpClient();
                client.Connect(localEndPoint);

                Console.WriteLine($"Client connected to {client.Client.RemoteEndPoint}");

                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.ASCII);
                var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                writer.WriteLine($"@Server {name}\r\n");

                Task.Run(() =>
                {
                    ReceiveMessages(reader);
                });

                writer.WriteLine($"{name} joined the chat");

                while (true)
                {
                    Console.Write("Enter message: ");
                    string message = Console.ReadLine() ?? string.Empty;

                    if (message.ToLowerInvariant() == "bye")
                    {
                        writer.WriteLine($"{name} left the chat");
                        client.Close();
                        break;
                    }

                    if (message.StartsWith("@"))
                    {
                        int spaceIndex = message.IndexOf(' ');
                        if (spaceIndex != -1)
                        {
                            string recipientName = message.Substring(1, spaceIndex - 1);
                            string privateMessage = message.Substring(spaceIndex + 1).Trim();
                            writer.WriteLine($"@{recipientName} {privateMessage}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid private message format. Use '@recipient message'");
                        }
                    }
                    else
                    {
                        writer.WriteLine(message);
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

        static void ReceiveMessages(System.IO.StreamReader reader)
        {
            try
            {
                while (true)
                {
                    string data = reader.ReadLine();
                    Console.WriteLine($"Received: {data}");
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