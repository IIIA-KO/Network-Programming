using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ClientWPF
{
    public partial class ClientWindow : Window
    {
        private Socket listener;

        public ClientWindow()
        {
            InitializeComponent();
        }


        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
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
        
        private async void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string message = messageTextBox.Text;

                if (message.ToLowerInvariant() == "bye")
                {
                    await listener.SendAsync(Encoding.ASCII.GetBytes($"Client left the chat\r\n"));
                    listener.Close();
                    return;
                }

                listener.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
                messageTextBox.Text = string.Empty;
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
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
}