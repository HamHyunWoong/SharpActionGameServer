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

//실시간 액션게임의 서버를 구현하고자 하였습니다. (일괄처리서버)
//그렇기에 0.1초 단위로 모든 플레이어와 몬스터의 좌표를 공유하고 교환합니다. 
//이번 프로젝트에서는 접속자 한명당 스레드 하나를 할당하여 실시간 처리가 가능합니다. 
//I/O 처리는 비동기테스크를 지원하는 함수를 사용하여 반응성이 좋은 서버를 만들어보고자 하였습니다.           
//또한 로그인서버 채팅서버를 게임서버에 합쳐서 한번에 구현되어 있습니다.
//몬스터는 서버에서 제어하는 구조로, 몬스터의 위치정보를 서버에서 생성하고 관리합니다. ->MonNavi.cs 
//リアルタイムアクションゲームのサーバーを実装しようとしました。(一括処理サーバ)
//そのため、0.1秒単位ですべてのプレイヤーとモンスターの座標を共有して交換します。
//今回のプロジェクトでは、接続者一人につき1つのスレッドを割り当てて、リアルタイム処理が可能です。
// I/ O処理は非同期タスクをサポートする関数を使用して反応性が良いサーバーを作ってみようとしました。
//また、ログインサーバーのチャットサーバーをゲームサーバに合わせて一度に実装されています。
//モンスターは、サーバーで制御する構造で、モンスターの位置情報をサーバで生成して管理します。 - > MonNavi.cs

namespace SharpGameServer
{
    class Program
    {

        public static Socket server;
        private static int port;
        private static string DB_ADMIN_ID;
        private static string DB_ADMIN_PASS;

        public static int loginCount;

        private static IPEndPoint ipEndPoint;

        //Thread -> Lock 
        public static List<Client_Data> clientList = new List<Client_Data>();
        public static List<Monster_Data> monsterList = new List<Monster_Data>();

        static string readMassage;
/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
///     //클라이언트와 몬스터의 공통스테이터스
        //クライアントとモンスターの共通のステータス
        public struct Status {
            public float x; //0
            public float y; //1
            public string anime; //2
            public float angle;
        }
        //클라이언트 정보
        //クライアント情報
        public struct Client_Data { 

            public Socket socket;
            public string ID_name;
            public string password;
            public Status stat;
  
        }
        //몬스터 정보 
        //モンスターの情報
        public struct Monster_Data
        {
            
            public string ID_name;
            public string target_name;
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
            loginCount = 0;
            Console.WriteLine("Server IP Address : " + Get_MyIP());
            Console.WriteLine("************************************************************");
            Console.Write("Port Number : ");

            port = int.Parse(Console.ReadLine());
            Console.WriteLine("My port is " + port);

            //서버 소켓 생성
            //サーバーのSocket
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //IP , Port 
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            server.Bind(ipEndPoint);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //데이터베이스 연결설정 ADMIN ID, PASS를 입력받음 
            //データベースの連動設定
            string db = "gamebase";
            Console.WriteLine("************************************************************");
            Console.Write("Database admin ID : ");
            DB_ADMIN_ID = Console.ReadLine();
            Console.WriteLine("************************************************************");

            Console.Write("Database admin Pass : ");
            DB_ADMIN_PASS = Console.ReadLine();
            Console.WriteLine("************************************************************");


            //데이터베이스 로그인 
            //データベースログイン
            DB_Connecton database = new DB_Connecton(db, DB_ADMIN_ID, DB_ADMIN_PASS);
            
            

            //몬스터리스트 추가 및 데이터베이스에서 몬스터정보 로드 
            //モンスターのリスト追加およびデータベースからのモンスター情報をロード
            database.getMonsterDB();

            //필드 몬스터 생성 및 처리 (접속 클라이언트가 없을경우 전송하지는 않는다. )
            //モンスターの召喚および処理(接続したクライアントが存在する場合のみ)
            MonsterService();



            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            server.Listen(4); //동시 접속자수 제한, 서버오픈
            

            Console.WriteLine("접속자 대기중...");


            while (true)
            {
                //클라이언트를 받아옴
                //クライアント接続
                Socket client = server.Accept();

                //Client IP 
                string address = client.RemoteEndPoint.ToString();

                // addressArray[0] = IP, addressArray[1] = Port
                string[] addressArray = address.Split(new char[] { ':' });

                //Client Address 
                Console.WriteLine("클라이언트 접속 성공! \n 클라이언트 정보 " + address);

                //Stream 
                NetworkStream networkStream = new NetworkStream(client);
                StreamReader streamReader = new StreamReader(networkStream);

                //클라이언트 데이터 구조체를 생성
                //クライアントデータの構造体を生成
                Client_Data c_data = new Client_Data();
                c_data.socket = client;
                

                //클라이언트 데이터 리스트를 업데이트함
                //クライアントデータをアップデート
                clientList.Add(c_data);

                //접속자 표시
                //接続者　表示
                Console.WriteLine("새로운 클라이언트 연결됨");


                //스레드 처리 
                //スレッド処理
                Thread serviceThread = new Thread(delegate ()
                { 
                    try
                    {
                        //게임 서비스 시작 
                        //ゲームサービス開始
                        Run(c_data, networkStream, streamReader,database);
                    }
                    catch (Exception e)
                    {
                        lock (clientList)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                            
                                Client_Data sk = clientList.ToArray<Client_Data>()[i];

                                if (sk.socket == client)
                                {
                                    Console.WriteLine("접속해제 클라이언트 정보 : " + address);
                                    lock (monsterList)
                                    {
                                        for (int j = 0; j < monsterList.Count; j++)
                                        {
                                        

                                            Monster_Data monster = monsterList.ToArray<Monster_Data>()[j];

                                            if (monster.target_name == sk.ID_name)
                                            {
                                                monster.target_name = "";
                                                monster.stat.anime = "IDLE";

                                            }
                                        }

                                    }

                                    //클라이언트 리스트에서 제거 
                                    clientList.RemoveAt(i);
                                    client.Close();



                                }

                                    //전체멀티케스트 
                                    //全員に送信
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
        //サービススレッド
        static void Run(Client_Data clientData, NetworkStream networkStream, StreamReader streamReader, DB_Connecton database)
        {
            Console.WriteLine("서비스스레드 시작");
            while (true)
            {
                Thread.Sleep(100);

                if (!clientData.socket.Connected)
                {
                    int i = 0;
                    lock (clientList)
                    {
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
                }
                else
                {

                    while ((readMassage = streamReader.ReadLine()) != null)
                    {
                        //Read Massage from Client
                        // Console.WriteLine("수신된 메시지 : " + readMassage);

                        string[] msgArr = readMassage.Split(new char[] { '$' }); //$로 구분 

                        // msgArr[0] = "NEWPLAYER" 
                        // msgArr[0] = "LOGIN" 
                        // msgArr[0] = "LOGOUT" 
                        // msgArr[0] = "TALK" 
                        // msgArr[0] = "MONSTER" 
                        // msgArr[0] = "PLAYER" 
                        switch (msgArr[0]) {
                            case "NEWPLAYER":
                                Console.WriteLine("플레이어 가입시도");
                                string msg = database.AddplayerDB(msgArr[1]);
                                //응답 메시지 전송
                                //応答メッセージ送信
                                SendMessage(clientData.socket, msg);

                                break;

                            case "LOGIN":
                                Console.WriteLine("플레이어 로그인 시도");
                                string msg2 = database.login_playerDB(msgArr[1], clientData.socket);
                                //응답 메시지 전송
                                //応答メッセージ送信
                                SendMessage(clientData.socket, msg2);
                                break;

                            case "LOGOUT":
                                Console.WriteLine("플레이어 로그아웃 요청");
                                string msg3 = database.logout_playerDB(msgArr[1]);
                                TcpMultiCast(msg3);
                                break;

                            case "TALK":
                                Console.WriteLine("채팅요청");
                                TcpMultiCast(readMassage);
                                break;


                            case "MONSTER":
                                string[] monArr = msgArr[1].Split(new char[] { '#' });
                                Console.WriteLine("몬스터 스테이터스 수신"); //이름,HP만 
                                lock (monsterList)
                                {
                                    for (int i = 0; i < monsterList.Count; i++)
                                    {
                                        Monster_Data mon = monsterList.ToArray<Monster_Data>()[i];


                                        //이름이 같을경우
                                        if (mon.ID_name == monArr[0])
                                        {
                                            mon.HP = int.Parse(monArr[1]);
                                            //해당 데이터를 삭제
                                            monsterList.RemoveAt(i);
                                            //새 데이터로 추가
                                            monsterList.Insert(i, mon);

                                            //이름이 같을경우
                                            //名前が同じ場合
                                            if (mon.ID_name == monArr[0])
                                            {
                                                mon.HP = int.Parse(monArr[1]);
                                                //해당 데이터를 삭제
                                                //該当データを削除
                                                monsterList.RemoveAt(i);
                                                //새 데이터로 추가
                                                //新しいデータを追加
                                                monsterList.Insert(i, mon);


                                            }


                                        }
                                    }
                                }
                                break;

                            case "PLAYER":

                                // 0 = x , 1 = y , 2 =anime , 3 =name_id 
                                string[] statArr = msgArr[1].Split(new char[] { '#' });


                                //좌표를 일시적으로 저장한 이유는 몬스터의 네비게이션 알고리즘에 활용하기 위해 -> 리얼타임 처리 

                                lock (clientList) {
                                    for (int i = 0; i < clientList.Count; i++)
                                    {
                                        Client_Data cData = clientList.ToArray<Client_Data>()[i];

                                        //이름이 같을경우
                                        if (cData.ID_name == statArr[0])
                                        {
                                            cData.ID_name = statArr[0];
                                            cData.stat.x = float.Parse(statArr[1]);
                                            cData.stat.y = float.Parse(statArr[2]);
                                            cData.stat.anime = statArr[3];
                                            cData.stat.angle = float.Parse(statArr[4]);

                                            //해당 데이터를 삭제
                                            clientList.RemoveAt(i);
                                            //새 데이터로 추가
                                            clientList.Insert(i, cData);

                                        }

                                    }
                                        //전 클라이언트에 해당 플레이어의 위치를 알림 
                                        //全てのクライアントに該当プレイヤーの座標を知らせる。
                                        TcpMultiCast(readMassage);
                                        break;

                                   

                                }

}

                    }


                }
            } }
        void MonsterService() {

            Thread serviceThread = new Thread(delegate ()
            {
                while (true)
                {

                    string mon_msg = "MONSTER$";
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        lock (monsterList)
                        {
                            Monster_Data monster = monsterList.ToArray<Monster_Data>()[i];

                            //플레이어가 2명 이상 모일경우 게임시작
                            //プレイヤーが二人以上、集まった場合ゲーム開始
                            if (loginCount>=2) {
                                //monster 위치를 갱신 
                                //monster　座標を更新
                                MonNavi.start(monster, i);

                                if (monster.HP <= 0)
                                {
                                    monsterList.RemoveAt(i);

                                }
                            }

                        }
                    }


                    //0.2초마다 슬립 시킴 
                    //0.2秒ごとにスリップ
                    Thread.Sleep(200);
                }
           } );
            //Start Thread
            serviceThread.Start();

        }

        //한 클라이언트에게 송신하기
        //一つのクライアントへ送信
        static void SendMessage(Socket sk,string message) {
            try
            {
                 NetworkStream networkStream = new NetworkStream(sk);
                 StreamWriter streamWriter = new StreamWriter(networkStream);

                 //비동기 I/O 전송처리(AsyncTask) 
                 //非同期I/O送信処理
                 streamWriter.WriteLineAsync(message);
                 //버퍼를 비동기적으로 지움(AsyncTask)
                　//bufferを非同期的に消す。
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
        //AsyncTaskを利用したメッセージ送信
        //非同期送信処理ー＞MultiCast
        public static void TcpMultiCast(string message)
        {
            lock (clientList)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    Client_Data sk = clientList.ToArray<Client_Data>()[i];

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
                            Console.WriteLine("error : " + e.Message);
                        }


                    }
                }
            }
        }
    }






}
