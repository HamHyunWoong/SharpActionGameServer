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
        private static string DB_table;
        private static string DB_URL;
        private static string DB_ADMIN_ID;
        private static string DB_ADMIN_PASS;



        private static IPEndPoint ipEndPoint;
        private static List<Client_Data> clientList = new List<Client_Data>();

        static string readMassage;


        public struct Client_Data { 

            public Socket socket;

            public string ID_name;
            public string password;

            public int [] inventory;
  
        }




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

            //서버 소켓 생성
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //IP , Port 설정
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            server.Bind(ipEndPoint);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //데이터베이스 연결설정 URL ,PORT ,ADMIN ID, PASS를 입력받음 



            Console.Write("Table Name : ");
            DB_table = Console.ReadLine();
            Console.WriteLine("************************************************************");
            Console.Write("Database admin ID : ");
            DB_ADMIN_ID = Console.ReadLine();
            Console.WriteLine("************************************************************");

            Console.Write("Database admin Pass : ");
            DB_ADMIN_PASS = Console.ReadLine();
            Console.WriteLine("************************************************************");


            //데이터베이스 로그인 
            DB_Connecton database = new DB_Connecton();
            
            database.LoginDB(DB_table, DB_ADMIN_ID, DB_ADMIN_PASS);
         

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            server.Listen(1000); //접속자수 제한, 서버오픈

            Console.WriteLine("접속자 대기중...");


            while (true)
            {
                //클라이언트를 받아옴
                Socket client = server.Accept();

                //Client IP 주소
                string address = client.RemoteEndPoint.ToString();

                // addressArray[0] = IP, addressArray[1] = Port
                string[] addressArray = address.Split(new char[] { ':' });

                //연결된 Client Address 물리주소를 표시
                Console.WriteLine("클라이언트 접속 성공! \n 클라이언트 정보 " + address);

                //Stream 생성 
                NetworkStream networkStream = new NetworkStream(client);
                StreamReader streamReader = new StreamReader(networkStream);

                //클라이언트 데이터 구조체를 생성
                Client_Data c_data = new Client_Data();
                c_data.socket = client;
                

                //클라이언트 데이터 리스트를 업데이트함
                clientList.Add(c_data);

                //접속자 알림 (전체멀티케스트)
                TcpMultiCast("<<[" + address + "] 님이 게임에 접속하셨습니다. >>");

                //Create Service Thread
                Thread serviceThread = new Thread(delegate ()
                {
                    try
                    {
                        //게임 서비스 시작 
                        Run(client, networkStream, streamReader);
                    }
                    catch (Exception e)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            Client_Data sk = clientList.ToArray<Client_Data>()[i];
                           
                            if (sk.socket == client)
                            {
                                Console.WriteLine("접속해제 클라이언트 정보 : " + address);

                                //클라이언트 리스트에서 제거 
                                clientList.RemoveAt(i);
                                client.Close();

                                //전체멀티케스트 
                                TcpMultiCast("<<[" + address + "] 님이 채팅방에서 나가셨습니다.>>");
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


        //서비스 스레드 
        static void Run(Socket client, NetworkStream networkStream, StreamReader streamReader)
        {
            Console.WriteLine("서비스스레드 시작");
            while (true)
            {
                Thread.Sleep(10);
                if (!client.Connected)
                {
                    int i = 0;
                    foreach (Client_Data sk in clientList)
                    {

                        if (sk.socket == client)
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

                        // msgArr[0] = "NEWPLAYER" 이면 가입 처리 ->데이터베이스로 저장 
                        // msgArr[0] = "LOGIN" 이면 로그인 및 플레이어 데이터 송신
                        // msgArr[0] = "LOGOUT" 이면 로그아웃 및 플레이어 데이터 수신 
                        // msgArr[0] = "TALK" 이면 채팅모드
                        // msgArr[0] = "ITEM" 이면 플레이어 소지 아이템정보 수신
                        // msgArr[0] = "MONSTER" 이면 보스정보
                        // msgArr[0] = "PLAYER" 이면 플레이어












                        //BroadCast Server to Client
                        TcpMultiCast(readMassage);
                    }


                }

            }

            //End Service Thread
            Console.WriteLine("서비스스레드 종료됨");

        }

        static void TcpMultiCast(string message)
        {


            foreach (Client_Data sk in clientList)
            {
                if (sk.socket != null)
                {
                    //Send
                    try
                    {
                        NetworkStream networkStream = new NetworkStream(sk.socket);
                        StreamWriter streamWriter = new StreamWriter(networkStream);

                        Console.WriteLine(message);
                        streamWriter.WriteLine(message);
                        streamWriter.Flush();


                        networkStream = null;
                        streamWriter = null;
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine("error : "+e.Message);

                    }


                }
            }
        }
    }






}
