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
        public DB_Connecton(string DB_name, string admin_ID, string password)
        {
            try
            {
                Console.WriteLine("데이터베이스 오픈시도");

                using (myConnection = new MySqlConnection("Server=localhost;Database=" + DB_name + ";Uid=" + admin_ID + ";Pwd=" + password + ";")) {
                    //데이터베이스 연결설정 ->GameDB
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
        public string AddplayerDB(string playerInfo)
        {
                string reply = "NEWPLAYER$가입실패";
           
           

                string[] msgArr = playerInfo.Split(new char[] { '#' });
                string str = "A";
                try
                {
                myConnection.Open();
                // 가입하려는 아이디 값으로 검색 
                MySqlCommand cmd = new MySqlCommand("SELECT player_ID FROM playerstat WHERE player_ID='" + msgArr[0] + "'", myConnection);
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        str += rdr[0].ToString();
                    }

                    Console.WriteLine("val : "+str);

                myConnection.Close();

                if (str == "A")
                {
                    Console.WriteLine("called");
                    //넣기
                    myConnection.Open();
                    MySqlCommand cmd2 = new MySqlCommand("INSERT INTO playerstat (player_ID , player_PASS, Player_X , Player_Y , Player_ANI) VALUES ('" + msgArr[0] + "', '" + msgArr[1] + "' , 50 ,50 ,'IDLE')", myConnection);
                    cmd2.ExecuteNonQuery();


                    reply = "NEWPLAYER$가입성공";
                    myConnection.Close();
                    return reply;

                }
                else {

                    Console.WriteLine("가입실패처리 ");
                    //발견되었을 경우 가입실패라 알림 
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
        public string login_playerDB(string playerInfo)
        {
            string login_reply = "LOGIN$로그인실패";
            try
            {
                //
                string[] msgArr = playerInfo.Split(new char[] { '#' });

                // DB 열기
                myConnection.Open();
                try {
                        //해당 플레이어 ID로부터 튜플을 찾음
             
                        string sql = "SELECT * FROM playerstat WHERE player_ID = '" +msgArr[0] + "'";


                        Console.WriteLine(sql);
                        MySqlCommand cmd = new MySqlCommand(sql, myConnection);
                        MySqlDataReader rdr = cmd.ExecuteReader();

                    string[] dataset = new string[5];

                    while (rdr.Read())
                    {

                        dataset[0] = (string)rdr[0];
                        dataset[1] = (string)rdr[1]; //password
                        dataset[2] = ((float)rdr[2]).ToString();
                        dataset[3] = ((float)rdr[3]).ToString();
                        dataset[4] = (string)rdr[4];

                    }

                    
                    //패스워드 비교
                    if (msgArr[1] == dataset[1]) {


                        login_reply = "LOGIN$로그인성공#"+ dataset[0]+"#"+ dataset[1]+"#" + dataset[2] + "#" + dataset[3] + "#" + dataset[4] + "";
                        return login_reply;
                    }


                    } catch (Exception e) {

                        //로그인 실패
                        return login_reply;
                    }
                

            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error\n 프로그램을 종료하고 처음부터 다시 설정해 주세요 : " + e.Message);
            }
            myConnection.Close();
            return login_reply;

        }
        //로그아웃하기 
        public string logout_playerDB(string playerInfo)
        {
            string reply = "LOGINOUT$로그아웃실패";
            try
            {

                string[] msgArr = playerInfo.Split(new char[] { '#' });

                // DB 열기
                myConnection.Open();


                    //해당 플레이어 ID로부터 튜플을 찾음
                    string statmsg = "LOGOUT$로그아웃성공";


                    //데이터베이스에 플레이어 위치 정보 저장
                    MySqlCommand cmd = new MySqlCommand("UPDATE playerstat SET Player_X ="+msgArr[2]+" WHERE player_ID="+msgArr[0], myConnection);
                    cmd.ExecuteNonQuery();
                    MySqlCommand cmd2 = new MySqlCommand("UPDATE playerstat SET Player_Y =" + msgArr[3] + " WHERE player_ID=" + msgArr[0], myConnection);
                    cmd2.ExecuteNonQuery();

                    return statmsg;

  
            }
            catch (Exception e)
            {
                return reply;
            }
            myConnection.Close();
        }
        //몬스터 정보 불러오기
        public void getMonsterDB()
        {
            try
            {
                
                // DB 열기
                myConnection.Open();
                Console.WriteLine("************************************************************");
                Console.WriteLine("*********************데이터베이스에 접속완료****************");
                Console.WriteLine("************************************************************");

                //일단 몇개 샘플을 넣었습니다. 
                string[] mon_names = {"Mon1","Mon2" };


                for (int i =0; i<mon_names.Length;i++) {
                    Program.Monster_Data mon = new Program.Monster_Data();


                    string sql = "SELECT * FROM monsterstat WHERE Monster_ID = '"+mon_names[i]+"'";
                    Console.WriteLine(sql);
                    MySqlCommand cmd = new MySqlCommand(sql, myConnection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    
                    while (rdr.Read())
                    {

                        mon.ID_name = (string)rdr[0];
                        mon.HP = (int)rdr[1];
                        mon.stat.x = (float)rdr[2];
                        mon.stat.y = (float)rdr[3];
                        mon.stat.anime = (string)rdr[4];
                        mon.angle = (float)rdr[5];
               

                    }
                    
                    rdr.Close();

                    Console.WriteLine(mon.ID_name + " , " + mon.HP + " , " + mon.stat.x + " , " + mon.stat.y + " , " + mon.stat.anime + " , " + mon.angle);

                    //리스트에 추가
                    Program.monsterList.Add(mon);

                    }

                Console.WriteLine("MonList Count :  "+ Program.monsterList.Count);

            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error : " + e.Message);
            }
            myConnection.Close();
        }

    }
}