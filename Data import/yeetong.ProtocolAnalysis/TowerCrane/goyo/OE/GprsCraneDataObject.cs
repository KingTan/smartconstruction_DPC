using System;
using System.Collections.Generic;

using System.Text;

namespace ProtocolAnalysis.TowerCrane.OE
{
    /// <summary>
    /// 数据对象类
    /// </summary>
    public class GprsCraneDataObject
    {
        /// <summary>
        /// 实时数据对象
        /// </summary>
        private CraneCurrent current = new CraneCurrent();
        public CraneCurrent Current
        {
            get { return current; }
            set { current = value; }
        }
        /// <summary>
        /// 心跳
        /// </summary>
        private Heartbeat heartbeat = new Heartbeat();
        public Heartbeat Heartbeat
        {
            get { return heartbeat; }
            set { heartbeat = value; }
        }
        private CraneConfig _craneConfig = new CraneConfig();
        /// <summary>
        /// 参数上传
        /// </summary>
        public CraneConfig CraneConfig
        {
            get { return _craneConfig; }
            set { _craneConfig = value; }
        }
        private CraneRunTime _craneRuntime = new CraneRunTime();
        /// <summary>
        /// 塔吊运行时间
        /// </summary>
        public CraneRunTime CraneRuntime
        {
            get { return _craneRuntime; }
            set { _craneRuntime = value; }
        }
        /// <summary>
        /// 拷贝
        /// </summary>
        /// <returns></returns>
        public GprsCraneDataObject Clone()
        {
            return (GprsCraneDataObject)this.MemberwiseClone();
        }
        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <returns></returns>
        public object DeepClone()
        {
            GprsCraneDataObject newP = new GprsCraneDataObject();
            newP = (GprsCraneDataObject)this.MemberwiseClone();
            return newP;
        }
    }
}
