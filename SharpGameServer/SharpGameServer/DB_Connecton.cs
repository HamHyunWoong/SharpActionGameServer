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

    }
}