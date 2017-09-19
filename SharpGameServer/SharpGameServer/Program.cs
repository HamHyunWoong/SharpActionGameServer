using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;

namespace SharpGameServer
{
    class Program
    {

        public static Socket server;
        private static int port;
        private static IPEndPoint ipEndPoint;
        private static List<Socket> clientList = new List<Socket>();
        static string readMassage;

        static void Main(string[] args)
        {
            try
            {
                Program mainServer = new Program();
                mainServer.StartServer();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void StartServer()
        {

            Console.WriteLine("Server IP Address : " + Get_MyIP());
            Console.WriteLine("************************************************************");
            Console.Write("Port Number : ");

            port = int.Parse(Console.ReadLine());
            Console.WriteLine("My port is " + port);
            //Create Socket
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Init IP and Port
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            server.Bind(ipEndPoint);
            server.Listen(1000); //Limit Client User


            while (true)
            {
                Console.WriteLine("Client Waiting...");
                //Client Waiting and Accept Socket
                Socket client = server.Accept();

                //Client IP Address 
                string address = client.RemoteEndPoint.ToString();

                // addressArray[0] = IP, addressArray[1] = Port
                string[] addressArray = address.Split(new char[] { ':' });

                //Show Connected Client Address
                Console.WriteLine("클라이언트 접속 성공! \n 클라이언트 정보 " + address);

                //Create Stream
                NetworkStream networkStream = new NetworkStream(client);
                StreamReader streamReader = new StreamReader(networkStream);

                //Update ClientList
                clientList.Add(client);

                //BroadCast User Connect Message
                TcpBroadCast("<<[" + address + "] 님이 채팅방에 접속하셨습니다. >>");

                //Create Service Thread
                Thread serviceThread = new Thread(delegate ()
                {
                    try
                    {
                        Run(client, networkStream, streamReader);
                    }
                    catch (Exception e)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            Socket sk = clientList.ToArray<Socket>()[i];
                            if (sk == client)
                            {
                                Console.WriteLine("접속해제 클라이언트 정보 : " + address);

                                //Client List
                                clientList.RemoveAt(i);
                                client.Close();

                                //BroadCast User DisConnect Message
                                TcpBroadCast("<<[" + address + "] 님이 채팅방에서 나가셨습니다.>>");
                            }
                        }
                    }
                });
                //Start Thread
                serviceThread.Start();


            }

        }



        private static string Get_MyIP()
        {
            IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
            string myip = host.AddressList[0].ToString();
            return myip;
        }


        //Service Thread 
        static void Run(Socket client, NetworkStream networkStream, StreamReader streamReader)
        {
            Console.WriteLine("서비스스레드 시작");
            while (true)
            {
                Thread.Sleep(10);
                if (!client.Connected)
                {
                    int i = 0;
                    foreach (Socket sk in clientList)
                    {

                        if (sk == client)
                        {

                            clientList.RemoveAt(i);
                            client.Close();

                            break;

                        }
                        i++;
                    }

                }
                else
                {

                    Console.WriteLine(".");

                    while ((readMassage = streamReader.ReadLine()) != null)
                    {
                        //Read Massage from Client
                        Console.WriteLine("수신된 메시지 : " + readMassage);

                        string[] msgArr = readMassage.Split(new char[] {','}); //콤마로 구분 

                        // msgArr[0] = ADD 이면 가입 처리 
                        // msgArr[0] = LOGIN 이면 로그인
                        // msgArr[0] = LOGOUT 이면 로그아웃
                        // msgArr[0] = 



                        if () {



                        }




                        //BroadCast Server to Client
                        TcpBroadCast(readMassage);
                    }


                }

            }

            //End Service Thread
            Console.WriteLine("서비스스레드 종료됨");

        }

        static void TcpBroadCast(string message)
        {


            foreach (Socket sk in clientList)
            {
                if (sk != null)
                {
                    //Send
                    try
                    {
                        NetworkStream networkStream = new NetworkStream(sk);
                        StreamWriter streamWriter = new StreamWriter(networkStream);

                        Console.WriteLine(message);
                        streamWriter.WriteLine(message);
                        streamWriter.Flush();


                        networkStream = null;
                        streamWriter = null;
                    }
                    catch (Exception e)
                    {
                        int i = 0;
                        foreach (Socket sk2 in clientList)
                        {

                            if (sk == sk2)
                            {

                                clientList.RemoveAt(i);
                                sk2.Close();


                                break;

                            }
                            i++;
                        }


                    }


                }
            }
        }
    }






}
