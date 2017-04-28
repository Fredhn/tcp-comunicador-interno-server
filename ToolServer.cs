using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommunicationTool_Server
{
    public partial class ToolServer : Form
    {
        private bool serverStatus = false;
        private byte[] _buffer = new byte[1024];
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ToolServer()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            __ClientSockets = new List<SocketT2h>();
        }

        private void btn_startServer_Click(object sender, EventArgs e)
        {
            ControlServerStatus();
        }

        private void ControlServerStatus()
        {
            if (serverStatus == false)
            {
                serverStatus = true;
                SetupServer();
                btn_startServer.Enabled = false;
                btn_startServer.Text = "Servidor ON";
                btn_startServer.BackColor = System.Drawing.Color.Red;
            }
        }
        private void SetupServer()
        {
            richTextBox_chatStream.Text += "Iniciando servidor . . . \n";
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void AppceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            __ClientSockets.Add(new SocketT2h(socket));
            //list_Client.Items.Add(socket.RemoteEndPoint.ToString());

            //lb_soluong.Text = "Số client đang kết nối: " + __ClientSockets.Count.ToString();
            richTextBox_chatStream.Text += "Client connected. . . \n";
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {

            Socket socket = (Socket)ar.AsyncState;
            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(ar);
                }
                catch (Exception)
                {
                    // client đóng kết nối
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            //lb_soluong.Text = "Số client đang kết nối: " + __ClientSockets.Count.ToString();
                        }
                    }
                    // xóa trong list
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];
                    Array.Copy(_buffer, dataBuf, received);
                    string text = Encoding.ASCII.GetString(dataBuf);
                    //lb_stt.Text = "Text received: " + text;

                    string reponse = string.Empty;
                    //if (text.Contains("@@"))
                    //{
                    //    for (int i = 0; i < list_Client.Items.Count; i++)
                    //    {
                    //        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                    //        {
                    //            list_Client.Items.RemoveAt(i);
                    //            list_Client.Items.Insert(i, text.Substring(1, text.Length - 1));
                    //            __ClientSockets[i]._Name = text;
                    //            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                    //            return;
                    //        }
                    //    }
                    //}

                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            richTextBox_chatStream.AppendText(__ClientSockets[i]._Name + ": " + text + "\n");
                        }
                    }

                    for (int j = 0; j < __ClientSockets.Count; j++)
                    {
                        if (!(socket.RemoteEndPoint.ToString().Equals(__ClientSockets[j]._Socket.RemoteEndPoint.ToString())))
                        {
                            Sendata(__ClientSockets[j]._Socket, __ClientSockets[j]._Name + ": " + text + "\n");
                        }
                    }


                    if (text == "bye \n")
                    {
                        return;
                    }
                    reponse = ": " + text + "\n";
                    Sendata(socket, reponse);
                }
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            //lb_soluong.Text = "Số client đang kết nối: " + __ClientSockets.Count.ToString();
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        void Sendata(Socket socket, string noidung)
        {
            byte[] data = Encoding.ASCII.GetBytes(noidung);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        //private void btnSend_Click(object sender, EventArgs e)
        //{
        //    for (int j = 0; j < __ClientSockets.Count; j++)
        //    {
        //        //if (__ClientSockets[j]._Socket.Connected && __ClientSockets[j]._Name.Equals("@"+t))
        //        {
        //            Sendata(__ClientSockets[j]._Socket, richTextBox_chatInput.Text);
        //        }
        //    }
        //    richTextBox_chatStream.AppendText("Server: " + richTextBox_chatInput.Text + "\n");
        //}
        private void pictureBox_BroadcastMsg_Click(object sender, EventArgs e)
        {
            if (richTextBox_chatInput.Text != "")
            {
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    //if (__ClientSockets[j]._Socket.Connected && __ClientSockets[j]._Name.Equals("@"+t))
                    {
                        Sendata(__ClientSockets[j]._Socket, ": Server diz: " + richTextBox_chatInput.Text + "\n");
                    }
                }
                richTextBox_chatStream.AppendText(": Server diz: " + richTextBox_chatInput.Text + "\n");
                richTextBox_chatInput.Text = "";
            }
        }
        private void CheckEnterKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)

            {
                if (richTextBox_chatInput.Text != "")
                {
                    for (int j = 0; j < __ClientSockets.Count; j++)
                    {
                        //if (__ClientSockets[j]._Socket.Connected && __ClientSockets[j]._Name.Equals("@"+t))
                        {
                            Sendata(__ClientSockets[j]._Socket, ": Server diz: " + richTextBox_chatInput.Text + "\n");
                        }
                    }
                    richTextBox_chatStream.AppendText(": Server diz: " + richTextBox_chatInput.Text + "\n");
                    richTextBox_chatInput.Text = "";
                }
            }
        }

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            richTextBox_chatStream.SelectionStart = richTextBox_chatStream.Text.Length;
            // scroll it automatically
            richTextBox_chatStream.ScrollToCaret();
        }
    }
    public class SocketT2h
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketT2h(Socket socket)
        {
            this._Socket = socket;
        }
    }
}
