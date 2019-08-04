using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.SoftHat.Mysql;



public static class GprsResolveSoftHat
{
    /// <summary>
    /// 心跳及实时数据解析
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
    {

        if (c == 8)
        {
            byte[] bytes = OnResolveHeabert(b, c);
            if (bytes != null)
                client.SendBuffer(bytes);
        }
        if (c > 8) //如果长度大于8位及实时数据
        {
           byte[] bytes= OnResolveCurrent(b, c);
           if (bytes != null)
               client.SendBuffer(bytes);
        }
        return "";
    }
    /// <summary>
    /// 心跳
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    private static byte[] OnResolveHeabert(byte[] b, int bCount)
    {
        SoftHatModelHeart heart = new SoftHatModelHeart();
        heart.SnAdr = ConvertData.ToHexString(b, 4, 1);
        heart.Sequence = ConvertData.ToHexString(b, 5, 1);
        heart.OnlineTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        byte[] bytes = new byte[8];
        bytes[0] = 0xff;
        bytes[1] = 0xff;
        bytes[2] = 0xff;
        bytes[3] = 0xff;
        bytes[4] = b[4];
        bytes[5] = b[5];
        bytes[6] = b[6];
        bytes[7] = b[7];
        return bytes;
    }
    /// <summary>
    /// 实时数据
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    private static byte[] OnResolveCurrent(byte[] b, int bCount)
    {
        SoftHatModelCurrent current = new SoftHatModelCurrent();
        current.SnAdr = ConvertData.ToHexString(b, 4, 1);
        current.Sequence = ConvertData.ToHexString(b, 5, 1);
        current.OnlineTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (ConvertData.ToHexString(b, 8, 1) == "81")
            current.StuState = "欠压";
        else
            current.StuState = "正常";
        byte[] by = new byte[4];
        for (int i = 9,j=0; i < bCount; i++,j++)
        {
            if (j == 4)
            {
                string id = Encoding.ASCII.GetString(by);
                current.CardId.Add(id);
                j = 0;
            }
            by[j] = b[i];
        }
        byte[] bytes = new byte[9];
        bytes[0] = 0xff;
        bytes[1] = 0xff;
        bytes[2] = 0xff;
        bytes[3] = 0xff;
        bytes[4] = b[4];
        bytes[5] = b[5];
        bytes[6] = b[6];
        bytes[7] = b[7];
        bytes[8] = 0x01;
        return bytes;
    }
}

