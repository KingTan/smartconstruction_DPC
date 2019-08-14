using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace API_request_data
{
    class ConvertWind
    {
        /// <summary>
        /// 风速转换为风速等级
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int WindToLeve(float f)
        {
            int i = 0;
            if (f < 0.3)
                i = 0;
            else if (f >= 0.3 && f < 1.6)
                i = 1;
            else if (f >= 1.6 && f < 3.4)
                i = 2;
            else if (f >= 3.4 && f < 5.5)
                i = 3;
            else if (f >= 5.5 && f < 8.0)
                i = 4;
            else if (f >= 8.0 && f < 10.8)
                i = 5;
            else if (f >= 10.8 && f < 13.9)
                i = 6;
            else if (f >= 13.9 && f < 17.2)
                i = 7;
            else if (f >= 17.2 && f < 20.8)
                i = 8;
            else if (f >= 20.8 && f < 24.5)
                i = 9;
            else if (f >= 24.5 && f < 28.5)
                i = 10;
            else if (f >= 28.5 && f < 32.6)
                i = 11;
            else if (f > 32.6)
                i = 12;
            return i;
        }

        /// <summary>
        /// 风速等级转换风速
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float LeveToWind(int f)
        {
            switch (f)
            {
                case 0:
                    return RandomHelp.GetRandom(0, 29) / 100.00f;
                case 1:
                    return RandomHelp.GetRandom(30, 159) / 100.00f;
                case 2:
                    return RandomHelp.GetRandom(160, 339) / 100.00f;
                case 3:
                    return RandomHelp.GetRandom(340, 549) / 100.00f;
                case 4:
                    return RandomHelp.GetRandom(550, 799) / 100.00f;
                case 5:
                    return RandomHelp.GetRandom(800, 1079) / 100.00f;
                case 6:
                    return RandomHelp.GetRandom(1080, 1389) / 100.00f;
                case 7:
                    return RandomHelp.GetRandom(1390, 1719) / 100.00f;
                case 8:
                    return RandomHelp.GetRandom(1720, 2079) / 100.00f;
                case 9:
                    return RandomHelp.GetRandom(2080, 2449) / 100.00f;
                case 10:
                    return RandomHelp.GetRandom(2450, 2849) / 100.00f;
                case 11:
                    return RandomHelp.GetRandom(2850, 3259) / 100.00f;
                case 12:
                    return RandomHelp.GetRandom(3260, 3400) / 100.00f;
                default:
                    return 0f;
            }
        }

        public static class RandomHelp
        {
            /// <summary>
            /// 获取指定范围内的随机数
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <returns></returns>
            public static int GetRandom(int min, int max)
            {
                //随机数
                long tick = DateTime.Now.Ticks;
                Random seed = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                return seed.Next(min, max);
            }
        }
    }
}
