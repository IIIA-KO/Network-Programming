using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ServerWPF
{
    public partial class ServerWindow : Window
    {
        private Socket listener;
        private Socket clientSocket;

        public ServerWindow()
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

        private async void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (clientSocket != null)
                {
                    string message = tbMessage.Text;
                    if (message.ToLowerInvariant() == "bye")
                    {
                        await clientSocket.SendAsync(Encoding.ASCII.GetBytes("Server said goodbye ^_^\r\n"));
                        clientSocket.Close();
                        listener.Close();
                        return;
                    }

                    await clientSocket.SendAsync(Encoding.ASCII.GetBytes(tbMessage.Text + "\r\n"));
                    tbMessage.Text = string.Empty;
                }
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