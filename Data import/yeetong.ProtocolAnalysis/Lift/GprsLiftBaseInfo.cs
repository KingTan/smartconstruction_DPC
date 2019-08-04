using System;
using System.Collections.Generic;

using System.Text;

namespace ProtocolAnalysis
{
    /// <summary>
    /// 实时数据
    /// </summary>
    [Serializable]
    public class Lift_Current
    {
        #region 属性
        private string _sn = "";
        /// <summary>
        /// 设备编号
        /// </summary>
        public string SN
        {
            get { return _sn; }
            set { _sn = value; }
        }

        private string _cardNo = "";
        /// <summary>
        /// 一卡通卡号
        /// </summary>
        public string CardNo
        {
            get { return _cardNo; }
            set { _cardNo = value; }
        }
        private string _GPS_X = "";
        /// <summary>
        /// GPS_X
        /// </summary>
        public string GPS_X
        {
            get { return _GPS_X; }
            set { _GPS_X = value; }
        }
        private string _GPS_Y = "";
        /// <summary>
        /// GPS_Y
        /// </summary>
        public string GPS_Y
        {
            get { return _GPS_Y; }
            set { _GPS_Y = value; }
        }

        private string _weight = "0";
        /// <summary>
        /// 载重(吨)
        /// </summary>
        public string Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        private string _angle = "0";
        /// <summary>
        /// 转角
        /// </summary>
        public string Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        private string _wind = "0";
        /// <summary>
        /// 风速
        /// </summary>
        public string Wind
        {
            get { return _wind; }
            set { _wind = value; }
        }
        private string _windLevel = "0";
        /// <summary>
        /// 风速等级
        /// </summary>
        public string WindLevel
        {
            get { return _windLevel; }
            set { _windLevel = value; }
        }


        private string _height = "0";
        /// <summary>
        /// 高度
        /// </summary>
        public string Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private string _floors = "0";
        /// <summary>
        /// 楼层
        /// </summary>
        public string Floors
        {
            get { return _floors; }
            set { _floors = value; }
        }

        private string _speed = "0";
        /// <summary>
        /// 速度
        /// </summary>
        public string Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }


        private string _type = "0";
        /// <summary>
        /// 数据类型：0---正常 1---预警 2---报警 3---预报警
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _alarmType = "00000000";
        /// <summary>
        /// 警告码
        /// </summary>
        public string AlarmType
        {
            get { return _alarmType; }
            set { _alarmType = value; }
        }

        private string _angleX = "0";
        /// <summary>
        /// 倾角X轴
        /// </summary>
        public string AngleX
        {
            get { return _angleX; }
            set { _angleX = value; }
        }

        private string _angleY = "0";
        /// <summary>
        /// 倾角Y轴
        /// </summary>
        public string AngleY
        {
            get { return _angleY; }
            set { _angleY = value; }
        }

        private string _rtime = "";
        /// <summary>
        /// 时间
        /// </summary>
        public string Rtime
        {
            get { return _rtime; }
            set { _rtime = value; }
        }
        private string _personNum = "0";
        /// <summary>
        /// 人数
        /// </summary>
        public string personNum
        {
            get { return _personNum; }
            set { _personNum = value; }
        }
        private string _sensorSet = "";
        /// <summary>
        /// 传感器状态
        /// </summary>
        public string SensorSet
        {
            get { return _sensorSet; }
            set { _sensorSet = value; }
        }
        private string powerStatu = "0";
        /// <summary>
        /// 电源状态
        /// </summary>
        public string PowerStatu
        {
            get { return powerStatu; }
            set { powerStatu = value; }
        }

        private string cycleId = "0";
        /// <summary>
        /// 工作循环序列号
        /// </summary>
        public string CycleId
        {
            get { return cycleId; }
            set { cycleId = value; }
        }
        private string rtc = "";
        /// <summary>
        /// 时间
        /// </summary>
        public string Rtc
        {
            get { return rtc; }
            set { rtc = value; }
        }
        private string sensorState = "0";
        /// <summary>
        /// 传感器状态
        /// </summary>
        public string SensorState
        {
            get { return sensorState; }
            set { sensorState = value; }
        }
        private string powerState = "0";
        /// <summary>
        /// 电源状态
        /// </summary>
        public string PowerState
        {
            get { return powerState; }
            set { powerState = value; }
        }
        private string controlState = "0";
        /// <summary>
        /// 控制状态
        /// </summary>
        public string ControlState
        {
            get { return controlState; }
            set { controlState = value; }
        }
        private string gprsSignal = "0";
        /// <summary>
        /// GPRS信号强度
        /// </summary>
        public string GprsSignal
        {
            get { return gprsSignal; }
            set { gprsSignal = value; }
        }
        private string ratedWeight = "0";
        /// <summary>
        /// 当前额定载荷
        /// </summary>
        public string RatedWeight
        {
            get { return ratedWeight; }
            set { ratedWeight = value; }
        }
        private string curScreen = "";
        /// <summary>
        /// 显示屏当前页面
        /// </summary>
        public string CurScreen
        {
            get { return curScreen; }
            set { curScreen = value; }
        }
        private string safeWeight = "0";
        /// <summary>
        /// 当前额定重量
        /// </summary>
        public string SafeWeight
        {
            get { return safeWeight; }
            set { safeWeight = value; }
        }
        private string relayStatus = "0";
        /// <summary>
        /// 继电器状态
        /// </summary>
        public string RelayStatus
        {
            get { return relayStatus; }
            set { relayStatus = value; }
        }
        private string cardId = "0";
        /// <summary>
        /// 身份证号
        /// </summary>
        public string CardId 
        {
            get { return cardId; }
            set { cardId = value; }
        }
        #endregion
        public Lift_Current Clone()
        {
            return (Lift_Current)this.MemberwiseClone();
        }

        /// <summary>
        /// 珠海告警码 上高度预警+上高度报警+下限位报警
        /// </summary>
        public string AlarmCode_ZH
        {
            get;
            set;
        }

        /// <summary>
        /// 重量未经单位换算 （用于珠海等）
        /// </summary>
        public string Weight_NoUnitConversions
        {
            get;
            set;
        }

        public Lift_Current()
        {
            AlarmCode_ZH = "000";
            Weight_NoUnitConversions = "0";
        }
    }

    /// <summary>
    /// 心跳
    /// </summary>
    [Serializable]
    public class Lift_Heartbeat
    {
        public Lift_Heartbeat()
        { }
        #region Model
        private string _sn;
        private string _onlineTime;
        /// <summary>
        /// 设备的序列号
        /// </summary>
        public string SN
        {
            set { _sn = value; }
            get { return _sn; }
        }

        /// <summary>
        /// 在线时间
        /// </summary>
        public string OnlineTime
        {
            set { _onlineTime = value; }
            get { return _onlineTime; }
        }

        #endregion Model
    }

    /// <summary>
    /// 身份验证
    /// </summary>
    public class Lift_Authentication
    {
        #region Model
        private string _sn;
        private string _cardID;
        private string _kardID;
        private string _time;
        private int _status;
        public bool isFace = false;
        private string _name;   //姓名
        private string _code;   //身份证
        private string _cardNo; //卡号
        private string _telephone; //电话
        private string _job;       //职位
        private string _empNo;     //编号
        /// <summary>
        /// 姓名
        /// </summary>
        public string name
        {
            set { _name = value; }
            get { return _name; }
        }
        /// <summary>
        /// 身份证
        /// </summary>
        public string code
        {
            set { _code = value; }
            get { return _code; }
        }
        /// <summary>
        /// 卡号
        /// </summary>
        public string cardNo
        {
            set { _cardNo = value; }
            get { return _cardNo; }
        }
        /// <summary>
        /// 电话
        /// </summary>
        public string telephone
        {
            set { _telephone = value; }
            get { return _telephone; }
        }
        /// <summary>
        /// 职位
        /// </summary>
        public string job
        {
            set { _job = value; }
            get { return _job; }
        }
        /// <summary>
        /// 工号
        /// </summary>
        public string empNo
        {
            set { _empNo = value; }
            get { return _empNo; }
        }

        /// <summary>
        /// 设备的序列号
        /// </summary>
        public string SN
        {
            set { _sn = value; }
            get { return _sn; }
        }
        /// <summary>
        ///司机工号
        /// </summary>
        public string personNum
        {
            set { _cardID = value; }
            get { return _cardID; }
        }
        /// <summary>
        /// 设备IP地址
        /// </summary>
        public string KardID
        {
            set { _kardID = value; }
            get { return _kardID; }
        }

        /// <summary>
        /// 刷卡时间
        /// </summary>
        public string Time
        {
            set { _time = value; }
            get { return _time; }
        }

        /// <summary>
        /// 登录状态
        /// </summary>
        public int Status
        {
            set { _status = value; }
            get { return _status; }
        }

        #endregion Model
    }
    /// <summary>
    /// 召唤参数
    /// </summary>
  [Serializable]
    public class Lift_param
    {
        #region Model
        private string _sn="0";
        private string _nullAD="0";
        private string _standardWeightAD="0";
        private string _famaWeight="0";
        private string _standardWeight="0";
        private string _otherWeight="0";
        private string _cvWeightAlert="0";
        private string _cvWeightAlarm="0";
        private string _valueFloor1="0";
        private string _valueFloor2="0";
        private string _heightF1="0";
        private string _heightF2="0";
        private string _heightF3="0";
        private string _heightF4="0";
        private string _valueF1="0";
        private string _valueF2="0";
        private string _valueF3="0";
        private string _valueF4="0";
        private string _totalFloors="0";
        private string _windAlert="0";
        private string _windAlarm="0";
        private string _hardwareSN="0";
        private string _buyTime="";
        private string _instalTime="0";
        private string _limitType="0";
        private string _hardwareVersion="0";
        private string _softVersion="0";
        private string _pubTime="";

        private string _paramUpdateTime="0";
        private string _identificationWay="0";
        private string _takeSampleValue1="0";
        private string _takeSampleValue2="0";
        private string _takeSampleValue3="0";
        private string _takeSampleValue4="0";
        private string _takeSampleValue5="0";
        private string _totalHeight="0";
        private string _contrastCycle="0";
        private string _superContrastCycle="0";
        private string _period1StartTime="";
        private string _period1EndTime="";
        private string _period1RatedLoad="0";
        private string _period2StartTime="";
        private string _period2EndTime="";
        private string _period2RatedLoad="0";
        private string _period3StartTime="0";
        private string _period3EndTime="";
        private string _period3RatedLoad="0";
        private string _period4StartTime="";
        private string _period4EndTime="";
        private string _period4RatedLoad="0";
        private string _geoCoordinateX="0";
        private string _northOrSouthX="0";
        private string _geoCoordinateY="0";
        private string _eastOrWestY="0";

        ////////////////20160816添加////////////////
        private string _functionConfig = "";
        private string _standardHeavyWeight = "0";
        private string _ratedLoad = "0";
        private string _emptyhookAD = "0";
        private string _emptyhookAD1 = "0";
        private string _emptyhookAD2 = "0";
        private string _standardWeightAD1 = "0";
        private string _standardWeightAD2 = "0";
        private string _oneFloorAD = "0";
        private string _highFloorAD = "0";
        private string _driverContrastCycle = "0";
        private string _limitPersonNum = "0";
        private string _bootLoaderVersion = "";
        ////////////////////////////////////////


        /****************V1.3.0**************/
        //层高5
        public string HeightF5 = "0";
        //层高6
        public string HeightF6 = "0";
        //层高7
        public string HeightF7 = "0";
        //层高8
        public string HeightF8 = "0";
        //层高9
        public string HeightF9 = "0";
        //层高10
        public string HeightF10 = "0";
        //层高11
        public string HeightF11 = "0";
        //层高12
        public string HeightF12 = "0";
        //层高13
        public string HeightF13 = "0";
        //层高14
        public string HeightF14 = "0";
        //层高15
        public string HeightF15 = "0";
        //层高16
        public string HeightF16 = "0";
        //起始楼层设置
        public string StartFloorSet = "0";
        //楼层设置1
        public string FloorSet1 = "0"; //已經有了
        //楼层设置2
        public string FloorSet2 = "0";//已經有了
        //楼层设置3
        public string FloorSet3 = "0";//已經有了
        //楼层设置4
        public string FloorSet4 = "0";//已經有了
        //楼层设置5
        public string FloorSet5 = "0";
        //楼层设置6
        public string FloorSet6 = "0";
        //楼层设置7
        public string FloorSet7 = "0";
        //楼层设置8
        public string FloorSet8 = "0";
        //楼层设置9
        public string FloorSet9 = "0";
        //楼层设置10
        public string FloorSet10 = "0";
        //楼层设置11
        public string FloorSet11 = "0";
        //楼层设置12
        public string FloorSet12 = "0";
        //楼层设置13
        public string FloorSet13 = "0";
        //楼层设置14
        public string FloorSet14 = "0";
        //楼层设置15
        public string FloorSet15 = "0";
        //楼层设置16
        public string FloorSet16 = "0";
        /************************************/
        /// <summary>
        /// 设备编号
        /// </summary>
        public string SN
        {
            set { _sn = value; }
            get { return _sn; }
        }
        /// <summary>
        /// 空载AD采样值
        /// </summary>
        public string NullAD
        {
            set { _nullAD = value; }
            get { return _nullAD; }
        }
        /// <summary>
        /// 标准重量AD采样值
        /// </summary>
        public string StandardWeightAD
        {
            set { _standardWeightAD = value; }
            get { return _standardWeightAD; }
        }
        /// <summary>
        /// 标准砝码重量
        /// </summary>
        public string FamaWeight
        {
            set { _famaWeight = value; }
            get { return _famaWeight; }
        }
        /// <summary>
        /// 额定重量
        /// </summary>
        public string StandardWeight
        {
            set { _standardWeight = value; }
            get { return _standardWeight; }
        }
        /// <summary>
        /// 偏载重量
        /// </summary>
        public string OtherWeight
        {
            set { _otherWeight = value; }
            get { return _otherWeight; }
        }
        /// <summary>
        /// 重量预警系数
        /// </summary>
        public string CvWeightAlert
        {
            set { _cvWeightAlert = value; }
            get { return _cvWeightAlert; }
        }
        /// <summary>
        /// 重量报警系数
        /// </summary>
        public string CvWeightAlarm
        {
            set { _cvWeightAlarm = value; }
            get { return _cvWeightAlarm; }
        }
        /// <summary>
        /// 1楼采样值
        /// </summary>
        public string ValueFloor1
        {
            set { _valueFloor1 = value; }
            get { return _valueFloor1; }
        }
        /// <summary>
        /// 2楼采样值
        /// </summary>
        public string ValueFloor2
        {
            set { _valueFloor2 = value; }
            get { return _valueFloor2; }
        }
        /// <summary>
        /// F1 楼层高
        /// </summary>
        public string HeightF1
        {
            set { _heightF1 = value; }
            get { return _heightF1; }
        }
        /// <summary>
        /// F2 楼层高
        /// </summary>
        public string HeightF2
        {
            set { _heightF2 = value; }
            get { return _heightF2; }
        }
        /// <summary>
        /// F3 楼层高
        /// </summary>
        public string HeightF3
        {
            set { _heightF3 = value; }
            get { return _heightF3; }
        }
        /// <summary>
        /// F4 楼层高
        /// </summary>
        public string HeightF4
        {
            set { _heightF4 = value; }
            get { return _heightF4; }
        }
        /// <summary>
        /// F1 楼值
        /// </summary>
        public string ValueF1
        {
            set { _valueF1 = value; }
            get { return _valueF1; }
        }
        /// <summary>
        /// F2 楼值
        /// </summary>
        public string ValueF2
        {
            set { _valueF2 = value; }
            get { return _valueF2; }
        }
        /// <summary>
        /// F3 楼值
        /// </summary>
        public string ValueF3
        {
            set { _valueF3 = value; }
            get { return _valueF3; }
        }
        /// <summary>
        /// F4 楼值
        /// </summary>
        public string ValueF4
        {
            set { _valueF4 = value; }
            get { return _valueF4; }
        }
        /// <summary>
        /// 总楼层数
        /// </summary>
        public string TotalFloors
        {
            set { _totalFloors = value; }
            get { return _totalFloors; }
        }
        /// <summary>
        /// 风速预警值
        /// </summary>
        public string WindAlert
        {
            set { _windAlert = value; }
            get { return _windAlert; }
        }
        /// <summary>
        /// 风速报警值
        /// </summary>
        public string WindAlarm
        {
            set { _windAlarm = value; }
            get { return _windAlarm; }
        }
        /// <summary>
        ///  设备物理编号
        /// </summary>
        public string HardwareSN
        {
            set { _hardwareSN = value; }
            get { return _hardwareSN; }
        }
        /// <summary>
        ///  出厂时间
        /// </summary>
        public string BuyTime
        {
            set { _buyTime = value; }
            get { return _buyTime; }
        }
        /// <summary>
        ///  安装时间
        /// </summary>
        public string InstalTime
        {
            set { _instalTime = value; }
            get { return _instalTime; }
        }
        /// <summary>
        /// 限位配置
        /// </summary>
        public string LimitType
        {
            set { _limitType = value; }
            get { return _limitType; }
        }
        /// <summary>
        /// 硬件版本号
        /// </summary>
        public string HardwareVersion
        {
            set { _hardwareVersion = value; }
            get { return _hardwareVersion; }
        }
        /// <summary>
        ///  软件版号
        /// </summary>
        public string SoftVersion
        {
            set { _softVersion = value; }
            get { return _softVersion; }
        }
        /// <summary>
        ///  召唤时间
        /// </summary>
        public string PubTime
        {
            set { _pubTime = value; }
            get { return _pubTime; }
        }
        /// <summary>
        /// 时间
        /// </summary>
        public string ParamUpdateTime
        {
            set { _paramUpdateTime = value; }
            get { return _paramUpdateTime; }
        }
        /// <summary>
        /// 身份认证方式
        /// </summary>
        public string IdentificationWay
        {
            set { _identificationWay = value; }
            get { return _identificationWay; }
        }

        /// <summary>
        /// 采样值 1
        /// </summary>
        public string TakeSampleValue1
        {
            set { _takeSampleValue1 = value; }
            get { return _takeSampleValue1; }
        }
        /// <summary>
        /// 采样值 2
        /// </summary>
        public string TakeSampleValue2
        {
            set { _takeSampleValue2 = value; }
            get { return _takeSampleValue2; }
        }
        /// <summary>
        /// 采样值 3
        /// </summary>
        public string TakeSampleValue3
        {
            set { _takeSampleValue3 = value; }
            get { return _takeSampleValue3; }
        }
        /// <summary>
        /// 采样值 4
        /// </summary>
        public string TakeSampleValue4
        {
            set { _takeSampleValue4 = value; }
            get { return _takeSampleValue4; }
        }
        /// <summary>
        /// 采样值 5
        /// </summary>
        public string TakeSampleValue5
        {
            set { _takeSampleValue5 = value; }
            get { return _takeSampleValue5; }
        }
        /// <summary>
        /// 总高度
        /// </summary>
        public string TotalHeight
        {
            set { _totalHeight = value; }
            get { return _totalHeight; }
        }
        /// <summary>
        /// 对比周期
        /// </summary>
        public string ContrastCycle
        {
            set { _contrastCycle = value; }
            get { return _contrastCycle; }
        }
        /// <summary>
        /// 监理对比周期
        /// </summary>
        public string SuperContrastCycle
        {
            set { _superContrastCycle = value; }
            get { return _superContrastCycle; }
        }
        /// <summary>
        /// 时段 1 起始时间
        /// </summary>
        public string Period1StartTime
        {
            set { _period1StartTime = value; }
            get { return _period1StartTime; }
        }
        /// <summary>
        /// 时段 1 结束时间
        /// </summary>
        public string Period1EndTime
        {
            set { _period1EndTime = value; }
            get { return _period1EndTime; }
        }
        /// <summary>
        /// 时段 1 额定载荷
        /// </summary>
        public string Period1RatedLoad
        {
            set { _period1RatedLoad = value; }
            get { return _period1RatedLoad; }
        }
        /// <summary>
        /// 时段2 起始时间
        /// </summary>
        public string Period2StartTime
        {
            set { _period2StartTime = value; }
            get { return _period2StartTime; }
        }
        /// <summary>
        /// 时段2 结束时间
        /// </summary>
        public string Period2EndTime
        {
            set { _period2EndTime = value; }
            get { return _period2EndTime; }
        }
        /// <summary>
        /// 时段2 载荷时间
        /// </summary>
        public string Period2RatedLoad
        {
            set { _period2RatedLoad = value; }
            get { return _period2RatedLoad; }
        }
        /// <summary>
        /// 时段3 起始时间
        /// </summary>
        public string Period3StartTime
        {
            set { _period3StartTime = value; }
            get { return _period3StartTime; }
        }
        /// <summary>
        /// 时段3 结束时间
        /// </summary>
        public string Period3EndTime
        {
            set { _period3EndTime = value; }
            get { return _period3EndTime; }
        }
        /// <summary>
        /// 时段3 载荷时间
        /// </summary>
        public string Period3RatedLoad
        {
            set { _period3RatedLoad = value; }
            get { return _period3RatedLoad; }
        }
        /// <summary>
        /// 时段4 起始时间
        /// </summary>
        public string Period4StartTime
        {
            set { _period4StartTime = value; }
            get { return _period4StartTime; }

        }
        /// <summary>
        /// 时段4 结束时间
        /// </summary>
        public string Period4EndTime
        {
            set { _period4EndTime = value; }
            get { return _period4EndTime; }
        }
        /// <summary>
        /// 时段4 载荷时间
        /// </summary>
        public string Period4RatedLoad
        {
            set { _period4RatedLoad = value; }
            get { return _period4RatedLoad; }
        }
        /// <summary>
        /// 纬度
        /// </summary>
        public string GeoCoordinateX
        {
            set { _geoCoordinateX = value; }
            get { return _geoCoordinateX; }
        }
        /// <summary>
        /// 北纬 or 南纬
        /// </summary>
        public string NorthOrSouthX
        {
            set { _northOrSouthX = value; }
            get { return _northOrSouthX; }
        }
        /// <summary>
        /// 经度
        /// </summary>
        public string GeoCoordinateY
        {
            set { _geoCoordinateY = value; }
            get { return _geoCoordinateY; }
        }
        /// <summary>
        /// 东经 or 西经
        /// </summary>
        public string EastOrWestY
        {
            set { _eastOrWestY = value; }
            get { return _eastOrWestY; }
        }

        /// <summary>
        /// 功能配置
        /// </summary>
        public string FunctionConfig
        {
            set { _functionConfig = value; }
            get { return _functionConfig; }
        }
        /// <summary>
        /// 标准重物重量
        /// </summary>
        public string StandardHeavyWeight
        {
            set { _standardHeavyWeight = value; }
            get { return _standardHeavyWeight; }
        }
        /// <summary>
        /// 额定载荷
        /// </summary>
        public string RatedLoad
        {
            set { _ratedLoad = value; }
            get { return _ratedLoad; }
        }
        /// <summary>
        /// 空钩时AD采样值
        /// </summary>
        public string EmptyhookAD
        {
            set { _emptyhookAD = value; }
            get { return _emptyhookAD; }
        }
        /// <summary>
        /// 空钩时AD采样值1
        /// </summary>
        public string EmptyhookAD1
        {
            set { _emptyhookAD1 = value; }
            get { return _emptyhookAD1; }
        }
        /// <summary>
        /// 空钩时AD采样值2
        /// </summary>
        public string EmptyhookAD2
        {
            set { _emptyhookAD2 = value; }
            get { return _emptyhookAD2; }
        }
        /// <summary>
        /// 标准重物采样值 1
        /// </summary>
        public string StandardWeightAD1
        {
            set { _standardWeightAD1 = value; }
            get { return _standardWeightAD1; }
        }
        /// <summary>
        /// 标准重物采样值 2
        /// </summary>
        public string StandardWeightAD2
        {
            set { _standardWeightAD2 = value; }
            get { return _standardWeightAD2; }
        }
        /// <summary>
        /// 1 楼楼层采样值
        /// </summary>
        public string OneFloorAD
        {
            set { _oneFloorAD = value; }
            get { return _oneFloorAD; }
        }
        /// <summary>
        /// 最高层楼层采样值
        /// </summary>
        public string HighFloorAD
        {
            set { _highFloorAD = value; }
            get { return _highFloorAD; }
        }
        /// <summary>
        /// 司机身份对比周期
        /// </summary>
        public string DriverContrastCycle
        {
            set { _driverContrastCycle = value; }
            get { return _driverContrastCycle; }
        }
        /// <summary>
        /// 限载人数
        /// </summary>
        public string LimitPersonNum
        {
            set { _limitPersonNum = value; }
            get { return _limitPersonNum; }
        }
        /// <summary>
        /// bootloader 版本号
        /// </summary>
        public string BootLoaderVersion
        {
            set { _bootLoaderVersion = value; }
            get { return _bootLoaderVersion; }
        }
        #endregion Model

        public Lift_param()
        {
            _sn = "";
            _nullAD = "";
            _standardWeightAD = "";
            _famaWeight = "";
            _standardWeight = "";
            _otherWeight = "";
            _cvWeightAlert = "";
            _cvWeightAlarm = "";
            _valueFloor1 = "";
            _valueFloor2 = "";
            _heightF1 = "";
            _heightF2 = "";
            _heightF3 = "";
            _heightF4 = "";
            _valueF1 = "";
            _valueF2 = "";
            _valueF3 = "";
            _valueF4 = "";
            _totalFloors = "";
            _windAlert = "";
            _windAlarm = "";
            _hardwareSN = "";
            _buyTime = "";
            _instalTime = "";
            _limitType = "";
            _hardwareVersion = "";
            _softVersion = "";
            _pubTime = "";

            _paramUpdateTime = "";
            _identificationWay = "";
            _takeSampleValue1 = "";
            _takeSampleValue2 = "";
            _takeSampleValue3 = "";
            _takeSampleValue4 = "";
            _takeSampleValue5 = "";
            _totalHeight = "";
            _contrastCycle = "";
            _superContrastCycle = "";
            _period1StartTime = "";
            _period1EndTime = "";
            _period1RatedLoad = "";
            _period2StartTime = "";
            _period2EndTime = "";
            _period2RatedLoad = "";
            _period3StartTime = "";
            _period3EndTime = "";
            _period3RatedLoad = "";
            _period4StartTime = "";
            _period4EndTime = "";
            _period4RatedLoad = "";
            _geoCoordinateX = "";
            _northOrSouthX = "";
            _geoCoordinateY = "";
            _eastOrWestY = ""; 
        }
    }

    /// <summary>
    /// 违规运行帧
    /// </summary>
       [Serializable]
    public class Lift_IllegalOperation
    {
        /// <summary>
        /// 设备的序列号
        /// </summary>
        public string SN
        {set ;get ;}

        /// <summary>
        /// 违规运行标识
        /// </summary>
        public byte IllegalOperationIdentifier
        { set; get; }
    }

    /// <summary>
    /// 设备运行时间
    /// </summary>
    [Serializable]
       public class Lift_RuntimeEp
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 总运行时间
        /// </summary>
        public uint TotalRuntime { get; set; }
        /// <summary>
        /// 本次开机运行时间
        /// </summary>
        public uint StartingUpRuntime { get; set; }
        /// <summary>
        /// 预留
        /// </summary>
        public object obligate { get; set; }
    }

}

public class LiftFeature
{
    /// <summary>
    /// 设备号
    /// </summary>
    public string equipmentNo { get; set; }
    /// <summary>
    /// 用户id
    /// </summary>
    public string userid { get; set; }
    /// <summary>
    /// 总包数
    /// </summary>
    public string TotalPack { get; set; }
    /// <summary>
    /// 当前包
    /// </summary>
    public string CurrentPack { get; set; }
    /// <summary>
    /// 特征码
    /// </summary>
    public string FeaturePack { get; set; }
    /// <summary>
    /// 总包
    /// </summary>
    public string SumFeaturePack { get; set; }
}
[Serializable]
public class LiftFeatureIssued
{
    public string equipmentNo { get; set; }
    public string userid { get; set; }
    public int TotalPack { get; set; }
    public int CurrentPack { get; set; }
    public string SumFeaturePack { get; set; }
   
}