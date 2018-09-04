using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServerSide server;
        public MainWindow()
        {
            InitializeComponent();
            server = new ServerSide();
            server.logFunction = UpdateLog;
            MesServer.IsEnabled = false;
            SendServer.IsEnabled = false;
            Port.IsEnabled = true;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
            MesServer.IsEnabled = true;
            SendServer.IsEnabled = true;
            Port.IsEnabled = false;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopServer();
            MesServer.IsEnabled = false;
            SendServer.IsEnabled = false;
            Port.IsEnabled = true;
        }

        private void SendServer_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void StartServer()
        {
            server.Start(Port.Text);
        }

        private void StopServer()
        {
            server.Stop();
        }

        private void SendMessage()
        {
            server.SendToClients(MesServer.Text);
        }

        private void UpdateLog(string s)
        {
            ServerLog.Items.Add(s);
        }

        private void MesServer_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
