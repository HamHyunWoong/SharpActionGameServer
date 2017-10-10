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
    class DB_Connecton
    {
        MySqlConnection myConnection;

        //생성자
        //コンストラクタ
        public DB_Connecton(string DB_name, string admin_ID, string password)
        {
            try
            {
                Console.WriteLine("데이터베이스 오픈시도");

                using (myConnection = new MySqlConnection("Server=localhost;Database=" + DB_name + ";Uid=" + admin_ID + ";Pwd=" + password + ";")) {
                    //데이터베이스 연결설정 ->GameDB
                    //データベース連結テスト
                    myConnection.Open();
                    Console.WriteLine("데이터베이스 오픈성공");
                    myConnection.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error\n 프로그램을 종료하고 처음부터 다시 설정해 주세요 : " + e.Message);
            }

        }
        //새로 가입하기
        //新プレイヤー登録処理
        public string AddplayerDB(string playerInfo)
        {
                string reply = "NEWPLAYER$가입실패";
           
           

                string[] msgArr = playerInfo.Split(new char[] { '#' });
                string str = "A";
                try
                {
                myConnection.Open();
                // 가입하려는 아이디 값으로 검색 
                //アカウント検索
                MySqlCommand cmd = new MySqlCommand("SELECT player_ID FROM playerstat WHERE player_ID='" + msgArr[0] + "'", myConnection);
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        str += rdr[0].ToString();
                    }

                    Console.WriteLine("val : "+str);

                myConnection.Close();
                //등록성공처리
                //登録成功処理
                if (str == "A")
                {

                    //데이터베이스에등록
                    //データベースに情報登録
                    myConnection.Open();
                    MySqlCommand cmd2 = new MySqlCommand("INSERT INTO playerstat (player_ID , player_PASS, Player_X , Player_Y , Player_ANI , email) VALUES ('" + msgArr[0] + "', '" + msgArr[1] + "' , -30 ,0 ,'IDLE' ,'"+msgArr[2]+"' )", myConnection);
                    cmd2.ExecuteNonQuery();

                    
                    reply = "NEWPLAYER$가입성공";
                    myConnection.Close();
                    return reply;

                }
                else {

                    Console.WriteLine("가입실패처리 ");
                    //등록실패처리
                    //登録失敗処理
                    reply = "NEWPLAYER$가입실패";
                }

                }

            catch (Exception e)
            {
                Console.WriteLine("error : "+e.Message);

                reply = "NEWPLAYER$가입실패";
                    myConnection.Close();
                    return reply;
            }
            myConnection.Close();
            return reply;
        }
        //로그인하기
        //ログインする
        public string login_playerDB(string playerInfo,Socket socket)
        {
            string login_reply = "LOGIN$로그인실패";
                string[] msgArr = playerInfo.Split(new char[] { '#' });
                string str = "A";
                try
                {
                    Console.WriteLine("message : " + playerInfo);
                    myConnection.Open();
                    // 로그인하려는 아이디 값으로 검색 
                    //ログインアカウントで検索
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM playerstat WHERE player_ID='" + msgArr[0] + "'", myConnection);
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    string[] dataset = new string[5];

                    while (rdr.Read())
                    {

                        dataset[0] = (string)rdr[0];
                        dataset[1] = (string)rdr[1]; //password
                        dataset[2] = rdr[2].ToString();
                        dataset[3] = rdr[3].ToString();


                    }
                    rdr.Close();
                    myConnection.Close();
                    Console.WriteLine("사용자 PASS : " + dataset[1]);
                //패스워드 처리
                //パスワード処理
                if (msgArr[1] == dataset[1])
                {
                    //client split: 0 = Login , 1 = ID , 2 =Password , 3 = X, 4=Y
                    //client split: 0 = 로그인성공 , 1 = 아이디 , 2  = 패스워드 ,3 = X,  4 =Y
                    login_reply = "LOGIN$로그인성공#" + dataset[0] + "#" + dataset[1] + "#" + dataset[2] + "#" + dataset[3];
                    //Login Succ
                    Console.WriteLine("로그인 성공 : " + login_reply);
                    Console.WriteLine("클라이언트 리스트 : " + Program.clientList.Count);

                    Program.TcpMultiCast("TALK$<"+dataset[0]+" 님이 로그인 하셨습니다.  >");

                    for (int i = 0; i < Program.clientList.Count; i++)
                    {
                        Program.Client_Data cData = Program.clientList.ToArray<Program.Client_Data>()[i];
                        //소켓이 같을경우
                        //Socketが同じ場合
                        if (cData.socket == socket)
                        {
                            Console.WriteLine("갱신 호출됨");
                            cData.ID_name = dataset[0];
                            cData.stat.x = float.Parse(dataset[2]);
                            cData.stat.y = float.Parse(dataset[3]);


                            //해당 데이터를 삭제
                            //該当データを削除
                            Program.clientList.RemoveAt(i);
                            //새 데이터로 추가
                            //新しいデータを追加
                            Program.clientList.Insert(i, cData);
                            Console.WriteLine("로그인 데이터 리스트에 추가됨 id =" + Program.clientList.ToArray<Program.Client_Data>()[i].ID_name);
                            Program.loginCount++;
                        }
                        else {

                            Console.WriteLine("해당 데이터가 아님");
                        }



                    }



                }
                else {


                    Console.WriteLine("로그인 실패");

                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error : " + e.Message);
            }
            return login_reply;

        }
        //로그아웃하기 
        //ログアウト
        public string logout_playerDB(string playerInfo)
        {
            string reply = "LOGINOUT$로그아웃실패";
            try
            {

                string[] msgArr = playerInfo.Split(new char[] { '#' });

                //해당 플레이어 ID로부터 튜플을 찾음
                //該当プレイヤーのIDから情報をロード
                string statmsg = "LOGOUT$로그아웃성공#"+msgArr[0];

                Program.TcpMultiCast("TALK$<" + msgArr[0] + " 님이 로그아웃 하셨습니다.  >");


                //데이터베이스에 플레이어 마지막 위치 정보 저장
                //プレイヤーの最後の座標をセーブ
                myConnection.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE playerstat SET Player_X ="+msgArr[1]+" WHERE player_ID= '"+msgArr[0]+"'", myConnection);
                cmd.ExecuteNonQuery();
                myConnection.Close();

                myConnection.Open();
                MySqlCommand cmd2 = new MySqlCommand("UPDATE playerstat SET Player_Y =" + msgArr[2] + " WHERE player_ID='" + msgArr[0] + "'", myConnection);
                cmd2.ExecuteNonQuery();
                myConnection.Close();
                Program.loginCount--;
                return statmsg;

  
            }
            catch (Exception e)
            {
                return reply;
            }
            
        }
        //데이터베이스에서 몬스터 정보 불러오기
        //データベースからモンスター情報をロード
        public void getMonsterDB()
        {
            try
            {

                //Sample Monster
                string[] mon_names = {"Mon1","Mon2" };


                for (int i =0; i<mon_names.Length;i++) {
                   
                    myConnection.Open();
                    Program.Monster_Data mon = new Program.Monster_Data();


                    string sql = "SELECT * FROM monsterstat WHERE Monster_ID = '"+mon_names[i]+"'";
                    Console.WriteLine(sql);
                    MySqlCommand cmd = new MySqlCommand(sql, myConnection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    //データをロード
                    while (rdr.Read())
                    {

                        mon.ID_name = (string)rdr[0];
                        mon.HP = (int)rdr[1];
                        mon.stat.x = (float)rdr[2];
                        mon.stat.y = (float)rdr[3];
                        mon.stat.anime = (string)rdr[4];
                        mon.angle = (float)rdr[5];
                        mon.target_name = "";               
                    }
                    
                    rdr.Close();
                    myConnection.Close();
                    Console.WriteLine(mon.ID_name + " , " + mon.HP + " , " + mon.stat.x + " , " + mon.stat.y + " , " + mon.stat.anime + " , " + mon.angle);

                    //리스트에 추가
                    //リストに追加
                    Program.monsterList.Add(mon);

                    }

                Console.WriteLine("MonList Count :  "+ Program.monsterList.Count);

            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error : " + e.Message);
            }
           
        }

    }
}
