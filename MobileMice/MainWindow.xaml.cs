using System;
using System.Windows;
using System.Windows.Threading;

namespace MobileMice
{
    /**
     * Mobile Mice Dekstop Server
     * Automatically starts server for mobile connection
     * */

    public partial class MainWindow : Window
    {
        private NetworkInfo netInfo;
        private TcpServer server;

        private DispatcherTimer ipCheckTimer;
        private bool hasIp = false;
        private int ipCheckCntr = 0;

        // laser beam
        private WindowLaser winLaser;

        public MainWindow()
        {
            InitializeComponent();

            netInfo = new NetworkInfo();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            init();

            ipCheckTimer.Start();

            server.init();

            winLaser = new WindowLaser();
            winLaser.Hide();
        }

        public void init()
        {
            initTimers();
            server = new TcpServer(this);
        }

        public void initTimers()
        {
            ipCheckTimer = new DispatcherTimer();
            ipCheckTimer.Tick += new EventHandler(ipCheckTimer_Tick);
            ipCheckTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
        }

        private void ipCheckTimer_Tick(object sender, EventArgs e)
        {
            if (!hasIp || (ipCheckCntr % 10 == 0))
            {
                // check local ip
                string localIp = "";
                if (netInfo.getLocalIp(out localIp))
                {
                    lblIp.Content = localIp;
                    hasIp = true;
                }
                else
                {
                    lblIp.Content = "";
                    hasIp = false;
                }
            }

            ipCheckCntr++;
        }

        // log console - row insert
        public void addToConsole(string msg)
        {
            lvConsole.Dispatcher.BeginInvoke((Action)(() => lvConsole.Items.Insert(0, msg)));
        }

        private void slidCoef_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (server != null)
            {
                server.changeMouseMoveCoef(e.NewValue);
            }
        }

        public void showLaserBeam(int x, int y)
        {
            winLaser.Dispatcher.Invoke(() => winLaser.Left = x);
            winLaser.Dispatcher.Invoke(() => winLaser.Top = y);
            winLaser.Dispatcher.Invoke(() => winLaser.Show());
            
            /*
            winLaser.Left = x;
            winLaser.Top = y;

            winLaser.Show();
            */
        }

        public void hideLaserBeam()
        {
            winLaser.Dispatcher.Invoke(() => winLaser.Hide());
            // winLaser.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            winLaser.Close();
        }
    }
}
