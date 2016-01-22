using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renju
{
    class Program
    {
        static void Main(string[] args)
        {


            //Dictionary<int, int> temp = new Dictionary<int, int>(); ;
            //temp[5] = 10;
            //Console.WriteLine(temp[5]);
            //Console.ReadKey();
            Field field = new Field();
            int x;
            int y;
            bool is_white_move = false;

            field.Clear();

            //field.cur_points[7, 8] = 1;
            //field.cur_points[7, 9] = 1;
            //field.cur_points[8, 8] = 1;
            //field.cur_points[9, 9] = 1;

            //field.cur_points[7, 8] = 0;
            //field.cur_points[8, 8] = 0;
            //field.cur_points[9, 8] = 0;
            //field.cur_points[10, 8] = 0;
            //field.cur_points[11, 8] = 0;

            field.OutField();

            //Console.WriteLine(field.CanStepHere(field.cur_points, Field.black_code, 7, 7, false, false));

            while (true)
            {
                field.alpha_beta_res = 0;
                field.operations = 0;
                Console.WriteLine("Оценка игры (Чем выше, тем ближе победа белых): " + field.MarkField(field.cur_points));
                
                if (!is_white_move)
                {
                    int temp_mark;
                    //field.whosStepNow = 
                    field.SelectStep(field.cur_points, is_white_move ? Field.white_code : Field.black_code, 1, out temp_mark, out x, out y,int.MinValue, int.MaxValue);
                    Console.WriteLine("Хожу на (" + x + ";" + y + ")");
                    Console.WriteLine("Отсечено alpha-beta алгоритмом: " + field.alpha_beta_res);
                    Console.WriteLine("Операций: " + field.operations);
                    //field.cur_points = field.Step(field.cur_points, is_white_move ? field.white_code : field.black_code, x, y);

                }
                else
                {
                    x = -1; y = -1;
                    bool temp = true;
                    while (temp)
                    {
                        try
                        {
                            x = int.Parse(Console.ReadLine());
                            y = int.Parse(Console.ReadLine());

                            temp = false;
                        }
                        catch { }
                        
                    }
                    
                }


                if (field.CanStepHere(field.cur_points, is_white_move ? Field.white_code : Field.black_code, x, y, true))
                {
                    field.cur_points = field.Step(field.cur_points, is_white_move ? Field.white_code : Field.black_code, x, y);

                    is_white_move = !is_white_move;
                    field.current_step++;
                }

                if (x == -1 || y == -1)
                {
                    is_white_move = !is_white_move;
                    field.current_step++;
                }
                Console.WriteLine("Продолжить?");
                Console.ReadKey();
                field.OutField();
            }

        }
    }
}
