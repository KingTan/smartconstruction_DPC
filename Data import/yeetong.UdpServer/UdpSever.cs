using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class UdpSever
{
    #region 字段
    // 定义节点
    IPEndPoint ipEndPoint = null;
    // 定义UDP发送和接收
    UdpClient udpReceive = null;
    UdpState udpReceiveState = null;
    public event Action<Byte[], UdpState> eventTrigger;
    // 异步状态同步 
    AutoResetEvent receiveDone = new AutoResetEvent(false);
    static object syncUdp = new object();
    Thread t;
    #endregion

    public void UdpSeverInit(int port)
    {
        // 本机节点
        ipEndPoint = new IPEndPoint(IPAddress.Any, port);
        // 实例化
        udpReceive = new UdpClient(ipEndPoint);

        // 分别实例化udpSendState、udpReceiveState
        udpReceiveState = new UdpState();
        udpReceiveState.udpClient = udpReceive;
        udpReceiveState.ipEndPoint = ipEndPoint;

        //监听UDP的线程
        t = new Thread(Thread_Listener);
        t.IsBackground = true;
        t.Priority = ThreadPriority.AboveNormal;
        t.Start();
    }
    /// <summary>
    /// 监听链接
    /// </summary>
    public void Listener(int port)
    {
        UdpSeverInit(port);
    }
    /// <summary>
    /// 监听链接的线程
    /// </summary>
    void Thread_Listener()
    {
        while (true)
        {
            lock (syncUdp)
            {
                try
                {
                    // 调用接收回调函数
                    IAsyncResult iar = udpReceive.BeginReceive(new AsyncCallback(ReceiveCallback), udpReceiveState);
                    receiveDone.WaitOne();
                }
                catch
                {
                    receiveDone.Set();
                }
            }
        }
    }
    /// <summary>
    /// 接收回调函数
    /// </summary>
    /// <param name="iar"></param>
    void ReceiveCallback(IAsyncResult iar)
    {
        try
        {
            UdpState udpState = iar.AsyncState as UdpState;
            if (iar.IsCompleted)
            {
                Byte[] rBytes = udpState.udpClient.EndReceive(iar, ref udpState.remoteEP);
                if (eventTrigger != null)
                    eventTrigger(rBytes, udpState);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            receiveDone.Set();
        }
    }
    /// <summary>
    /// 发送函数
    /// </summary>
    public void SendMsg(UdpState udpState, string content)
    {
        Byte[] sendBytes = Encoding.Default.GetBytes(content);
        try
        {
            udpState.udpClient.Send(sendBytes, sendBytes.Length, udpState.remoteEP);
        }
        catch
        {
        }
    }/// <summary>
     /// 发送函数
     /// </summary>
    public static void SendMsgStr(UdpState udpState, string content)
    {
        //Byte[] sendBytes = Encoding.UTF8.GetBytes(content);
        Byte[] sendBytes=Enumerable.Range(0, content.Length).Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(content.Substring(x, 2), 16))
                     .ToArray();
        try
        {
            udpState.udpClient.Send(sendBytes, sendBytes.Length, udpState.remoteEP);
        }
        catch
        {
        }
    }
    /// <summary>
    /// 停止监听
    /// </summary>
    public void StopListen()
    {
        try
        {
            if (udpReceive != null)
                udpReceive.Close();
            if (ipEndPoint != null)
                ipEndPoint = null;
            if (udpReceiveState != null)
                udpReceiveState = null;
            if (t != null && t.IsAlive)
            {
                t.Abort();
                t = null;
            }
        }
        catch (Exception)
        {

        }
    }
}

public class UdpState
{
    public UdpClient udpClient;
    public IPEndPoint ipEndPoint;
    public IPEndPoint remoteEP;
    public byte[] buffer = new byte[1024];
    public int counter = 0;
}

