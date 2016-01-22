using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renju
{

    class Field
    {
        public char char_white = 'O';
        public char char_black = 'X';

        static public int empty_code = -1;
        static public int white_code = 0;
        static public int max_code = white_code;
        static public int black_code = 1;
        static public int min_code = black_code;

        public int alpha_beta_res = 0;
        public int operations = 0;

        public int[,] cur_points = new int[15, 15];
        bool[,,] flags = new bool[4, 15, 15];
        bool[,,] locked_cells = new bool[4, 15, 15];

        public int current_step = 0;

        public int whosStepNow = black_code;

        //Массив после шага
        public int[,] Step(int[,] points, int who, int x, int y)
        {
            int[,] temp_points = (int[,]) points.Clone();
            if(x >= 0 && y >= 0)
                temp_points[x, y] = who;
            return temp_points;
        }
            
        //Очистить массивы
        public void Clear()
        {
            current_step = 0;
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    cur_points[i, j] = empty_code;
                }
            }
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                    for (int k = 0; k < 4; k++)
                        locked_cells[k, i, j] = false;
        }

        //Вывести игровое поле
        public void OutField()
        {
            Console.Clear();
            Console.SetCursorPosition(4, 2);
            for (int i = 0; i <= 30; i++)
            {
                for (int j = 0; j <= 60; j++)
                {
                    if (i == 0)
                    {
                        if (j == 0) Console.Write("┌");
                        else if (j == 60) Console.Write("┐");
                        else if (j % 4 == 0) Console.Write("┬");
                        else  Console.Write("─");
                    }
                    else if (i == 30)
                    {
                        if (j == 0) Console.Write("└");
                        else if (j == 60) Console.Write("┘");
                        else if (j % 4 == 0) Console.Write("┴");
                        else  Console.Write("─");
                    }
                    else if (i % 2 == 0)
                    {
                        if (j == 0) Console.Write("├");
                        else if (j == 60) Console.Write("┤");
                        else if (j % 4 == 0) Console.Write("┼");
                        else Console.Write("─");
                    }
                    else if (i % 2 == 1)
                    {
                        if (j == 0) Console.Write("│");
                        else if (j == 60) Console.Write("│");
                        else if (j % 4 == 0) Console.Write("│");
                        else Console.Write(" ");
                    }
                }
                Console.SetCursorPosition(4, Console.CursorTop + 1);
            }

            for (int i = -1; i < 16; i++)
            {
                for (int j = -1; j < 16; j++)
                {
                    Console.SetCursorPosition(2 + (j + 1)*4, 1 + (i + 1) * 2);
                    if ((i == -1 && j == 15) || (i == 15 && j == -1) || (i == -1 && j == -1) || (i == 15 && j == 15)) Console.Write(" ");
                    if ((i == -1 || i == 15) && j >= 0 && j < 15) Console.Write((j ) + "");
                    if ((j == -1 || j == 15) && i >= 0 && i < 15) Console.Write((i ) + "");
                    if (i > -1 && j > -1 && i < 15 && j < 15)
                    {
                        if (cur_points[i, j] == black_code) Console.Write("" + char_black + "");

                        if (cur_points[i, j] == empty_code) Console.Write(" ");
                        if (cur_points[i, j] == white_code) Console.Write("" + char_white + "");
                    }

                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        //Может ли игрок использовать эту ячейку 
        public bool Compare(int[,] points, int who, int x, int y)
        {
            return (x < 15) && (x >= 0) &&
                   (y < 15) && (y >= 0) &&
                   points[x, y] == who;
        }

        public bool CanStepHere(int[,] points, int who, int x, int y, bool FirstStepsRules, bool lightVersion)
        {
            if ((x < 15) && (x >= 0) &&
                (y < 15) && (y >= 0) &&
                points[x, y] == empty_code)
            {
                if (who == white_code)
                {
                    if (FirstStepsRules && current_step == 1 && ((Math.Abs(x - 7) > 1) || (Math.Abs(y - 7) > 1))) { return false; }
                }

                    if (who == black_code)
                {
                    if (FirstStepsRules && current_step == 0 && ((x != 7) || (y != 7))) { return false; }
                    if (FirstStepsRules && current_step == 2 && ((Math.Abs(x - 7) > 2) || (Math.Abs(y - 7) > 2))) { return false; }

                    if (!lightVersion)
                    {
                        int[,] len = new int[4, 2];
                        //(-)
                        LineLenght(points, black_code, x, y, 1, 0, out len[0, 0]);
                        LineLenght(points, black_code, x, y, -1, 0, out len[0, 1]);
                        //(|)
                        LineLenght(points, black_code, x, y, 0, 1, out len[1, 0]);
                        LineLenght(points, black_code, x, y, 0, -1, out len[1, 1]);
                        //(/)
                        LineLenght(points, black_code, x, y, 1, -1, out len[2, 0]);
                        LineLenght(points, black_code, x, y, -1, 1, out len[2, 1]);
                        //(\)
                        LineLenght(points, black_code, x, y, 1, 1, out len[3, 0]);
                        LineLenght(points, black_code, x, y, -1, -1, out len[3, 1]);

                        // В каком-то из направлений ряд длины 6 и больше
                        for (int i = 0; i < 4; i++)
                        {
                            if (len[i, 0] + len[i, 1] + 1 > 5) return false;
                        }

                        //Если построен ряд из 5, то не надо проверки на вилки
                        for (int i = 0; i < 4; i++)
                        {
                            if (len[i, 0] + len[i, 1] + 1 == 5) return true;
                        }

                    
                        //Вилки. 4x4
                        int fours = 0;
                        int step_x = 0, step_y = 0;
                        int[,] new_points = Step(points, who, x, y);

                        for (int i = 0; i < 4; i++) //Перебираем все направления
                        {
                            if (i == 0) { step_x = 1; step_y = 0; }
                            if (i == 1) { step_x = 0; step_y = 1; }
                            if (i == 2) { step_x = 1; step_y = -1; }
                            if (i == 3) { step_x = 1; step_y = 1; }

                            //Особая вилка в одном направлении X-XxX-X
                            int right = 0;
                            for (int j = -3; j <= 3; j++)
                            {
                                if (j != 0)
                                {
                                    if (j % 2 == 0 && CanStepHere(new_points, empty_code, x + step_x * (j), y + step_y * (j), false)) right++;
                                    if (Math.Abs(j % 2) == 1 && Compare(new_points, black_code, x + step_x * (j), y + step_y * (j))) right++;
                                }

                            }
                            if (right == 6) return false;

                            //Вилки в разных направлениях
                            for (int j = -4; j <= 0; j++) //Двигаем область возможной новой пятерки
                            {
                                int empty = 0;
                                int your = 0;
                                int empty_x = 0; int empty_y = 0;
                                //Если в нашей пятерке 1 пустое место и 4 черных
                                for (int k = 0; k <= 4; k++)
                                {
                                    if (Compare(new_points, empty_code, x + step_x * (j + k), y + step_y * (j + k)))
                                    {
                                        empty_x = x + step_x * (j + k);
                                        empty_y = y + step_y * (j + k);
                                        empty++;

                                    }
                                    else if (Compare(new_points, black_code, x + step_x * (j + k), y + step_y * (j + k)))
                                    {
                                        your++;
                                    }

                                }
                                if (empty == 1 && your == 4)
                                {
                                    //Если есть возможность создать пятерку, поставив точку на пустую

                                    if (CanStepHere(new_points, who, empty_x, empty_y, false))
                                    {
                                        fours++;
                                        if (fours >= 2) return false;
                                        //Console.WriteLine("Empty X " + empty_x + " empty Y " + empty_y);
                                        //Console.WriteLine("New four " + i + " Points: " + your +" " + empty);
                                        break;
                                    }
                                }
                            }
                        }


                        //Вилки. 3x3
                        int threes = 0;
                        step_x = 0; step_y = 0;

                        for (int i = 0; i < 4; i++) //Перебираем все направления
                        {
                            if (i == 0) { step_x = 1; step_y = 0; }
                            if (i == 1) { step_x = 0; step_y = 1; }
                            if (i == 2) { step_x = 1; step_y = -1; }
                            if (i == 3) { step_x = 1; step_y = 1; }


                            for (int j = -3; j <= 0; j++) //Двигаем область возможной новой открытой четверки
                            {
                                int empty = 0;
                                int your = 0;
                                int empty_x = 0; int empty_y = 0;
                                //Если в нашей четверке 1 пустое место (не считая вилки)
                                for (int k = 0; k <= 3; k++)
                                {
                                    if (Compare(new_points, empty_code, x + step_x * (j + k), y + step_y * (j + k)))
                                    {
                                        empty_x = x + step_x * (j + k);
                                        empty_y = y + step_y * (j + k);
                                        empty++;
                                    }
                                    else if (Compare(new_points, who, x + step_x * (j + k), y + step_y * (j + k)))
                                    {
                                        your++;
                                    }

                                }
                                if (empty == 1 && your == 3)
                                {
                                    //Если есть возможность создать открытую 4-ку, поставив точку на пустую
                                    //if (!CanStepHere(Step(new_points, who, empty_x, empty_y), who, x + step_x * (j - 1), y + step_y * (j - 1)))
                                    //    Console.WriteLine("Cant step on  " + (x + step_x * (j - 1)) + " - " + (step_y * (j - 1)));
                                    //if (!CanStepHere(Step(new_points, who, empty_x, empty_y), who, x + step_x * (j + 1), y + step_y * (j + 1)))
                                    //    Console.WriteLine("Cant step on  " + (x + step_x * (j + 1)) + " - " + (step_y * (j + 1)));
                                    //Console.WriteLine("Empty X " + empty_x + " empty Y " + empty_y);
                                    if (CanStepHere(new_points, who, empty_x, empty_y, false) &&
                                        CanStepHere(Step(new_points, who, empty_x, empty_y), who, x + step_x * (j - 1), y + step_y * (j - 1), false) &&
                                        CanStepHere(Step(new_points, who, empty_x, empty_y), who, x + step_x * (j + 1 + 3), y + step_y * (j + 1 + 3), false))
                                    {
                                        //Console.WriteLine("New free " + i);
                                        threes++;
                                        if (threes > 1) return false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    

                }
                
                return true;

            }
            else
            {
                return false;
            }
        }

        public bool CanStepHere(int[,] points, int who, int x, int y, bool FirstStepsRules)
        {
            return CanStepHere(points, who, x, y, FirstStepsRules, false);
        }

        //Остаток длины линии
        public void LineLenght(int[,] points, int who, int x, int y, int x_step, int y_step, out int len, out int last_x, out int last_y)
        {
            if ((x + x_step < 15) && (x + x_step >=0) &&
                (y + y_step < 15) && (y + y_step >= 0) &&
                points[x + x_step, y + y_step] == who)
            {
                int r_len;
                LineLenght(points, who, x + x_step, y + y_step, x_step, y_step, out r_len, out last_x, out last_y);
                len = 1 + r_len;
            }
            else
            {
                last_x = x;
                last_y = y;
                len = 0;
            }
        }
        public void LineLenght(int[,] points, int who, int x, int y, int x_step, int y_step, out int len)
        {
            if ((x + x_step < 15) && (x + x_step >= 0) &&
                (y + y_step < 15) && (y + y_step >= 0) &&
                points[x + x_step, y + y_step] == who)
            {
                int r_len;
                LineLenght(points, who, x + x_step, y + y_step, x_step, y_step, out r_len);
                len = 1 + r_len;
            }
            else
            {
                len = 0;
            }
        }
        
        //Побеждает ли игрок
        public bool isWinner(int[,] points, int who)
        {
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                    for (int k = 0; k < 4; k++)
                        flags[k, i, j] = false;

            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    if (points[i, j] == who)
                    {
                        int step_x = 0; int step_y = 0;

                        for (int k = 0; k < 4; k++) //Перебираем все направления
                        {
                            if (k == 0) { step_x = 1; step_y = 0; }
                            if (k == 1) { step_x = 0; step_y = 1; }
                            if (k == 2) { step_x = 1; step_y = -1; }
                            if (k == 3) { step_x = 1; step_y = 1; }
                            {
                                if (!flags[k, i, j]) //Если выбранная точка наша и направление еще не рассмотренно
                                {
                                    flags[k, i, j] = true;

                                    int w_len = 0;
                                    int l = 0;//Перебираем саму линию
                                    while (Compare(points, who, i + step_x * l, j + step_y * l))
                                    {
                                        w_len++;
                                        flags[k, i + step_x * l, j + step_y * l] = true;
                                        l++;
                                    }

                                    if (w_len == 5) return true;

                                }
                            }
                        }
                    }

                }
            
            return false;

        }

        //Оценка доски для игрока
        public int Mark(int[,] points, int who)
        {
            int mark = 0;
            int o_ones = 0;
            int o_two = 0;
            int o_threes = 0;
            int o_fours = 0;
            int ones = 0;
            int two = 0;
            int threes = 0;
            int fours = 0;
            //
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                    for(int k = 0; k < 4; k++) 
                        flags[k, i, j] = false;

            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    if(points[i, j] == who)
                    {
                        int step_x = 0; int step_y = 0;

                        for (int k = 0; k < 4; k++) //Перебираем все направления
                        {
                            if (k == 0) { step_x = 1; step_y = 0; }
                            if (k == 1) { step_x = 0; step_y = 1; }
                            if (k == 2) { step_x = 1; step_y = -1; }
                            if (k == 3) { step_x = 1; step_y = 1; }
                            {
                                if (!flags[k, i, j] && !locked_cells[k, i, j]) //Если выбранная точка наша и направление еще не рассмотренно
                                {
                                    flags[k, i, j] = true;

                                    int w_len = 0, last_x = 0, last_y = 0;
                                    int e1_len = 0; //Пустых ячеек слева
                                    int e2_len = 0; //Пустых ячеек справа

                                    int l = 0;//Перебираем саму линию
                                    while (Compare(points, who, i + step_x * l, j + step_y * l))
                                    {
                                        w_len++;
                                        flags[k, i + step_x * l, j + step_y * l] = true;
                                        last_x = i + step_x * l;
                                        last_y = j + step_y * l;
                                        l++;
                                    }

                                    if (w_len == 5)
                                    {
                                        //Console.WriteLine("Нашел пятерку: (" + i + ";" + j + ") - (" + (i + step_x * (w_len - 1)) + ";" + (j + step_y * (w_len - 1)) + "). Его длина: " + w_len);
                                        return int.MaxValue;
                                    }

                                    //Перебираем справа 
                                    while (e2_len + w_len < 5 && 
                                           (CanStepHere(points, who, i + step_x * l, j + step_y * l, false) ||
                                           Compare(points, who, i + step_x * l, j + step_y * l)))
                                    {
                                        e2_len++;
                                        l++;
                                    }
                                    l = -1;//Перебираем слева
                                    while ((e1_len + e2_len + w_len < 5 || e1_len == 0) &&
                                           (CanStepHere(points, who, i + step_x * l, j + step_y * l, false) ||
                                           Compare(points, who, i + step_x * l, j + step_y * l)))
                                    {
                                        e1_len++;
                                        l--;
                                    }
                                    //Если ряд заблокирован с двух сторон, его больше не рассматриваем
                                    if (w_len >= 1 && w_len != 5 && e1_len == 0 && e2_len == 0)
                                    {
                                        //Блокирую ряд
                                        //Console.WriteLine("Ряд закрыт: (" + i + ";" + j + ") - (" + (i + step_x * (w_len-1)) + ";" + (j + step_y * (w_len - 1)) + "). Его длина: " + w_len);
                                        //for (l = 0; l < w_len; l++) locked_cells[k, i + step_x * l, j + step_y * l] = true;
                                    }
                                    //Повышаем оценку
                                    if (e1_len + e2_len + w_len >= 5)
                                    {
                                        //mark += w_len;
                                        if (w_len == 1 && (e1_len != 0 && e2_len != 0)) o_ones++;
                                        if (w_len == 2 && (e1_len != 0 && e2_len != 0)) o_two++;
                                        if (w_len == 3 && (e1_len != 0 && e2_len != 0)) o_threes++;
                                        if (w_len == 4 && (e1_len != 0 && e2_len != 0)) o_fours++;
                                        if (w_len == 1 && (e1_len == 0 || e2_len == 0)) ones++;
                                        if (w_len == 2 && (e1_len == 0 || e2_len == 0)) two++;
                                        if (w_len == 3 && (e1_len == 0 || e2_len == 0)) threes++;
                                        if (w_len == 4 && (e1_len == 0 || e2_len == 0)) fours++;
                                        //if (w_len == 5) return int.MaxValue;
                                        //if (w_len == 4) mark = 10000;
                                        //if (w_len == 4 && (e1_len == 0 || e2_len == 0)) mark = 5000;
                                        //if (w_len == 5) mark = int.MaxValue; //См.ранее!
                                        //if (e1_len > 0) mark *= w_len;
                                        //if (e2_len > 0) mark *= w_len;
                                        //if(w_len>1)
                                        //Console.WriteLine("Линия. Наших: " + w_len + " Пустых: " + (e1_len + e2_len) + " Направление: " + k);
                                    }
                                }
                            }
                        }
                    }
                    
                }
            //mark += 2 * o_ones * o_ones;
            //mark += 1 * ones;
            mark += 5 * o_two * o_two;
            mark += 3 * two;
            mark += 80 * o_threes * o_threes;
            mark += 10 * threes;
            mark += 1000 * o_fours * o_fours;
            mark += 20 * fours;
            return mark;

        }

        //Оценка доски
        public int MarkField(int[,] points)
        {
            return Mark(points, max_code) - Mark(points, min_code);
        }

        //Сравнение оценок
        public bool isBetterForMe(int who, int newMark, int oldMark)
        {
            if(who == max_code) return newMark > oldMark;
            else return newMark < oldMark;
        }

        //Выбрать шаг для компьютера
        public void SelectStep(int[,] points, int who, int remaining_depth, out int selected_mark, out int selected_x, out int selected_y, int alpha, int beta)
        {
            operations++;

            int alpha_new = alpha, beta_new = beta;
            int mark, better_mark = 0, better_x = -1, better_y = -1;
            
            if (who == max_code) mark = int.MinValue;
            else mark = int.MaxValue;

            better_mark = mark;

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (alpha_new >= beta_new) alpha_beta_res++;
                    if (alpha_new < beta_new && CanStepHere(points, who, i, j, true, true)) //Подбираем лучший из подходящих (без учета вилок)
                    {
                        int temp_x, temp_y;
                        int[,] new_points = Step(points, who, i, j);
                        if (remaining_depth == 0 || isWinner(new_points, who == max_code ? min_code : max_code))
                        {
                            mark = MarkField(new_points);
                        }
                        else
                        {
                            SelectStep(new_points, who == max_code ? min_code : max_code,
                                    remaining_depth - 1, out mark, out temp_x, out temp_y, alpha_new, beta_new);
                        }
                        
                        //Если след ход минимизировал (А этот - макс), устанавливаем максимум alpha:mark, иначе минимум beta:mark
                        if (who == max_code && alpha_new < mark) alpha_new = mark;
                        if (who == min_code && beta_new > mark) beta_new = mark;

                        if (isBetterForMe(who, mark, better_mark) && CanStepHere(points, who, i, j, true))
                        {
                            better_mark = mark;
                            better_x = i;
                            better_y = j;
                        }
                    }
                }
            }
            
    
            

            selected_mark = better_mark;
            selected_x = better_x;
            selected_y = better_y;
        }
        
    }
}
