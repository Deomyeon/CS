using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace ServerCSThread
{
    class Server
    {
        static void Main(string[] args)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 56789);
            sock.Bind(ep);

            sock.Listen(10);

            Socket clientSock = sock.Accept();

            string s;

            byte[] buff = new byte[1024];
            Helper helper = new Helper();
            helper.clientSock = clientSock;
            helper.buff = buff;

            Thread thread = new Thread(new ThreadStart(helper.ThreadReceive));
            thread.Start();

            while (clientSock.Connected)
            {
                s = Console.ReadLine();
                byte[] outbuff = Encoding.UTF8.GetBytes(s);
                clientSock.Send(outbuff, 0, outbuff.Length, SocketFlags.None);
            }
            clientSock.Close();
        }
    }

    class Helper
    {
        public Socket clientSock;
        public byte[] buff;

        public void ThreadReceive()
        {
            string output;
            while (clientSock.Connected)
            {
                clientSock.Receive(buff, 0, buff.Length, SocketFlags.None);
                output = Encoding.UTF8.GetString(buff, 0, buff.Length).Split('\0')[0];

                Console.WriteLine($"클라이언트 : {output}");
                Array.Clear(buff, 0x0, buff.Length);
            }
        }
    }
}
