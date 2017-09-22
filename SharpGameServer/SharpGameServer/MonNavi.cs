using System;

namespace SharpGameServer
{
    class MonNavi
    {


        //몬스터 인식범위에 충돌하면 해당 클라이언트 정보를 반환
        public static void start(Program.Monster_Data mon, int index) {
            float mx = mon.stat.x;
            float my = mon.stat.y;
            int HP = mon.HP;
            Program.Client_Data player;
            for (int i=0;i<Program.clientList.Count;i++) {
                player = Program.clientList.ToArray()[i];
                float px = player.stat.x;
                float py = player.stat.y;

                //플레이어와 몬스터 사이의 거리 계산
                double distance = Math.Sqrt((mx - px)*(mx - px) + (my-py)* (my - py));

                //움직이지 않고 공격
                if (mon.HP > 0 && distance <= 1) {

                    mon.stat.anime = "ATTACK";

                }
                //몬스터와 플레이어가 가까워질 경우 몬스터가 플레이어 추적
                else if (mon.HP > 0 && distance <= 20)
                {
                    //몬스터를 이동 
                    MoveTo(mx, my, px, py, mon);
                    mon.stat.anime = "MOVE";



                }
                else { //몬스터 대기  
                    mon.stat.anime = "IDLE";


                }
                string mon_msg = "MONSTER$";
                mon_msg = mon_msg + "" + mon.ID_name + "#" + mon.stat.x + "#" + mon.stat.y + "#" + mon.stat.anime + "#" + mon.HP + "#" + mon.angle;
                //현재 몬스터 필드테이블을 접속한 모든 플레이어에게 전송
                Program.TcpMultiCast(mon_msg);
                Program.monsterList.RemoveAt(index);
                Program.monsterList.Insert(index, mon);




            }



        }



        static void MoveTo(float mx , float my,float px, float py, Program.Monster_Data mon) {

            if (mx < px) {

                mon.stat.x += 0.1f;
            }
            if (mx > px)
            {

                mon.stat.x -= 0.1f;
            }
            if (my < py)
            {

                mon.stat.y += 0.1f;
            }
            if (mx > px)
            {

                mon.stat.y -= 0.1f;
            }

        }



    }
}