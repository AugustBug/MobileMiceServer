using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileMice
{
    /**
     *  *** MESSAGE FORMAT ***
     *  Header($) + Message Code + Parameters with seperator(;) + Footer(#)
     *  
     *  => Mouse Move
     *  $MM;Px;Py#
     *  Px: mouse x distance, pos or neg
     *  Py: mouse y distance, pos or neg
     *  
     *  => Mouse Click
     *  $MC;Code;Action#
     *  Code: R for right click, L for left click
     *  Action: 0 for release, 1 for press
     *  
     *  => Scroll
     *  $SC;Dir#
     *  Dir: U for upward, D for downward
     * 
     *  => Functions
     *  $FC;Code;Action#
     *  Code: R for right action, L for left action, T for top action
     *  Action: 0 for release, 1 for press
     *  
     *  => Screen Point
     *  $SP;Px;Py#
     *  Px: mouse x position, positive
     *  Py: mouse y position, positive
     *  
     **/

    class TcpServer
    {
        private MainWindow parentGui;

        private int appPort = 2262;
        private byte[] data = new byte[1024];
        private int size = 1024;
        private Socket server;

        private double mouseMoveCoef = 0.3;

        private double cummX, cummY;

        private const int PAGE_UP_VK = 0x21;
        private const int PAGE_DOWN_VK = 0x22;
        //private const int PAGE_UP_VK = 0x25;
        //private const int PAGE_DOWN_VK = 0x27;
        private const int ESCAPE_VK = 0x1B;

        public TcpServer(MainWindow parentGui)
        {
            this.parentGui = parentGui;
        }

        public void init()
        {
            server = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, appPort);
            server.Bind(iep);
            server.Listen(5);
            server.BeginAccept(new AsyncCallback(acceptConn), server);

            parentGui.addToConsole("Sunucu başlatıldı.");
        }

        public void changeMouseMoveCoef(double coef)
        {
            mouseMoveCoef = coef;
        }

        void acceptConn(IAsyncResult iar)
        {
            Socket oldserver = (Socket)iar.AsyncState;
            Socket client = oldserver.EndAccept(iar);
            parentGui.addToConsole("Cihaz bağlandı. " + client.RemoteEndPoint.ToString());
            string stringData = "Welcome to Mobile Mice Server";
            byte[] message1 = Encoding.ASCII.GetBytes(stringData);
            client.BeginSend(message1, 0, message1.Length, SocketFlags.None,
                        new AsyncCallback(SendData), client);
        }

        void SendData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int sent = client.EndSend(iar);
            client.BeginReceive(data, 0, size, SocketFlags.None,
                        new AsyncCallback(ReceiveData), client);
        }

        void ReceiveData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int recv = client.EndReceive(iar);
            if (recv == 0)
            {
                client.Close();
                parentGui.addToConsole("Bağlantı bekleniyor...");
                server.BeginAccept(new AsyncCallback(acceptConn), server);
                return;
            }
            string receivedData = Encoding.ASCII.GetString(data, 0, recv);
            parentGui.addToConsole(">>" + receivedData);
            parseMessage(receivedData);
            byte[] message2 = Encoding.ASCII.GetBytes(receivedData);
            client.BeginSend(message2, 0, message2.Length, SocketFlags.None,
                         new AsyncCallback(SendData), client);
        }

        private void parseMessage(string msg)
        {
            if (msg.StartsWith("$"))
            {
                int end = msg.IndexOf('#');
                if (end >= 0)
                {
                    string msgIn = msg.Substring(1,end-1);
                    // Console.WriteLine(msgIn);
                    int sepIndex = msgIn.IndexOf(';');
                    if (sepIndex >= 0)
                    {
                        string[] parts = msgIn.Split(';');

                        string msgCode = parts[0];

                        if (msgCode.Equals("MM") && parts.Length > 2)
                        {
                            int mX = 0, mY = 0;
                            int.TryParse(parts[1], out mX);
                            int.TryParse(parts[2], out mY);
                            mouseMoveEvent(mX, mY);
                        }
                        else if (msgCode.Equals("MC") && parts.Length > 2)
                        {
                            mouseClickEvent(parts[1], parts[2]);
                        }
                        else if (msgCode.Equals("FC") && parts.Length > 2)
                        {
                            functionClickEvent(parts[1], parts[2]);
                        }
                        else if (msgCode.Equals("SP") && parts.Length > 2)
                        {
                            int sX = 0, sY = 0;
                            int.TryParse(parts[1], out sX);
                            int.TryParse(parts[2], out sY);
                            screenPositionEvent(sX, sY);
                        }
                    }
                }
            }
        }

        private void mouseMoveEvent(int distX, int distY)
        {
            // y negative
            distY = -1 * distY;

            if (distX < -2000)
                distX = -2000;
            if (distX > 2000)
                distX = 2000;
            if (distY < -2000)
                distY = -2000;
            if (distY > 2000)
                distY = 2000;

            double dist = Math.Sqrt(distX * distX + distY * distY);
            double sqDist = Math.Sqrt(dist/2);

            double deltaX = distX * sqDist * mouseMoveCoef + cummX;
            double deltaY = distY * sqDist * mouseMoveCoef + cummY;

            cummX = deltaX % 1;
            cummY = deltaY % 1;

            MouseKbEmulator.SetCursorPositionDelta((int)deltaX, (int)deltaY);

        }

        private void screenPositionEvent(int sX, int sY)
        {
            // y negative
            sY = 2000 - sY;

            if (sX < 0)
                sX = 0;
            if (sX > 2000)
                sX = 2000;
            if (sY < 0)
                sY = 0;
            if (sY > 2000)
                sY = 2000;

            if (sX == 0 && sY == 2000)
            {
                // release laser beam
                parentGui.hideLaserBeam();
            }
            else
            {
                double posX = (sX * System.Windows.SystemParameters.WorkArea.Width) / 2000;
                double posY = (sY * System.Windows.SystemParameters.WorkArea.Height) / 2000;

                parentGui.showLaserBeam((int)posX, (int)posY);
                //MouseKbEmulator.SetCursorPosition((int)posX, (int)posY);
            }

        }

        private void mouseClickEvent(string code, string action)
        {
            if (!action.Equals("0") && !action.Equals("1"))
            {
                return;
            }

            bool isOn = action.Equals("1") ? true : false;

            if (code.Equals("L") || code.Equals("l"))
            {
                MouseKbEmulator.MouseEvent(isOn ? MouseKbEmulator.MouseEventFlags.LeftDown :
                    MouseKbEmulator.MouseEventFlags.LeftUp);
            }
            else if (code.Equals("R") || code.Equals("r"))
            {
                MouseKbEmulator.MouseEvent(isOn ? MouseKbEmulator.MouseEventFlags.RightDown :
                    MouseKbEmulator.MouseEventFlags.RightUp);
            }
        }

        private void functionClickEvent(string code, string action)
        {
            if (!action.Equals("0") && !action.Equals("1"))
            {
                return;
            }

            bool isOn = action.Equals("1") ? true : false;

            if (code.Equals("L") || code.Equals("l"))
            {
                if (isOn)
                {
                    MouseKbEmulator.pressKeyDown(PAGE_UP_VK);
                }
                else
                {
                    MouseKbEmulator.pressKeyUp(PAGE_UP_VK);
                }

            }
            else if (code.Equals("R") || code.Equals("r"))
            {
                if (isOn)
                {
                    MouseKbEmulator.pressKeyDown(PAGE_DOWN_VK);
                }
                else
                {
                    MouseKbEmulator.pressKeyUp(PAGE_DOWN_VK);
                }
            }
            else if (code.Equals("T") || code.Equals("t"))
            {
                if (isOn)
                {
                    MouseKbEmulator.pressKeyDown(ESCAPE_VK);
                }
                else
                {
                    MouseKbEmulator.pressKeyUp(ESCAPE_VK);
                }
            }
        }
    }
}
