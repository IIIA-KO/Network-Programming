using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace ComputerClientWPF
{
    public partial class ComputerClientWindow : Window
    {
        private Socket listener;

        public ComputerClientWindow()
        {
            InitializeComponent();

            Thread.Sleep(1000);

            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                string ipAddress = ipTextBox.Text;
                int port;

                if (!int.TryParse(portTextBox.Text, out port))
                {
                    MessageBox.Show("Invalid port number", "Error");
                    return;
                }

                listener.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));

                Task.Run(() =>
                {
                    ReceiveMessages();
                });
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Error connecting to the server: {ex.Message}", "Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error");
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    string message = ClientRequests.GetRandomRequest();

                    if (message.ToLowerInvariant() == "bye")
                    {
                        listener.Send(Encoding.ASCII.GetBytes($"Client left the chat\r\n"));
                        listener.Close();
                        return;
                    }

                    listener.Send(Encoding.ASCII.GetBytes(message));

                    byte[] buffer = new byte[1024];
                    int bytesRead = listener.Receive(buffer);
                    string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    Dispatcher.Invoke(() => receivedMessageTextBlock.AppendText($"Server: {receivedMessage}\n"));
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
    }

    internal static class ClientRequests
    {
        private static readonly string[] Requests =
        {
            "How are you?",
            "Can you help me with something?",
            "What's the weather like today?",
            "Tell me a joke.",
            "What's the latest news?",
            "Recommend a good movie.",
            "Do you like music?",
            "What's your favorite color?",
            "Tell me about yourself.",
            "What's the meaning of life?",
            "Any interesting facts you can share?",
            "Do you believe in aliens?",
            "What's your favorite hobby?",
            "What's on your mind?",
            "Tell me a fun fact.",
            "Bye"
        };

        public static string GetRandomRequest()
        {
            return Requests[Random.Shared.Next(Requests.Length)];
        }
    }
}