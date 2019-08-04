using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis
{
    public class StrongECurrent
    {
        /// <summary>
        /// 设备类型唯一标识
        /// </summary>
        public string Uniqueid { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public string EquipmentNo { get; set; }
        /// <summary>
        /// 漏电路数
        /// </summary>
        public string Eleakage { get; set; }
        /// <summary>
        /// 温度路数
        /// </summary>
        public string Etemperature { get; set; }
        /// <summary>
        /// 实时故障报警值报警值A
        /// </summary>
        public string FCurrentLeakagefaultA { get; set; }
        /// <summary>
        /// 实时故障报警值报警温度A
        /// </summary>
        public string FCurrentemfaultA { get; set; }
        /// <summary>
        /// 实时故障报警值报警温度B
        /// </summary>
        public string FCurrentemfaultB { get; set; }
        /// <summary>
        /// 实时故障报警值报警温度C
        /// </summary>
        public string FCurrentemfaultC { get; set; }
        /// <summary>
        /// 实时故障报警值报警温度D
        /// </summary>
        public string FCurrentemfaultN { get; set; }
        /// <summary>
        /// 实时故障报警值电流路数A
        /// </summary>
        public string FWaterA { get; set; }
        /// <summary>
        /// 实时故障报警值电流路数B
        /// </summary>
        public string FWaterB { get; set; }
        /// <summary>
        /// 实时故障报警值电流路数C
        /// </summary>
        public string FWaterC { get; set; }
        /// <summary>
        /// 实时故障报警值电压路数A
        /// </summary>
        public string FVoltageA { get; set; }
        /// <summary>
        /// 实时故障报警值电压路数B
        /// </summary>
        public string FVoltageB { get; set; }
        /// <summary>
        /// 实时故障报警值电压路数C
        /// </summary>
        public string FVoltageC { get; set; }
        /// <summary>
        /// 实时值漏电路数
        /// </summary>
        public string CurrentLeakageA { get; set; }
        /// <summary>
        /// 实时值温度路数A
        /// </summary>
        public string CurrentTemA { get; set; }
        /// <summary>
        /// 实时值温度路数B
        /// </summary>
        public string CurrentTemB { get; set; }
        /// <summary>
        /// 实时值温度路数C
        /// </summary>
        public string CurrentTemC { get; set; }
        /// <summary>
        /// 实时值温度路数N
        /// </summary>
        public string CurrentTemN { get; set; }
        /// <summary>
        /// 实时电流路数A
        /// </summary>
        public string CWaterA { get; set; }
        /// <summary>
        /// 实时电流路数B
        /// </summary>
        public string CWaterB { get; set; }
        /// <summary>
        /// 实时电流路数C
        /// </summary>
        public string CWaterC { get; set; }
        /// <summary>
        /// 实时值电压路数A
        /// </summary>
        public string CVoltageA { get; set; }
        /// <summary>
        /// 实时值电压路数B
        /// </summary>
        public string CVoltageB { get; set; }
        /// <summary>
        /// 实时值电压路数C
        /// </summary>
        public string CVoltageC { get; set; }
        /// <summary>
        /// A相电量
        /// </summary>
        public string EamountA { get; set; }
        /// <summary>
        /// B相电量
        /// </summary>
        public string EamountB { get; set; }
        /// <summary>
        /// C相电量
        /// </summary>
        public string EamountC { get; set; }
        /// <summary>
        /// A相视载功率
        /// </summary>
        public string VloadA { get; set; }
        /// <summary>
        /// B相视载功率
        /// </summary>
        public string VloadB { get; set; }
        /// <summary>
        /// C相视载功率
        /// </summary>
        public string VloadC { get; set; }
        /// <summary>
        /// A相功率因数
        /// </summary>
        public string VfactorA { get; set; }
        /// <summary>
        /// B相功率因数
        /// </summary>
        public string VfactorB { get; set; }
        /// <summary>
        /// C相功率因数
        /// </summary>
        public string VfactorC { get; set; }
        /// <summary>
        /// A相频率
        /// </summary>
        public string VfrequencyA { get; set; }
        /// <summary>
        /// B相频率
        /// </summary>
        public string VfrequencyB { get; set; }
        /// <summary>
        /// C相频率
        /// </summary>
        public string VfrequencyC { get; set; }
        /// <summary>
        /// 电流互感穿心方向
        /// </summary>
        public string EMutualdirection { get; set; }
        /// <summary>
        /// A相故障电弧量报警值
        /// </summary>
        public string EarcAlarmA { get; set; }
        /// <summary>
        /// B相故障电弧量报警值
        /// </summary>
        public string EarcAlarmB { get; set; }
        /// <summary>
        /// C相故障电弧量报警值
        /// </summary>
        public string EarcAlarmC { get; set; }
        /// <summary>
        /// A相故障电弧量实时值
        /// </summary>
        public string EarcrealA { get; set; }
        /// <summary>
        /// B相故障电弧量实时值
        /// </summary>
        public string EarcrealB { get; set; }
        /// <summary>
        /// C相故障电弧量实时值
        /// </summary>
        public string EarcrealC { get; set; }
        /// <summary>
        /// 3相电压平衡度报警值
        /// </summary>
        public string EVoltagebalanceAlarm { get; set; }
        /// <summary>
        /// 3相电压平衡度实时值
        /// </summary>
        public string EVoltagebalanceReal { get; set; }
        /// <summary>
        /// 3相电流平衡度报警值
        /// </summary>
        public string EWaterbalanceAlarm { get; set; }
        /// <summary>
        /// 3相电流平衡度实时值
        /// </summary>
        public string EWaterbalanceReal { get; set; }
        /// <summary>
        /// 信号强度实时值
        /// </summary>
        public string Gpsignal { get; set; }
        /// <summary>
        /// Va电压相位角
        /// </summary>
        public string VaVoltageangle { get; set; }
        /// <summary>
        /// Vb电压相位角
        /// </summary>
        public string VbVoltageangle { get; set; }
        /// <summary>
        /// Vc电压相位角
        /// </summary>
        public string VcVoltageangle { get; set; }
        /// <summary>
        /// Ia电流相位角
        /// </summary>
        public string IaWaterangle { get; set; }
        /// <summary>
        /// Ib电流相位角 
        /// </summary>
        public string IbWaterangle { get; set; }
        /// <summary>
        /// Ic电流相位角
        /// </summary>
        public string IcWaterangle { get; set; }

    }
    /// <summary>
    /// 心跳实体
    /// </summary>
    public class StrongEHeatber
    {
        /// <summary>
        /// 设备号
        /// </summary>
        public string EquipmentNo { get; set; }
        /// <summary>
        /// 在线时间
        /// </summary>
        public string Rtc { get; set; }
    }
}
