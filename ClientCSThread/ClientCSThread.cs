using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace ClientCSThread
{
    class Client
    {
        static void Main(string[] args)
        {
            Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 56789);
            clientSock.Connect(ep);


            string msg = "nyan";

            
            byte[] outbuf = new byte[1024];
            Helper helper = new Helper();
            helper.clientSock = clientSock;
            helper.outbuf = outbuf;

            Thread thread = new Thread(new ThreadStart(helper.ThreadReceive));
            thread.Start();

            while (clientSock.Connected)
            {
                msg = Console.ReadLine();
                byte[] buff = Encoding.UTF8.GetBytes(msg);
                clientSock.Send(buff, 0, buff.Length, SocketFlags.None);
            }
            clientSock.Close();


        }
        
    }

    class Helper
    {
        public Socket clientSock;
        public byte[] outbuf;

        public void ThreadReceive()
        {
            string output;
            while (clientSock.Connected)
            {
                clientSock.Receive(outbuf, 0, outbuf.Length, SocketFlags.None);
                output = Encoding.UTF8.GetString(outbuf, 0, outbuf.Length).Split('\0')[0];

                Console.WriteLine($"서버 : {output}");
                Array.Clear(outbuf, 0x0, outbuf.Length);
            }
        }
    }
}
