using System;

namespace SharpGameServer
{
    class MonNavi
    {


        //몬스터 인식범위에 충돌하면 해당 클라이언트 정보를 반환
        public static void start(Program.Monster_Data mon, int index)
        {



            float mx = mon.stat.x;
            float my = mon.stat.y;
            int HP = mon.HP;
            Program.Client_Data player;

            for (int i = 0; i < Program.clientList.Count; i++)
            {
                player = Program.clientList.ToArray()[i];
                float px = player.stat.x;
                float py = player.stat.y;

                //플레이어와 몬스터 사이의 거리 계산
                double distance = Math.Sqrt((mx - px) * (mx - px) + (my - py) * (my - py));
                if ((mon.target_name == "") || mon.target_name == player.ID_name) {
                    //움직이지 않고 공격
                    if (mon.HP > 0 && distance <= 1)
                    {

                        mon.stat.anime = "ATTACK";

                    }
                    //몬스터와 플레이어가 가까워질 경우 몬스터가 플레이어 추적
                    else if (mon.HP > 0 && distance <= 10)
                    {

                        //Console.WriteLine("몬스터 인식범위에 충돌됨!");

                        mon.target_name = player.ID_name;



                        //몬스터를 이동 
                        mon = MoveTo(mx, my, px, py, mon);


                            mon.stat.anime = "MOVE";
                            
                        



                    }
                    else {
                        mon.stat.anime = "IDLE";
                        mon.target_name = "";
                    }
                }
                string mon_msg = "MONSTER$";

                mon_msg = (mon_msg + "" + mon.ID_name + "#" + mon.stat.x + "#" + mon.stat.y + "#" + mon.stat.anime + "#" + mon.HP);



                //현재 몬스터 필드테이블을 접속한 모든 플레이어에게 전송


                Program.monsterList.RemoveAt(index);
                Program.monsterList.Insert(index, mon);
                
                Console.WriteLine("monster : " + mon_msg+", Target :"+mon.target_name);
                Console.WriteLine("player id = " + player.ID_name + ", x = " + player.stat.x+" , y = "+player.stat.y);
                Console.WriteLine("계산된 거리 : " + distance);
                
                Program.TcpMultiCast(mon_msg);


            }
        }



        static Program.Monster_Data MoveTo(float mx, float my, float px, float py, Program.Monster_Data mon) {

            if (mx < px) {

                mon.stat.x += 0.02f;
                if (my < py)
                {

                    mon.stat.y += 0.02f;
                }
                if (my > py)
                {

                    mon.stat.y -= 0.02f;
                }

            }
            else
            {

                mon.stat.x -= 0.02f;
                if (my < py)
                {

                    mon.stat.y += 0.02f;
                }
                if (my > py)
                {

                    mon.stat.y -= 0.02f;
                }
            }
           
            return mon;
        }

    

    }
}