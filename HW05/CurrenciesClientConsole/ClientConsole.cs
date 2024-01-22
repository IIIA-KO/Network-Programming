using System.Net;
using System.Text;

namespace CurrenciesClientConsole
{
    internal class ClientConsole
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Enter name: ");
                string name = Console.ReadLine();

                Console.Write("Enter password: ");
                string password = Console.ReadLine();

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name), "Name cannot be null or empty");
                }

                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
                }

                var ip = IPAddress.Parse("127.0.0.1");
                var localEndPoint = new IPEndPoint(ip, 5000);
                var client = new System.Net.Sockets.TcpClient();
                client.Connect(localEndPoint);

                Console.WriteLine($"Client connected to {client.Client.RemoteEndPoint}");

                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.ASCII);
                var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                writer.WriteLine($"@Server {name} {password}\r\n");

                Task.Run(() =>
                {
                    ReceiveMessages(reader);
                });

                while (true)
                {
                    Console.Write("Enter two currencies separated by space (e.g. \"USD EUR\"): ");
                    string message = Console.ReadLine() ?? string.Empty;

                    if (message.ToLowerInvariant() == "exit")
                    {
                        writer.WriteLine($"Client {name} from {client.Client.LocalEndPoint} connected to server at {DateTime.Now}");
                        client.Close();
                        break;
                    }

                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                Console.ResetColor();
                Console.ReadLine();
            }
        }

        static void ReceiveMessages(StreamReader reader)
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