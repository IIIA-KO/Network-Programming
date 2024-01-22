using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CurrenciesServerLibrary
{
    public class Server
    {
        private Logger _logger;

        private readonly List<ClientInfo> _clients = new List<ClientInfo>();

        private Dictionary<string, decimal> _exchangeRates = new Dictionary<string, decimal>
        {
            { "USD", 1.0m },
            { "EUR", 0.85m },
            { "GBP", 0.75m },
            { "JPY", 110.25m },
            { "AUD", 1.35m },
            { "CAD", 1.25m },
            { "CHF", 0.91m },
            { "CNY", 6.43m },
            { "INR", 75.0m },
            { "BRL", 5.27m }
        };
        private readonly Dictionary<string, string> _userCredentials = new Dictionary<string, string>
        {
            { "user1", "111" },
            { "user2", "222" },
            { "user3", "333" },
            { "user4", "444" },
            { "user5", "555" },
        };

        private TcpListener _listener = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000));

        private readonly int _maxConnections;
        private int _currentConnections = 0;

        public event Action<string> LogUpdated;

        public Server(Action<string> logUpdated, string logFilePath, int maxConnections)
        {
            LogUpdated += logUpdated;
            this._logger = new Logger(logFilePath);
            this._maxConnections = maxConnections;
        }

        public void Start()
        {
            LogMessage("Currencies Server started");

            try
            {
                this._listener.Start();

                while (true)
                {
                    LogMessage("Waiting for connection....");

                    var clientInfo = new ClientInfo
                    {
                        TcpClient = this._listener.AcceptTcpClient(),
                        Name = "",
                        MaxRequestCount = 3
                    };

                    if (this._currentConnections >= this._maxConnections)
                    {
                        LogMessage("Server is at maximum capacity. Try again later.");
                        clientInfo.TcpClient.Close();
                        continue;
                    }

                    this._clients.Add(clientInfo);
                    this._currentConnections++;

                    LogMessage("Client connected");

                    Thread handleClientThread = new Thread(() => { HandleClient(clientInfo); });
                    handleClientThread.Start();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}\n{ex.StackTrace}\n{ex.HelpLink}");
            }
        }

        void HandleClient(ClientInfo clientInfo)
        {
            try
            {
                var stream = clientInfo.TcpClient.GetStream();
                var reader = new StreamReader(stream, Encoding.ASCII);
                var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                var info = reader.ReadLine();
                string[] nameMessageParts = info.Split(new[] { ' ' }, 3);
                if (nameMessageParts.Length != 3 || !nameMessageParts[0].StartsWith("@Server") || !ValidateCredentials(nameMessageParts[1], nameMessageParts[2]))
                {
                    writer.WriteLine("Invalid credentials. Disconnecting.");
                    LogConnection($"Disconnected;{clientInfo.TcpClient.Client.RemoteEndPoint};{DateTime.Now}");
                    clientInfo.TcpClient.Client.Shutdown(SocketShutdown.Both);
                    writer.Write("exit");
                    clientInfo.TcpClient.Close();
                    return;
                }
                clientInfo.Username = nameMessageParts[1].Trim();
                clientInfo.Password = nameMessageParts[2].Trim();

                LogConnection($"Connected;{clientInfo.Name};{clientInfo.TcpClient.Client.LocalEndPoint};{DateTime.Now}");

                while (true)
                {
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        LogMessage($"Message received from {clientInfo.Name}: {data}");

                        if (clientInfo.RequestCount >= clientInfo.MaxRequestCount)
                        {
                            writer.WriteLine("Max request limit reached. Try again later.");
                            break;
                        }

                        string[] currencies = data.Split(' ');
                        if (currencies.Length == 2 && this._exchangeRates.ContainsKey(currencies[0]) && this._exchangeRates.ContainsKey(currencies[1]))
                        {
                            decimal rate = this._exchangeRates[currencies[0]] / this._exchangeRates[currencies[1]];
                            writer.WriteLine($"Ratio {currencies[0]} to {currencies[1]}: {rate}");

                            clientInfo.RequestCount++;
                        }
                        else
                        {
                            writer.WriteLine("Invalid currencies input.");
                        }
                    }
                }
            }
            catch (Exception)
            {
                LogConnection($"Disconnected;{clientInfo.TcpClient.Client.RemoteEndPoint};{DateTime.Now}");
                this._clients.Remove(clientInfo);
                clientInfo.TcpClient.Close();
                throw;
            }
            finally
            {
                _currentConnections--;
            }
        }

        private void LogMessage(string logMessage)
        {
            LogUpdated?.Invoke(logMessage);
        }

        private void LogConnection(string logMessage)
        {
            this._logger.WriteLog(logMessage + "\n");
        }

        private bool ValidateCredentials(string username, string password)
        {
            return _userCredentials.ContainsKey(username) && _userCredentials[username] == password;
        }
    }
}