using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace ComputerServerWPF
{
    public partial class ComputerServerWindow : Window
    {
        private Socket listener;
        private Socket clientSocket;

        public ComputerServerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 5000));
                listener.Listen(100);

                Dispatcher.Invoke(() => logTextBox.AppendText($"Server started on 127.0.0.1:5000\n"));

                Dispatcher.Invoke(() => logTextBox.AppendText($"Waiting for connection\n"));

                clientSocket = listener.Accept();

                Dispatcher.Invoke(() => logTextBox.AppendText($"Connected: {clientSocket.LocalEndPoint}\n"));

                Task.Run(() =>
                {
                    ReceiveMessages();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSocket.Receive(buffer);
                    string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    Dispatcher.Invoke(() => logTextBox.AppendText($"Client: {receivedMessage}\n"));

                    string message = ComputerResponses.GetRandomResponse();
                    if (message.ToLowerInvariant() == "bye")
                    {
                        clientSocket.Send(Encoding.ASCII.GetBytes("Server said goodbye ^_^\r\n"));
                        clientSocket.Close();
                        listener.Close();
                        return;
                    }

                    clientSocket.Send(Encoding.ASCII.GetBytes(message));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                listener.Close();
            }
        }

        internal static class ComputerResponses
        {
            private static readonly string[] Responses =
            {
                "Hello, how can I assist you ?",
                "I’m here to help.",
                "What can I do for you today ?",
                "You’re better ask me later",
                "Thank you for contacting me.",
                "I’m happy to hear from you.",
                "How are you feeling today ?",
                "Sorry, I don’t understand your question.",
                "Please rephrase your question.",
                "I’m sorry, I can’t help you with that.",
                "Is there anything else I can do for you ?",
                "Have a nice day.",
                "I hope this helps.",
                "You’re welcome.",
                "I’m glad you’re satisfied.",
                "Bye"
            };

            public static string GetRandomResponse()
            {
                return Responses[Random.Shared.Next(Responses.Length)];
            }
        }
    }
}