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
using System.Data.SqlClient;

namespace SharpGameServer
{
    class DB_Connecton
    {
        SqlConnection myConnection;

        public void LoginDB(string table, string admin_ID, string password)
        {
            try { 
                    //데이터베이스 연결설정 ->GameDB
                    using (myConnection = new SqlConnection("Server=localhost;Database="+table+";Uid="+admin_ID+";Pwd="+password+";"))
                    {
                    // DB 열기
                        myConnection.Open();
                        Console.WriteLine("************************************************************");
                        Console.WriteLine("*********************데이터베이스에 접속완료****************");
                        Console.WriteLine("************************************************************");


                    }
                }
            catch (Exception e){
                Console.WriteLine(" DB error\n 프로그램을 종료하고 처음부터 다시 설정해 주세요 : "+e.Message);
            }

        }

        public string AddplayerDB(string table, string admin_ID, string password ,string playerInfo)
        {
            string reply = "NEWPLAYER$가입실패";
            try
            {
                //데이터베이스 연결설정 ->GameDB
                using (myConnection = new SqlConnection("Server=localhost;Database=" + table + ";Uid=" + admin_ID + ";Pwd=" + password + ";"))
                {


                    string[] msgArr = playerInfo.Split(new char[] { '#' });

                    // DB 열기
                    myConnection.Open();

                    //플레이어 ID 목록을 데이터베이스에서 받아옴

                    string[] playerList = null;

                    //플레이어 ID 목록과 가입하려는 ID정보를 대조 
                    bool usedID =false;

                    for (int i=0;i<playerList.Length;i++) {

                        if (msgArr[0] == playerList[i]) {
                            usedID = true;

                        }



                    }

                    

                    //사용된 아이디가 아닐경우 
                    if (usedID == false)
                    {

                        reply = "NEWPLAYER$가입성공";

                        //데이터베이스에 유져정보를 Insert

                    }

                   
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error\n 프로그램을 종료하고 처음부터 다시 설정해 주세요 : " + e.Message);
            }

            return reply;

        }
         
        public string login_playerDB(string table, string admin_ID, string password, string playerInfo)
        {
            string login_reply = "LOGIN$로그인실패";
            try
            {
                //데이터베이스 연결설정 ->GameDB
                using (myConnection = new SqlConnection("Server=localhost;Database=" + table + ";Uid=" + admin_ID + ";Pwd=" + password + ";"))
                {


                    string[] msgArr = playerInfo.Split(new char[] { '#' });

                    // DB 열기
                    myConnection.Open();

                   

                    try {
                        //해당 플레이어 ID로부터 튜플을 찾음
                        string statmsg = "PLAYER$";

                        //해당튜플 정보를 플레이어에게 송신



                        return statmsg;

                    } catch (Exception e) {

                        //로그인 실패
                        return login_reply;
                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error\n 프로그램을 종료하고 처음부터 다시 설정해 주세요 : " + e.Message);
            }

            return login_reply;

        }

        public string logout_playerDB(string table, string admin_ID, string password, string playerInfo)
        {
            string reply = "LOGINOUT$로그아웃실패";
            try
            {
                //데이터베이스 연결설정 ->GameDB
                using (myConnection = new SqlConnection("Server=localhost;Database=" + table + ";Uid=" + admin_ID + ";Pwd=" + password + ";"))
                {


                    string[] msgArr = playerInfo.Split(new char[] { '#' });

                    // DB 열기
                    myConnection.Open();



                    try
                    {
                        //해당 플레이어 ID로부터 튜플을 찾음
                        string statmsg = "LOGOUT$로그아웃성공";

                        //msgArr -> 데이터베이스에 넣음 

                        //기존튜플 삭제 

                        //새 튜플추가 



                        return statmsg;

                    }
                    catch (Exception e)
                    {

                        //로그아웃 실패
                        return reply;
                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error\n 프로그램을 종료하고 처음부터 다시 설정해 주세요 : " + e.Message);
            }

            return reply;

        }

        public void getMonsterDB(string table, string admin_ID, string password )
        {
            try
            {
                //데이터베이스 연결설정 ->GameDB
                using (myConnection = new SqlConnection("Server=localhost;Database=" + table + ";Uid=" + admin_ID + ";Pwd=" + password + ";"))
                {
                    // DB 열기
                    myConnection.Open();
                    Console.WriteLine("************************************************************");
                    Console.WriteLine("*********************데이터베이스에 접속완료****************");
                    Console.WriteLine("************************************************************");


                    //일단 몇개 샘플을 넣었습니다. 
                    string[] mon_names = {"Mon1","Mon1","Mon1","Mon1" };


                    for (int i =0; i<mon_names.Length;i++) {
                        Program.Monster_Data mon = new Program.Monster_Data();



                        //검색으로 튜플 불러오기 


                        //스트링 데이터 슬라이싱
                        //이름 HP X Y ANIME


                        //구조체에 적용
                        mon.ID_name = mon_names[i];


                        //리스트에 추가
                        Program.monsterList.Add(mon);

                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" DB error : " + e.Message);
            }

        }





    }
}