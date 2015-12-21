using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UserClassLibrary;
using System.Threading;

namespace ChatServer
{
    class StateObject
    {
        public Socket socket = null;
        public const int size = 1024;
        public int Size
        {
            get { return size; }
        }
        public byte[] buffer = new byte[size];
        public StringBuilder result = new StringBuilder();
    }

    class Program
    {

        static ManualResetEvent eve = new ManualResetEvent(false);
        static List<User> users = new List<User>()
        {
            new User("pal", "123"),
            new User("flo", "123"),
            new User("mocr", "123")
        };

        static void StartChatServer()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 1024);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                listener.Bind(ep);
                listener.Listen(10);

                while (true)
                {
                    eve.Reset();
                    Console.WriteLine("...");
                    listener.BeginAccept(AcceptCallBack, listener);
                    eve.WaitOne();
                    eve.Set();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void AcceptCallBack(IAsyncResult res)
        {
            Socket listener = (Socket)res.AsyncState;
            Socket clientSocket = listener.EndAccept(res);
            StateObject obj = new StateObject();
            obj.socket = clientSocket;
            clientSocket.BeginReceive(obj.buffer, 0, obj.Size, 0, ReceiveCallBack, obj);
        }

        static void ReceiveCallBack(IAsyncResult res)
        {
            string content = "";
            StateObject obj = (StateObject)res.AsyncState;
            Socket clientSocket = obj.socket;

            int count = clientSocket.EndReceive(res);
            if (count > 0)
            {
                obj.result.Append(Encoding.ASCII.GetString(obj.buffer, 0, count));

                content = obj.result.ToString();
                if (content.IndexOf("|") > -1)
                {
                    Console.WriteLine("Got data \"{0}\" from client.", content.Substring(0, content.IndexOf("|")));
                    //byte[] answer = Encoding.ASCII.GetBytes("default answer");
                    List<byte> answer = new List<byte>(Encoding.ASCII.GetBytes("default answer"));
                    string cmd = content.Substring(0, content.IndexOf("$") + 1);
                    content = content.Substring(content.IndexOf("$") + 1);
                    switch (cmd)
                    {
                        case "LogToChat$":
                            {
                                string[] userInfo = content.Split(' ');
                                userInfo[1] = userInfo[1].Substring(0, userInfo[1].IndexOf("|"));

                                var succes = (from u in users where u.UserName == userInfo[0] && u.Pass == userInfo[1] select u).FirstOrDefault();
                                if (succes != null)
                                {
                                    Console.WriteLine("Succes Login!");
                                    answer.Clear();
                                    answer.AddRange(Encoding.ASCII.GetBytes("LoginSuccesCmd$"));
                                }
                                else
                                {
                                    Console.WriteLine("Fail Login...");
                                    answer.Clear();
                                    answer.AddRange(Encoding.ASCII.GetBytes("LoginFailCmd$"));
                                }
                            }
                            break;

                        case "GetUserListCmd$":
                            {
                                answer.Clear();
                                answer.AddRange(Encoding.ASCII.GetBytes("RetUserListCmd$"));
                                foreach (User u in users)
                                {
                                    answer.AddRange(Encoding.ASCII.GetBytes(u.UserName + " "));
                                }
                                Console.WriteLine("Return UserList");
                            }
                            break;
                    }


                    clientSocket.BeginSend(answer.ToArray(), 0, answer.Count, 0, SendCallback, clientSocket);
                }
                else
                {
                    clientSocket.BeginReceive(obj.buffer, 0, obj.Size, 0, ReceiveCallBack, obj);
                }
            }
        }

        static void SendCallback(IAsyncResult res)
        {
            Socket clientSocket = (Socket)res.AsyncState;
            int count = clientSocket.EndSend(res);
            Console.WriteLine("Bytes send: {0}", count);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        static void Main(string[] args)
        {
            StartChatServer();
        }
    }
}
