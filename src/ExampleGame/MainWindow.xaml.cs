using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace ExampleGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Player LocalPlayer { get; set; }
        Player RemotePlayer { get; set; }
        string ClientIpAddress { get; set; }
        Thread serverThread = null;
        int ListenPort = 3461;
        int RemotePort = 3461;

        public MainWindow()
        {
            InitializeComponent();
            ClientIpAddress = "";
            LocalPlayer = new Player();
            RemotePlayer = new Player();
            RemotePlayer.PlayerShape.Fill = Brushes.BlueViolet;

            this.MouseMove += GameScene_MouseMove;

            InitializePlayer(LocalPlayer);
            InitializePlayer(RemotePlayer);

        }

        private void GameScene_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(GameScene);
            position.X -= LocalPlayer.ActualWidth / 2;
            position.Y -= LocalPlayer.ActualHeight / 2;
            if(position.X > 0 && position.Y > 0)
            {
                Canvas.SetTop(LocalPlayer, position.Y);
                Canvas.SetLeft(LocalPlayer, position.X);
            }
            SendUpdate(position.X, position.Y);
        }

        void InitializePlayer(Player p)
        {
            GameScene.Children.Add(p);
            Canvas.SetLeft(p, 0);
            Canvas.SetTop(p, 0);
        }


        private void SetRemoteIpClicked(object sender, RoutedEventArgs e)
        {
            Settings s = new Settings();
            s.ShowDialog();
            if(s.ModalResult == true)
            {
                ClientIpAddress = s.IpAddress;
                ListenPort = s.LocalPort;
                RemotePort = s.RemotePort;

                //I think this will create a new thread every time we edit settings.
                //Definitely not a great idea but works for our proof of concept.
                StartServerLoop();
            }
        }

        private void SendUpdate(double xPos, double yPos)
        {
            UdpClient client = new UdpClient();
            byte[] package = new byte[1024];

            //TODO: build package payload
            string x = xPos.ToString();
            string y = yPos.ToString();
            string coords = x + "," + y;
            package = Encoding.UTF8.GetBytes(coords);

            try
            {
                if(ClientIpAddress.Length > 0)
                {
                    client.Send(package, package.Length, ClientIpAddress, RemotePort);
                }
            }
            catch(Exception ex)
            {
                Debug.Write(ex.Message);
            }
        }

        private void StartServerLoop()
        {
            ThreadStart ts = () => { ServerLoop(); };
            serverThread = new Thread(ts);
            serverThread.Start();
        }

        private void ServerLoop()
        {
            UdpClient client = null;
            try
            {
                client = new UdpClient(ListenPort);
                IPEndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);
                while(true)
                {
                    byte[] clientData = client.Receive(ref remoteClient);
                    Debug.Write("data received");
                    //TODO: process data
                    string data = Encoding.UTF8.GetString(clientData);
                    string[] coords = data.Split(','); 
                    double yPos = double.Parse(coords[0]);
                    double xPos = double.Parse(coords[1]);
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        Canvas.SetTop(RemotePlayer, xPos);
                        Canvas.SetLeft(RemotePlayer, yPos);
                    }));

                }
            }
            catch(Exception ex)
            {
                Debug.Write(ex.Message);

                //restart server thread on error
                Thread.Sleep(500);
                StartServerLoop();
            }
        }
    }
}
