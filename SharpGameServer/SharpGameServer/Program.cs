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
using MySql.Data.MySqlClient;

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
        public static List<Client_Data> clientList = new List<Client_Data>();
        public static List<Monster_Data> monsterList = new List<Monster_Data>();

        static string readMassage;
/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
        public struct Status {
            public float x; //0
            public float y; //1
            public string anime; //2

        }
        public struct Client_Data { 

            public Socket socket;
            public string ID_name;
            public string password;
            public Status stat;
  
        }
        public struct Monster_Data
        {
            
            public string ID_name;
            public int HP;
            public float angle;
            public Status stat;


        }

/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>

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

            //데이터베이스 연결설정 ADMIN ID, PASS를 입력받음 
            string db = "gamebase";
            Console.WriteLine("************************************************************");
            Console.Write("Database admin ID : ");
            DB_ADMIN_ID = Console.ReadLine();
            Console.WriteLine("************************************************************");

            Console.Write("Database admin Pass : ");
            DB_ADMIN_PASS = Console.ReadLine();
            Console.WriteLine("************************************************************");


            //데이터베이스 로그인 
            DB_Connecton database = new DB_Connecton(db, DB_ADMIN_ID, DB_ADMIN_PASS);
            
            

            //몬스터리스트 추가 및 데이터베이스에서 몬스터정보 로드 
            database.getMonsterDB();

            //필드 몬스터 생성 및 처리 (접속 클라이언트가 없을경우 전송하지는 않는다. )
            MonsterService();
           


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            //이번프로젝트에서는 접속자 한명당 스레드 하나를 할당하여 실시간 처리를 증가시키기로 하였습니다.
            //룸 방식(1~16)의 멀티플레이 게임에 적합합니다. 
            //만일 동접자수는 늘리되 반응성을 유지시키는 방향이라면 각 게임방들을 컨트롤하는 서버와 로그인서버 채팅서버등이 별도로 필요합니다.
            
            //동접자수를 늘리되 반응성은 약간 포기하는 방향이라면 유져당 스레드를 할당하지 않고 메세지에 유져의 ID값을 넣고 그 정보를 기반으로 분류해서 서비스.
            //다만 이렇게 만든다면 클라이언트에서 마우스 클릭 이동방식을 채택해야 합니다.
            
            //다만 이번 프로젝트에서는 기획상 키보드 이동방식이기도 하고, 현실적 여건상 로그인서버 채팅서버를 게임서버에 합쳐서 한번에 구현해보기로 하였습니다.
            //그렇기에 이번 프로젝트에는 로그인기능 채팅기능 게임정보 처리기능 등이 모두 구현되어 있습니다. 
             
            server.Listen(8); //동시 접속자수 제한, 서버오픈
            

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

                //접속자 표시
                Console.WriteLine("새로운 클라이언트 연결됨");


                //스레드 처리 
                Thread serviceThread = new Thread(delegate ()
                { 
                    try
                    {
                        //게임 서비스 시작 
                        Run(c_data, networkStream, streamReader,database);
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
        static void Run(Client_Data clientData, NetworkStream networkStream, StreamReader streamReader, DB_Connecton database)
        {
            Console.WriteLine("서비스스레드 시작");
            while (true)
            {
                Thread.Sleep(100);
                if (!clientData.socket.Connected)
                {
                    int i = 0;
                    foreach (Client_Data sk in clientList)
                    {

                        if (sk.socket == clientData.socket)
                        {

                            clientList.RemoveAt(i);
                            clientData.socket.Close();

                            break;

                        }
                        i++;
                    }

                }
                else
                {

                    while ((readMassage = streamReader.ReadLine() ) != null)
                    {
                        //Read Massage from Client
                        Console.WriteLine("수신된 메시지 : " + readMassage);

                        string[] msgArr = readMassage.Split(new char[] {'$'}); //$로 구분 

                        // msgArr[0] = "NEWPLAYER" 이면 가입 처리 ->데이터베이스로 저장 
                        // msgArr[0] = "LOGIN" 이면 로그인 및 플레이어 데이터 송신
                        // msgArr[0] = "LOGOUT" 이면 로그아웃 및 플레이어 데이터 수신 
                        // msgArr[0] = "TALK" 이면 채팅모드
                        // msgArr[0] = "MONSTER" 이면 보스정보
                        // msgArr[0] = "PLAYER" 이면 플레이어

                        switch (msgArr[0]) {
                            case "NEWPLAYER":
                                Console.WriteLine("플레이어 가입시도");
                                string msg =database.AddplayerDB(msgArr[1]);
                                //응답 메시지 전송
                                SendMessage(clientData.socket, msg);
                                
                                break;

                            case "LOGIN":
                                Console.WriteLine("플레이어 로그인 시도");
                                string msg2 = database.login_playerDB(msgArr[1]);
                                //응답 메시지 전송
                                SendMessage(clientData.socket, msg2);
                                break;

                            case "LOGOUT":
                                Console.WriteLine("플레이어 로그아웃 요청");
                                string msg3 = database.logout_playerDB(msgArr[1]);
                                break;

                            case "TALK":
                                Console.WriteLine("채팅요청");
                                TcpMultiCast(readMassage);
                                break;


                            case "MONSTER":
                                Console.WriteLine("몬스터 스테이터스 수신"); //이름,HP만 
                                for(int i =0; i< monsterList.Count;i++)
                                {
                                    Monster_Data mon = monsterList.ToArray<Monster_Data>()[i];

                                    //이름이 같을경우
                                    if (mon.ID_name == msgArr[1]) {
                                        mon.HP = int.Parse(msgArr[2]);
                                        //해당 데이터를 삭제
                                        monsterList.RemoveAt(i);
                                        //새 데이터로 추가
                                        monsterList.Insert(i,mon);

                                    }
                                    


                                }
                                break;

                            case "PLAYER":
                                Console.WriteLine("플레이어 스테이터스 수신");

                                // #을 기준으로 한번 더 자름 
                                // 0 = x , 1 = y , 2 =anime , 3 =name_id -> 다만 id는 이미 플레이어 로그인 시점에서 다뤘기에 구조체에는 포함하지 않고 메세지에만...
                                string[] statArr = msgArr[1].Split(new char[] { '#' });

                                //좌표를 일시적으로 저장한 이유는 몬스터의 네비게이션 알고리즘에 활용하기 위해 -> 리얼타임 처리 
                                clientData.stat.x = float.Parse(statArr[0]);
                                clientData.stat.y = float.Parse(statArr[1]);
                                clientData.stat.anime = statArr[2];

                                //전 클라이언트에 해당 플레이어의 위치를 알림 
                                TcpMultiCast(readMassage);


                                break;


                        }



                        //BroadCast Server to Client
                        //TcpMultiCast(readMassage);
                    }


                }

            }

            //End Service Thread
            Console.WriteLine("서비스스레드 종료됨");

        }

        void MonsterService() {

            Thread serviceThread = new Thread(delegate ()
            {
                while (true)
                {

                    string mon_msg = "MONSTER$";
                    for (int i = 0; i < monsterList.Count; i++)
                    {

                        Monster_Data monster = monsterList.ToArray<Monster_Data>()[i];

                        //monster 위치를 갱신 
                        MonNavi.start(monster,i);
                        
                    }


                    //0.1초마다 슬립 시킴 
                    Thread.Sleep(100);
                }
            });
            //Start Thread
            serviceThread.Start();

        }


        static void SendMessage(Socket sk,string message) {
            try
            {
                 NetworkStream networkStream = new NetworkStream(sk);
                 StreamWriter streamWriter = new StreamWriter(networkStream);

                 //비동기 I/O 전송처리(AsyncTask) 
                 streamWriter.WriteLineAsync(message);
                 //버퍼를 비동기적으로 지움(AsyncTask)
                 streamWriter.FlushAsync();


                 networkStream = null;
                 streamWriter = null;
            }
            catch (Exception e)
            {

                Console.WriteLine("error : " + e.Message);

            }

        }


        //AsyncTask 이용한 메세지 전송
        //비동기 전송처리 -> 멀티케스팅 
        public static void TcpMultiCast(string message)
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
                        //Console.WriteLine(message);

                        //비동기 I/O 전송처리(AsyncTask) 
                        streamWriter.WriteLineAsync(message);
                        //버퍼를 비동기적으로 지움(AsyncTask)
                        streamWriter.FlushAsync();


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
