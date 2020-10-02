using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ExampleGame
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public string IpAddress
        {
            get;
            private set;
        }

        public int RemotePort { get; set; }
        public int LocalPort { get; set; }
        public bool ModalResult { get; private set; }
        

        public Settings()
        {
            InitializeComponent();
        }


        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            IpAddress = ClientIpAddress.Text;
            int port = -1;
            if(Int32.TryParse(ClientRemotePort.Text, out port))
            {
                RemotePort = port;
            }
            if(Int32.TryParse(LocalListenPort.Text, out port))
            {
                LocalPort = port;
            }
            ModalResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ModalResult = false;
            this.Close();
        }
    }
}
