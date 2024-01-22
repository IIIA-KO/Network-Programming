using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client");

            try
            {
                Console.Write("Enter name: ");
                string? name = Console.ReadLine();

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name), "Name cannot be null or empty");
                }

                var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
                var localEndPoint = new IPEndPoint(ip, 5000);
                var listener = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Connect(localEndPoint);

                Console.WriteLine($"Client connected to {listener.RemoteEndPoint}");

                listener.Send(Encoding.ASCII.GetBytes($"@Server {name}\r\n"));

                Task.Run(() =>
                {
                    ReceiveMessages(listener);
                });

                while (true)
                {
                    Console.Write("Enter message: ");
                    string message = Console.ReadLine() ?? string.Empty;
                    if (message.ToLowerInvariant() == "bye")
                    {
                        listener.Send(Encoding.ASCII.GetBytes($"{name}: left the chat\r\n"));
                        listener.Shutdown(SocketShutdown.Both);
                        listener.Close();
                        break;
                    }

                    if (message.StartsWith("@"))
                    {
                        int spaceIndex = message.IndexOf(' ');
                        if (spaceIndex != -1)
                        {
                            string recipientName = message.Substring(1, spaceIndex - 1);
                            string privateMessage = message.Substring(spaceIndex + 1).Trim();
                            listener.Send(Encoding.ASCII.GetBytes($"@{recipientName} {privateMessage}\r\n"));
                        }
                        else
                        {
                            Console.WriteLine("Invalid private message format. Use '@recipient message'");
                        }
                    }
                    else
                    {
                        listener.Send(Encoding.ASCII.GetBytes($"{name}: {message}\r\n"));
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

        static void ReceiveMessages(Socket listener)
        {
            try
            {
                while (true)
                {
                    var bytes = new byte[1024];
                    int bytesRec = listener.Receive(bytes);
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

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