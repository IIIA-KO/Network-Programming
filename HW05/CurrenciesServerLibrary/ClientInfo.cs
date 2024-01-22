using System.Net.Sockets;

namespace CurrenciesServerLibrary
{
    internal class ClientInfo
    {
        public required TcpClient TcpClient { get; set; }
        public string Name { get; set; }
        public int RequestCount { get; set; }
        public int MaxRequestCount { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
