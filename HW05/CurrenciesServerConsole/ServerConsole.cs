using CurrenciesServerLibrary;
namespace CurrenciesServerConsole
{
    internal class ServerConsole
    {   
        static void Main(string[] args)
        {
            Server server = new Server(LogUpdatedHandler, "logs.txt", 3);

            server.Start();
        }

        private static void LogUpdatedHandler(string logMessage)
        {
            Console.WriteLine(logMessage);
        }
    }
}