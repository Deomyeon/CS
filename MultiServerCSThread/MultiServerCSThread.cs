using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace MultiServerCSThread
{
    class Server
    {
        static public Thread[] threads;
        static public Queue<string>[] queues;
        static public Helper[] helpers;
        static public bool[]   clientConnect;

        static void Main(string[] args)
        {
            int roomsize = -1;
            string text;
            Console.WriteLine("몇명 들어올 수 있게 하시겠어요?");
            while (true)
            {
                roomsize = int.Parse(Console.ReadLine());
                if (roomsize >= 10)
                {
                    Console.WriteLine("컴 터지겠어요!!");
                    continue;
                }
                break;
            }
            threads = new Thread[roomsize];
            queues = new Queue<string>[roomsize];
            helpers = new Helper[roomsize];
            clientConnect = new bool[roomsize];

            for (int i = 0; i < roomsize; i++)
            {
                helpers[i] = new Helper();
                queues[i] = new Queue<string>();
                clientConnect[i] = new bool();
                if (i == 0)
                {
                    threads[i] = new Thread(new ThreadStart(helpers[i].ThreadConnect));
                    helpers[i].index = i;
                    threads[i].Start();
                }
                else
                {
                    while (true)
                    {
                        if (clientConnect[i - 1])
                        {
                            threads[i] = new Thread(new ThreadStart(helpers[i].ThreadConnect));
                            helpers[i].index = i;
                            threads[i].Start();
                            break;
                        }
                    }
                }
            }

            while (true)
            {
                text = Console.ReadLine();
                byte[] outbuff = Encoding.Default.GetBytes(text);
                for (int i = 0; i < roomsize; i++)
                {
                    if (clientConnect[i])
                    {
                        helpers[i].clientSock.Send(outbuff, 0, outbuff.Length, SocketFlags.None);
                    }
                }
            }
        }
    }

    class Helper
    {
        public Socket clientSock;
        public byte[] buff;
        public string output;
        public int index;
        public void ThreadConnect()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 56789);

            sock.Bind(ep);
            sock.Listen(3);
            clientSock = sock.Accept();
            sock.Close();
            Server.clientConnect[index] = true;

            buff = new byte[1024];

            Thread thread = new Thread(new ThreadStart(ThreadReceive));
            thread.Start();

            while (clientSock.Connected)
            {
                if (Server.queues[index].Count > 0)
                {
                    output = Server.queues[index].Peek();
                    Server.queues[index].Dequeue();
                    byte[] outbuff = Encoding.Default.GetBytes(output);
                    clientSock.Send(outbuff, 0, outbuff.Length, SocketFlags.None);
                }
            }
            clientSock.Close();
        }
        public void ThreadReceive()
        {
            while (clientSock.Connected)
            {
                clientSock.Receive(buff, 0, buff.Length, SocketFlags.None);
                output = Encoding.Default.GetString(buff, 0, buff.Length);
                Server.queues[index].Enqueue(output);
                Console.WriteLine($"사용자 {index} : {output}");
                Array.Clear(buff, 0x0, buff.Length);
            }
        }
    }
}
