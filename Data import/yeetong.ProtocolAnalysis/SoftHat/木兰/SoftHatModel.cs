using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class SoftHatModelHeart
{
    /// <summary>
    /// 设备地址
    /// </summary>
    public string SnAdr { get; set; }
    /// <summary>
    /// 序列号
    /// </summary>
    public string Sequence { get; set; }
    /// <summary>
    /// 设备在线时间
    /// </summary>
    public string OnlineTime { get; set; }
}

public class SoftHatModelCurrent
{
    /// <summary>
    /// 设备地址
    /// </summary>
    public string SnAdr { get; set; }
    /// <summary>
    /// 序列号
    /// </summary>
    public string Sequence { get; set; }
    /// <summary>
    /// 设备在线时间
    /// </summary>
    public string OnlineTime { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public string StuState { get; set; }
    /// <summary>
    /// 卡号
    /// </summary>
    public List<string> CardId = new List<string>();
}

