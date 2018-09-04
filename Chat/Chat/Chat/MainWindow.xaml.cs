using System.Windows;
using static Chat.ClientSide;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SocketsClient client;
        public MainWindow()
        {
            InitializeComponent();
            client = new SocketsClient();
            client.logFunction = UpdateLog;
            client.updateScreenAfterDisconnect = UpdateDisconnect; 
            Port.IsEnabled = true;
            IP.IsEnabled = true;
            MesClient.IsEnabled = false;
            SendClient.IsEnabled = false;
            DisConnect.IsEnabled = false;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectClient())
            {
                Port.IsEnabled = false;
                IP.IsEnabled = false;
                MesClient.IsEnabled = true;
                Connect.IsEnabled = false;
                DisConnect.IsEnabled = true;
                SendClient.IsEnabled = true;
            }
        }

        private void DisConnect_Click(object sender, RoutedEventArgs e)
        {
            DisconnectClient();
            UpdateDisconnect();
            DisConnect.IsEnabled = false;
            Connect.IsEnabled = true;
        }

        private void SendClient_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private bool ConnectClient()
        {
            return client.Connect(IP.Text, Port.Text);
        }

        private void DisconnectClient()
        {
            client.Disconnect();
        }

        private void SendMessage()
        {
            client.Send(MesClient.Text);
        }

        private void UpdateDisconnect()
        {
            Port.IsEnabled = true;
            IP.IsEnabled = true;
            MesClient.IsEnabled = false;
            SendClient.IsEnabled = false;
        }

        private void UpdateLog(string s)
        {
            ClientLog.Items.Add(s);
        }
    }
}
