namespace SharpGameServer
{
    class MonNavi
    {
        public static void Start(Program.Monster_Data mon) {

            //인식범위에 들어오면 움직임
            if (CheckAttack(mon))
            {



            }
            else { //그렇지 않으면 일반움직임 




            }



        }

        //몬스터 인식범위에 충돌했는지 처리 
        static bool CheckAttack(Program.Monster_Data mon) {

            if (1==1) {

                return true;

            } else {

                return false;
            }

        }


    }
}