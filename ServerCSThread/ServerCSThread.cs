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

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 5678);
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
                byte[] outbuff = Encoding.Default.GetBytes(s);
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
            int nbytes = 0;
            string output;
            while (clientSock.Connected)
            {
                clientSock.Receive(buff, 0, buff.Length, SocketFlags.None);
                for (int i = 0; i < buff.Length; i++)
                {
                    if (buff[i] == 0)
                    {
                        nbytes = i;
                        break;
                    }
                }
                output = Encoding.Default.GetString(buff, 0, nbytes);

                Console.WriteLine($"클라이언트 : {output}");
                Array.Clear(buff, 0x0, buff.Length);
            }
        }
    }
}
