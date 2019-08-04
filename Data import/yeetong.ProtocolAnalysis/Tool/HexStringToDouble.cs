using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：把16进制的字符串转换成对应的double类型
    创建时间：2017.6.28
    文件功能描述：把16进制的字符串转换成对应的double类型
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace ProtocolAnalysis
{
    public class HexStringToDouble
    {
        public static float HexStringToDoubleFun(string HexString)
        {
            try
            {
                uint num = uint.Parse(HexString, System.Globalization.NumberStyles.AllowHexSpecifier);
                byte[] floatValues = BitConverter.GetBytes(num);
                float f = BitConverter.ToSingle(floatValues, 0);
                return f;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
