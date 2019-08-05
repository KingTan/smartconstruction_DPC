using System;
using System.Collections.Generic;
using System.Text;

namespace ProtocolAnalysis.TowerCrane.OE
{
    /// <summary>
    /// 实时数据
    /// </summary>
    [Serializable]
    public class CraneCurrent
    {
        #region 属性
        private string craneno = "";
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Craneno
        {
            get { return craneno; }
            set { craneno = value; }
        }

        private string card = "00000000";
        /// <summary>
        /// 身份认证号
        /// </summary>
        public string Card
        {
            get { return card; }
            set { card = value; }
        }

        private string weight = "0.00";
        /// <summary>
        /// 载重(吨)
        /// </summary>
        public string Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        private string angle = "0";
        /// <summary>
        /// 转角
        /// </summary>
        public string Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        private string wind = "0.00";
        /// <summary>
        /// 风速
        /// </summary>
        public string Wind
        {
            get { return wind; }
            set { wind = value; }
        }
        private string windLevel = "0";
        /// <summary>
        /// 风速等级
        /// </summary>
        public string WindLevel
        {
            get { return windLevel; }
            set { windLevel = value; }
        }
        private string height = "0.00";
        /// <summary>
        /// 高度
        /// </summary>
        public string Height
        {
            get { return height; }
            set { height = value; }
        }

        private string radius = "0.00";
        /// <summary>
        /// 幅度
        /// </summary>
        public string Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        /*ZT20160923添加的力矩属性*/
        private string torque = "0.00";
        /// <summary>
        /// 力矩百分比 起重量和工作幅度的乘积来表示其起重能力
        /// </summary>
        public string Torque
        {
            get { return torque; }
            set { torque = value; }
        }


        private string torquepercent = "0.00";
        /// <summary>
        /// 力矩百分比
        /// </summary>
        public string Torquepercent
        {
            get { return torquepercent; }
            set { torquepercent = value; }
        }

        private string safetorque = "0.00";
        /// <summary>
        /// 安全力矩
        /// </summary>
        public string Safetorque
        {
            get { return safetorque; }
            set { safetorque = value; }
        }
        private string safeWeight = "0.00";
        /// <summary>
        /// 安全起重量（t）
        /// </summary>
        public string SafeWeight
        {
            get { return safeWeight; }
            set { safeWeight = value; }
        }
        private string type = "0";
        /// <summary>
        /// 数据类型：0---正常 1---预警 2---报警 3---预报警
        /// </summary>
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        private string alarmType = "00000000000000000000000000000000";
        /// <summary>
        /// 报警告码
        /// </summary>
        public string AlarmType
        {
            get { return alarmType; }
            set { alarmType = value; }
        }
        private string warnType = "00000000000000000000000000000000";
        /// <summary>
        /// 预警告码
        /// </summary>
        public string WarnType
        {
            get { return warnType; }
            set { warnType = value; }
        }
        private string sensorStatus = "0000000000000000";
        /// <summary>
        /// 传感器状态
        /// </summary>
        public string SensorStatus
        {
            get { return sensorStatus; }
            set { sensorStatus = value; }
        }
        private string limitStatus = "0000000000000000";
        /// <summary>
        /// 限位控制器状态
        /// </summary>
        public string LimitStatus
        {
            get { return limitStatus; }
            set { limitStatus = value; }
        }
        private string angleX = "0.00";
        /// <summary>
        /// 倾角X轴
        /// </summary>
        public string AngleX
        {
            get { return angleX; }
            set { angleX = value; }
        }
        private string longitude = "0.00";
        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }
        private string latitude = "0.00";
        /// <summary>
        /// 纬度
        /// </summary>
        public string Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }
        private string angleY = "0.00";
        /// <summary>
        /// 倾角Y轴
        /// </summary>
        public string AngleY
        {
            get { return angleY; }
            set { angleY = value; }
        }

        private string rtime = "";
        /// <summary>
        /// 时间
        /// </summary>
        public string Rtime
        {
            get { return rtime; }
            set { rtime = value; }
        }
        private string times = "0";
        /// <summary>
        /// 倍率
        /// </summary>
        public string Times
        {
            get { return times; }
            set { times = value; }
        }
        private string inAlarm = "0";
        /// <summary>
        /// 小车内限位报警
        /// </summary>
        public string InAlarm
        {
            get { return inAlarm; }
            set { inAlarm = value; }
        }
        private string outAlarm = "0";
        /// <summary>
        /// 小车外限位报警
        /// </summary>
        public string OutAlarm
        {
            get { return outAlarm; }
            set { outAlarm = value; }
        }
        private string upAlarm = "0";
        /// <summary>
        /// 上升限位报警
        /// </summary>
        public string UpAlarm
        {
            get { return upAlarm; }
            set { upAlarm = value; }
        }
        private string downAlarm = "0";
        /// <summary>
        /// 下降限位报警
        /// </summary>
        public string DownAlarm
        {
            get { return downAlarm; }
            set { downAlarm = value; }
        }

        private string leftAlarm = "0";
        /// <summary>
        /// 左转限位报警
        /// </summary>
        public string LeftAlarm
        {
            get { return leftAlarm; }
            set { leftAlarm = value; }
        }

        private string rightAlarm = "0";
        /// <summary>
        /// 右转限位报警
        /// </summary>
        public string RightAlarm
        {
            get { return rightAlarm; }
            set { rightAlarm = value; }
        }
        private string inAlarm_Area = "0";
        /// <summary>
        /// 前进进入禁止区域报警
        /// </summary>
        public string InAlarm_Area
        {
            get { return inAlarm_Area; }
            set { inAlarm_Area = value; }
        }
        private string outAlarm_Area = "0";
        /// <summary>
        /// 后退进入禁止区域报警
        /// </summary>
        public string OutAlarm_Area
        {
            get { return outAlarm_Area; }
            set { outAlarm_Area = value; }
        }
        private string leftAlarm_Area = "0";
        /// <summary>
        /// 左转进入禁止区域报警
        /// </summary>
        public string LeftAlarm_Area
        {
            get { return leftAlarm_Area; }
            set { leftAlarm_Area = value; }
        }

        private string rightAlarm_Area = "0";
        /// <summary>
        /// 右转进入禁止区域报警
        /// </summary>
        public string RightAlarm_Area
        {
            get { return rightAlarm_Area; }
            set { rightAlarm_Area = value; }
        }
        private string inAlarm_Hit = "0";
        /// <summary>
        /// 前进多机防碰撞报警
        /// </summary>
        public string InAlarm_Hit
        {
            get { return inAlarm_Hit; }
            set { inAlarm_Hit = value; }
        }
        private string outAlarm_Hit = "0";
        /// <summary>
        /// 后退多机防碰撞报警
        /// </summary>
        public string OutAlarm_Hit
        {
            get { return outAlarm_Hit; }
            set { outAlarm_Hit = value; }
        }
        private string leftAlarm_Hit = "0";
        /// <summary>
        /// 左转多机防碰撞报警
        /// </summary>
        public string LeftAlarm_Hit
        {
            get { return leftAlarm_Hit; }
            set { leftAlarm_Hit = value; }
        }

        private string rightAlarm_Hit = "0";
        /// <summary>
        /// 右转多机防碰撞报警
        /// </summary>
        public string RightAlarm_Hit
        {
            get { return rightAlarm_Hit; }
            set { rightAlarm_Hit = value; }
        }
        private string weightAlarm = "0";
        /// <summary>
        /// 吊重报警
        /// </summary>
        public string WeightAlarm
        {
            get { return weightAlarm; }
            set { weightAlarm = value; }
        }
        /// <summary>
        /// 力矩报警
        /// </summary>
        public string TorqueAlarm
        {
            get { return weightAlarm; }
            set { weightAlarm = value; }
        }
        private string hitAlarm = "0";
        /// <summary>
        /// 碰撞报警
        /// </summary>
        public string HitAlarm
        {
            get { return hitAlarm; }
            set { hitAlarm = value; }
        }
        private string windAlarm = "0";
        /// <summary>
        /// 风速报警
        /// </summary>
        public string WindAlarm
        {
            get { return windAlarm; }
            set { windAlarm = value; }
        }
        private string angleAlarm = "0";
        /// <summary>
        /// 倾斜报警
        /// </summary>
        public string AngleAlarm
        {
            get { return angleAlarm; }
            set { angleAlarm = value; }
        }
        private string inAlarm_Warn = "0";
        /// <summary>
        /// 小车内限位预警
        /// </summary>
        public string InAlarm_Warn
        {
            get { return inAlarm_Warn; }
            set { inAlarm_Warn = value; }
        }
        private string outAlarm_Warn = "0";
        /// <summary>
        /// 小车外限位预警
        /// </summary>
        public string OutAlarm_Warn
        {
            get { return outAlarm_Warn; }
            set { outAlarm_Warn = value; }
        }
        private string upAlarm_Warn = "0";
        /// <summary>
        /// 上升限位预警
        /// </summary>
        public string UpAlarm_Warn
        {
            get { return upAlarm_Warn; }
            set { upAlarm_Warn = value; }
        }
        private string downAlarm_Warn = "0";
        /// <summary>
        /// 下降限位预警
        /// </summary>
        public string DownAlarm_Warn
        {
            get { return downAlarm_Warn; }
            set { downAlarm_Warn = value; }
        }

        private string leftAlarm_Warn = "0";
        /// <summary>
        /// 左转限位预警
        /// </summary>
        public string LeftAlarm_Warn
        {
            get { return leftAlarm_Warn; }
            set { leftAlarm_Warn = value; }
        }

        private string rightAlarm_Warn = "0";
        /// <summary>
        /// 右转限位预警
        /// </summary>
        public string RightAlarm_Warn
        {
            get { return rightAlarm_Warn; }
            set { rightAlarm_Warn = value; }
        }
        private string inAlarm_Area_Warn = "0";
        /// <summary>
        /// 前进进入禁止区域预警
        /// </summary>
        public string InAlarm_Area_Warn
        {
            get { return inAlarm_Area_Warn; }
            set { inAlarm_Area_Warn = value; }
        }
        private string outAlarm_Area_Warn = "0";
        /// <summary>
        /// 后退进入禁止区域预警
        /// </summary>
        public string OutAlarm_Area_Warn
        {
            get { return outAlarm_Area_Warn; }
            set { outAlarm_Area_Warn = value; }
        }
        private string leftAlarm_Area_Warn = "0";
        /// <summary>
        /// 左转进入禁止区域预警
        /// </summary>
        public string LeftAlarm_Area_Warn
        {
            get { return leftAlarm_Area_Warn; }
            set { leftAlarm_Area_Warn = value; }
        }

        private string rightAlarm_Area_Warn = "0";
        /// <summary>
        /// 右转进入禁止区域预警
        /// </summary>
        public string RightAlarm_Area_Warn
        {
            get { return rightAlarm_Area_Warn; }
            set { rightAlarm_Area_Warn = value; }
        }
        private string inAlarm_Hit_Warn = "0";
        /// <summary>
        /// 前进多机防碰撞预警
        /// </summary>
        public string InAlarm_Hit_Warn
        {
            get { return inAlarm_Hit_Warn; }
            set { inAlarm_Hit_Warn = value; }
        }
        private string outAlarm_Hit_Warn = "0";
        /// <summary>
        /// 后退多机防碰撞预警
        /// </summary>
        public string OutAlarm_Hit_Warn
        {
            get { return outAlarm_Hit_Warn; }
            set { outAlarm_Hit_Warn = value; }
        }
        private string leftAlarm_Hit_Warn = "0";
        /// <summary>
        /// 左转多机防碰撞预警
        /// </summary>
        public string LeftAlarm_Hit_Warn
        {
            get { return leftAlarm_Hit_Warn; }
            set { leftAlarm_Hit_Warn = value; }
        }

        private string rightAlarm_Hit_Warn = "0";
        /// <summary>
        /// 右转多机防碰撞预警
        /// </summary>
        public string RightAlarm_Hit_Warn
        {
            get { return rightAlarm_Hit_Warn; }
            set { rightAlarm_Hit_Warn = value; }
        }
        private string weightAlarm_Warn = "0";
        /// <summary>
        /// 吊重预警
        /// </summary>
        public string WeightAlarm_Warn
        {
            get { return weightAlarm_Warn; }
            set { weightAlarm_Warn = value; }
        }
        /// <summary>
        /// 力矩预警
        /// </summary>
        public string TorqueAlarm_Warn
        {
            get { return weightAlarm_Warn; }
            set { weightAlarm_Warn = value; }
        }
        private string hitAlarm_Warn = "0";
        /// <summary>
        /// 碰撞预警
        /// </summary>
        public string HitAlarm_Warn
        {
            get { return hitAlarm_Warn; }
            set { hitAlarm_Warn = value; }
        }
        private string windAlarm_Warn = "0";
        /// <summary>
        /// 风速预警
        /// </summary>
        public string WindAlarm_Warn
        {
            get { return windAlarm_Warn; }
            set { windAlarm_Warn = value; }
        }
        private string angleAlarm_Warn = "0";
        /// <summary>
        /// 倾斜预警
        /// </summary>
        public string AngleAlarm_Warn
        {
            get { return angleAlarm_Warn; }
            set { angleAlarm_Warn = value; }
        }
        private string limitWindStatue = "0";
        /// <summary>
        /// 风速报警限位状态
        /// </summary>
        public string LimitWindStatue
        {
            get { return limitWindStatue; }
            set { limitWindStatue = value; }
        }

        private string limitWindStatue_sub = "0";
        /// <summary>
        /// 风速预警限位状态
        /// </summary>
        public string LimitWindStatue_sub
        {
            get { return limitWindStatue_sub; }
            set { limitWindStatue_sub = value; }
        }
        private string limitInStatue = "0";
        /// <summary>
        /// 幅度内限位状态
        /// </summary>
        public string LimitInStatue
        {
            get { return limitInStatue; }
            set { limitInStatue = value; }
        }

        private string limitInStatue_sub = "0";
        /// <summary>
        /// 幅度内换速状态
        /// </summary>
        public string LimitInStatue_sub
        {
            get { return limitInStatue_sub; }
            set { limitInStatue_sub = value; }
        }

        private string limitOutStatue = "0";
        /// <summary>
        /// 幅度外限位状态
        /// </summary>
        public string LimitOutStatue
        {
            get { return limitOutStatue; }
            set { limitOutStatue = value; }
        }

        private string limitOutStatue_sub = "0";
        /// <summary>
        /// 幅度外预减速状态
        /// </summary>
        public string LimitOutStatue_sub
        {
            get { return limitOutStatue_sub; }
            set { limitOutStatue_sub = value; }
        }

        private string limitUpStatue = "0";
        /// <summary>
        /// 高度上限位状态
        /// </summary>
        public string LimitUpStatue
        {
            get { return limitUpStatue; }
            set { limitUpStatue = value; }
        }

        private string limitUpStatue_sub = "0";
        /// <summary>
        /// 高度上限位减速状态
        /// </summary>
        public string LimitUpStatue_sub
        {
            get { return limitUpStatue_sub; }
            set { limitUpStatue_sub = value; }
        }

        private string limitDownStatue = "0";
        /// <summary>
        /// 高度下限位状态
        /// </summary>
        public string LimitDownStatue
        {
            get { return limitDownStatue; }
            set { limitDownStatue = value; }
        }

        private string limitDownStatue_sub = "0";
        /// <summary>
        /// 高度下限位换速状态
        /// </summary>
        public string LimitDownStatue_sub
        {
            get { return limitDownStatue_sub; }
            set { limitDownStatue_sub = value; }
        }

        private string limitLeftStatue = "0";
        /// <summary>
        /// 回转左限位状态
        /// </summary>
        public string LimitLeftStatue
        {
            get { return limitLeftStatue; }
            set { limitLeftStatue = value; }
        }

        private string limitLeftStatue_sub = "0";
        /// <summary>
        /// 回转左限位减速状态
        /// </summary>
        public string LimitLeftStatue_sub
        {
            get { return limitLeftStatue_sub; }
            set { limitLeftStatue_sub = value; }
        }

        private string limitRightStatue = "0";
        /// <summary>
        /// 回转右限位状态
        /// </summary>
        public string LimitRightStatue
        {
            get { return limitRightStatue; }
            set { limitRightStatue = value; }
        }

        private string limitRightStatue_sub = "0";
        /// <summary>
        /// 回转右限位减速状态
        /// </summary>
        public string LimitRightStatue_sub
        {
            get { return limitRightStatue_sub; }
            set { limitRightStatue_sub = value; }
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

        private string workCircle = "0";
        /// <summary>
        /// 工作循环序列号
        /// </summary>
        public string WorkCircle
        {
            get { return workCircle; }
            set { workCircle = value; }
        }

        private string cardNo = "0";
        /// <summary>
        /// 司机工号
        /// </summary>
        public string CardNo
        {
            get { return cardNo; }
            set { cardNo = value; }
        }

        private string rTC = "0";
        /// <summary>
        /// RTC
        /// </summary>
        public string RTC
        {
            get { return rTC; }
            set { rTC = value; }
        }

        private string armAngle = "0.00";
        /// <summary>
        /// 动臂俯仰角
        /// </summary>
        public string ArmAngle
        {
            get { return armAngle; }
            set { armAngle = value; }
        }

        private string distance = "0.00";
        /// <summary>
        /// 行程
        /// </summary>
        public string Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        private string safeRadius = "0.00";
        /// <summary>
        /// 安全幅度
        /// </summary>
        public string SafeRadius
        {
            get { return safeRadius; }
            set { safeRadius = value; }
        }

        private string zigBee = "0";
        /// <summary>
        /// zigBee状态
        /// </summary>
        public string ZigBee
        {
            get { return zigBee; }
            set { zigBee = value; }
        }

        private string modelStatus = "0";
        /// <summary>
        /// 模块状态
        /// </summary>
        public string ModelStatus
        {
            get { return modelStatus; }
            set { modelStatus = value; }
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

        private string craneScreen = "0";
        /// <summary>
        /// 设备显示
        /// </summary>
        public string CraneScreen
        {
            get { return craneScreen; }
            set { craneScreen = value; }
        }

        private string setInfo = "0";
        /// <summary>
        /// 可设置信息
        /// </summary>
        public string SetInfo
        {
            get { return setInfo; }
            set { setInfo = value; }
        }
        #endregion
        public CraneCurrent Clone()
        {
            return (CraneCurrent)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// 心跳
    /// </summary>
    [Serializable]
    public class Heartbeat
    {
        public Heartbeat()
        { }
        #region Model
        private string _sn;
        private string _ip;
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
        /// 设备IP地址
        /// </summary>
        public string IP
        {
            set { _ip = value; }
            get { return _ip; }
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
    [Serializable]
    public class Authentication
    {
        #region Model
        private string _sn;
        private string _kardID;
        private string _onlineTime;
        private int _status;
        public bool isFace = false;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string SN
        {
            set { _sn = value; }
            get { return _sn; }
        }

        /// <summary>
        /// 卡号
        /// </summary>
        public string KardID
        {
            set { _kardID = value; }
            get { return _kardID; }
        }

        /// <summary>
        /// 在线时间
        /// </summary>
        public string OnlineTime
        {
            set { _onlineTime = value; }
            get { return _onlineTime; }
        }

        /// <summary>
        /// 登录状态
        /// </summary>
        public int Status
        {
            set { _status = value; }
            get { return _status; }
        }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDCard
        {
            set;
            get;
        }
        /// <summary>
        /// 工号
        /// </summary>
        public string empNo
        {
            set;
            get;
        }
        #endregion Model
    }

    /// <summary>
    ///召唤参数
    /// </summary>
   [Serializable]
    public class CraneConfig
    {
        #region Model
        private int _id;
        private string _craneno;
        private string _ip;
        private string _port;
        private string _softversion;
        private string _highcaltparam;
        private string _amplitudecaltparam;
        private string _hoistcaltparam;
        private string _collisionchannelno;
        private string _momentcaltparam;
        private string _ratio;
        private string _remark;
        private string _minhighad;
        private string _maxhighad;
        private string _standardscale;
        private string _minamplitude;
        private string _minamplitudead;
        private string _maxamplitude;
        private string _maxamplitudead;
        private string _emptyhookad;
        private string _loadweightad;
        private string _farmarweight;
        private string _rotarytype;
        private string _absturndirection;
        private string _absturnvalue;
        private string _absturnpointvalue;
        private string _potleftlimitad;
        private string _potrightlimitad;
        private string _potlimitangle;
        private string _liftweight4ratio;
        private string _liftweightrange4r;
        private string _maxrange4ratio;
        private string _maxrangeweight4r;
        private string _liftweight2ratio;
        private string _liftweightrange2r;
        private string _maxrange2ratio;
        private string _maxrangeweight2r;
        private string _zigbeelocalno;
        private string _zigbeechannelno;
        private string _zigbeegroupno;
        private string _anticollisionx;
        private string _anticollisiony;
        private string _liftweightarmlenght;
        private string _balancearmlenght;
        private string _towerheight;
        private string _toweratheight;
        private string _ampreductionvalue;
        private string _amprestrictvalue;
        private string _highreductionvalue;
        private string _highrestrictvalue;
        private string _turnreducionvalue;
        private string _turnrestrictvalue;
        private string _areareductionvalue;
        private string _arearestrictvalue;
        private string _acreductionvalue;
        private string _acrestrictvalue;
        private string _throwovertorque;
        private string _cuttorque;
        private string _throwoverweight;
        private string _cutweight;
        private string _cfstate;
        private DateTime? _cftime;
        private string _setTime;

        ///////////////////////新协议添加对象/////////////////////////////////
        private string _RTC;
        private string _rotarySet;
        private string _windUnit;
        private string _windWarn;
        private string _windAlarm;
        private string _angleFrontWarn;
        private string _angleFrontAlarm;
        private string _angleBackWarn;
        private string _angleBackAlarm;
        private string _angleLeftWarn;
        private string _angleLeftAlarm;
        private string _angleRightWarn;
        private string _angleRightAlarm;
        private string _hitWarn;
        private string _hitAlarm;
        private string _hitSet;
        private string _hitSetInfo;
        private string _areaProWarn;
        private string _areaProAlarm;
        private string _areaProSwitch;
        private string _areaSetInfo;
        ////////////////////////////////end//////////////////////////////



        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string craneNo
        {
            set { _craneno = value; }
            get { return _craneno; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string ip
        {
            set { _ip = value; }
            get { return _ip; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string port
        {
            set { _port = value; }
            get { return _port; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string softVersion
        {
            set { _softversion = value; }
            get { return _softversion; }
        }
        /// <summary>
        /// 高度标定参数
        /// </summary>
        public string highCaltParam
        {
            set { _highcaltparam = value; }
            get { return _highcaltparam; }
        }
        /// <summary>
        /// 变幅标定参数
        /// </summary>
        public string amplitudeCaltParam
        {
            set { _amplitudecaltparam = value; }
            get { return _amplitudecaltparam; }
        }
        /// <summary>
        /// 吊重标定参数
        /// </summary>
        public string hoistCaltParam
        {
            set { _hoistcaltparam = value; }
            get { return _hoistcaltparam; }
        }
        /// <summary>
        /// 防碰撞频道号
        /// </summary>
        public string collisionChannelNo
        {
            set { _collisionchannelno = value; }
            get { return _collisionchannelno; }
        }
        /// <summary>
        /// 力矩标定参数
        /// </summary>
        public string momentCaltParam
        {
            set { _momentcaltparam = value; }
            get { return _momentcaltparam; }
        }
        /// <summary>
        /// 倍率
        /// </summary>
        public string ratio
        {
            set { _ratio = value; }
            get { return _ratio; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string remark
        {
            set { _remark = value; }
            get { return _remark; }
        }
        /// <summary>
        ///  最小高度时 AD 采样值
        /// </summary>
        public string minHighAD
        {
            set { _minhighad = value; }
            get { return _minhighad; }
        }
        /// <summary>
        ///  最大高度时 AD 采样值
        /// </summary>
        public string maxHighAD
        {
            set { _maxhighad = value; }
            get { return _maxhighad; }
        }
        /// <summary>
        /// 标准尺度长度
        /// </summary>
        public string standardScale
        {
            set { _standardscale = value; }
            get { return _standardscale; }
        }
        /// <summary>
        /// 最小幅度
        /// </summary>
        public string minAmplitude
        {
            set { _minamplitude = value; }
            get { return _minamplitude; }
        }
        /// <summary>
        ///  最小幅度时 AD 采样值
        /// </summary>
        public string minAmplitudeAD
        {
            set { _minamplitudead = value; }
            get { return _minamplitudead; }
        }
        /// <summary>
        /// 最大幅度
        /// </summary>
        public string maxAmplitude
        {
            set { _maxamplitude = value; }
            get { return _maxamplitude; }
        }
        /// <summary>
        ///  最大幅度时 AD 采样值
        /// </summary>
        public string maxAmplitudeAD
        {
            set { _maxamplitudead = value; }
            get { return _maxamplitudead; }
        }
        /// <summary>
        /// 空钩时AD采样值
        /// </summary>
        public string emptyhookAD
        {
            set { _emptyhookad = value; }
            get { return _emptyhookad; }
        }
        /// <summary>
        /// 吊重砝码AD采样值
        /// </summary>
        public string loadWeightAD
        {
            set { _loadweightad = value; }
            get { return _loadweightad; }
        }
        /// <summary>
        /// 砝码重量
        /// </summary>
        public string farmarWeight
        {
            set { _farmarweight = value; }
            get { return _farmarweight; }
        }
        /// <summary>
        /// 回转类型
        /// </summary>
        public string rotaryType
        {
            set { _rotarytype = value; }
            get { return _rotarytype; }
        }
        /// <summary>
        /// 绝对值回转方向
        /// </summary>
        public string absTurnDirection
        {
            set { _absturndirection = value; }
            get { return _absturndirection; }
        }
        /// <summary>
        /// 绝对值回转值
        /// </summary>
        public string absTurnValue
        {
            set { _absturnvalue = value; }
            get { return _absturnvalue; }
        }
        /// <summary>
        /// 绝对值回转点确认后的回转值
        /// </summary>
        public string absTurnPointValue
        {
            set { _absturnpointvalue = value; }
            get { return _absturnpointvalue; }
        }
        /// <summary>
        /// 电位器回转左限位 AD 值
        /// </summary>
        public string potLeftLimitAD
        {
            set { _potleftlimitad = value; }
            get { return _potleftlimitad; }
        }
        /// <summary>
        /// 电位器回转右限位 AD 值
        /// </summary>
        public string potRightLimitAD
        {
            set { _potrightlimitad = value; }
            get { return _potrightlimitad; }
        }
        /// <summary>
        /// 电位器回转左右限位角度和 
        /// </summary>
        public string potLimitAngle
        {
            set { _potlimitangle = value; }
            get { return _potlimitangle; }
        }
        /// <summary>
        /// 4 倍率时最大起重量
        /// </summary>
        public string liftWeight4Ratio
        {
            set { _liftweight4ratio = value; }
            get { return _liftweight4ratio; }
        }
        /// <summary>
        /// 4 倍率时最大起重量幅度 
        /// </summary>
        public string liftWeightRange4R
        {
            set { _liftweightrange4r = value; }
            get { return _liftweightrange4r; }
        }
        /// <summary>
        /// 4 倍率时最大幅度
        /// </summary>
        public string maxRange4Ratio
        {
            set { _maxrange4ratio = value; }
            get { return _maxrange4ratio; }
        }
        /// <summary>
        /// 4 倍率时最大幅度起重量
        /// </summary>
        public string maxRangeWeight4R
        {
            set { _maxrangeweight4r = value; }
            get { return _maxrangeweight4r; }
        }
        /// <summary>
        /// 2 倍率时最大起重量
        /// </summary>
        public string liftWeight2Ratio
        {
            set { _liftweight2ratio = value; }
            get { return _liftweight2ratio; }
        }
        /// <summary>
        ///  2 倍率时最大起重量幅度
        /// </summary>
        public string liftWeightRange2R
        {
            set { _liftweightrange2r = value; }
            get { return _liftweightrange2r; }
        }
        /// <summary>
        /// 2 倍率时最大幅度
        /// </summary>
        public string maxRange2Ratio
        {
            set { _maxrange2ratio = value; }
            get { return _maxrange2ratio; }
        }
        /// <summary>
        ///  2 倍率时最大幅度起重量
        /// </summary>
        public string maxRangeWeight2R
        {
            set { _maxrangeweight2r = value; }
            get { return _maxrangeweight2r; }
        }
        /// <summary>
        ///   ZIGBEE 本机编号
        /// </summary>
        public string zigbeeLocalNo
        {
            set { _zigbeelocalno = value; }
            get { return _zigbeelocalno; }
        }
        /// <summary>
        ///  ZIGBEE 本机频道号
        /// </summary>
        public string zigbeeChannelNo
        {
            set { _zigbeechannelno = value; }
            get { return _zigbeechannelno; }
        }
        /// <summary>
        /// ZIGBEE 本机组号
        /// </summary>
        public string zigbeeGroupNo
        {
            set { _zigbeegroupno = value; }
            get { return _zigbeegroupno; }
        }
        /// <summary>
        /// 防碰撞信息本机 X
        /// </summary>
        public string antiCollisionX
        {
            set { _anticollisionx = value; }
            get { return _anticollisionx; }
        }
        /// <summary>
        /// 防碰撞信息本机 Y
        /// </summary>
        public string antiCollisionY
        {
            set { _anticollisiony = value; }
            get { return _anticollisiony; }
        }
        /// <summary>
        /// 起重臂长
        /// </summary>
        public string liftWeightArmLenght
        {
            set { _liftweightarmlenght = value; }
            get { return _liftweightarmlenght; }
        }
        /// <summary>
        /// 平衡臂长
        /// </summary>
        public string balanceArmLenght
        {
            set { _balancearmlenght = value; }
            get { return _balancearmlenght; }
        }
        /// <summary>
        /// 塔身高度
        /// </summary>
        public string towerHeight
        {
            set { _towerheight = value; }
            get { return _towerheight; }
        }
        /// <summary>
        /// 塔冒高度
        /// </summary>
        public string towerAtHeight
        {
            set { _toweratheight = value; }
            get { return _toweratheight; }
        }
        /// <summary>
        /// 幅度减速值
        /// </summary>
        public string ampReductionValue
        {
            set { _ampreductionvalue = value; }
            get { return _ampreductionvalue; }
        }
        /// <summary>
        /// 幅度限速值
        /// </summary>
        public string ampRestrictValue
        {
            set { _amprestrictvalue = value; }
            get { return _amprestrictvalue; }
        }
        /// <summary>
        /// 高度减速值
        /// </summary>
        public string highReductionValue
        {
            set { _highreductionvalue = value; }
            get { return _highreductionvalue; }
        }
        /// <summary>
        /// 高度限速值
        /// </summary>
        public string highRestrictValue
        {
            set { _highrestrictvalue = value; }
            get { return _highrestrictvalue; }
        }
        /// <summary>
        /// 回转减速值
        /// </summar回转减速值y>
        public string turnReducionValue
        {
            set { _turnreducionvalue = value; }
            get { return _turnreducionvalue; }
        }
        /// <summary>
        /// 回转限位值
        /// </summary>
        public string turnRestrictValue
        {
            set { _turnrestrictvalue = value; }
            get { return _turnrestrictvalue; }
        }
        /// <summary>
        /// 区域保护减速值
        /// </summary>
        public string areaReductionValue
        {
            set { _areareductionvalue = value; }
            get { return _areareductionvalue; }
        }
        /// <summary>
        /// 区域保护限位值
        /// </summary>
        public string areaRestrictValue
        {
            set { _arearestrictvalue = value; }
            get { return _arearestrictvalue; }
        }
        /// <summary>
        /// 防碰撞限位值
        /// </summary>
        public string acReductionValue
        {
            set { _acreductionvalue = value; }
            get { return _acreductionvalue; }
        }
        /// <summary>
        /// 防碰撞减速值
        /// </summary>
        public string acRestrictValue
        {
            set { _acrestrictvalue = value; }
            get { return _acrestrictvalue; }
        }
        /// <summary>
        /// 换速力矩
        /// </summary>
        public string throwOverTorque
        {
            set { _throwovertorque = value; }
            get { return _throwovertorque; }
        }
        /// <summary>
        /// 切断力矩
        /// </summary>
        public string cutTorque
        {
            set { _cuttorque = value; }
            get { return _cuttorque; }
        }
        /// <summary>
        /// 换速重量
        /// </summary>
        public string throwOverWeight
        {
            set { _throwoverweight = value; }
            get { return _throwoverweight; }
        }
        /// <summary>
        /// 切断重量
        /// </summary>
        public string cutWeight
        {
            set { _cutweight = value; }
            get { return _cutweight; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string cfstate
        {
            set { _cfstate = value; }
            get { return _cfstate; }
        }
        /// <summary>
        /// 上传的时间
        /// </summary>
        public DateTime? cftime
        {
            set { _cftime = value; }
            get { return _cftime; }
        }
        /// <summary>
        /// 最后一次在终端设定的时间
        /// </summary>
        public string SetTime
        {
            set { _setTime = value; }
            get { return _setTime; }
        }
        /////////////////////////////新协议/////////////////////////////
        /// <summary>
        /// 参数修改时间/RTC
        /// </summary>
        public string RTC
        {
            get { return _RTC; }
            set { _RTC = value; }
        }
        /// <summary>
        /// 回转设置数据
        /// </summary>
        public string RotarySet
        {
            get { return _rotarySet; }
            set { _rotarySet = value; }
        }
        /// <summary>
        /// 风速单位
        /// </summary>
        public string WindUnit
        {
            get { return _windUnit; }
            set
            {
                _windUnit = value;
            }
        }
        /// <summary>
        /// 风速预警
        /// </summary>
        public string WindWarn
        {
            get { return _windWarn; }
            set { _windWarn = value; }
        }
        /// <summary>
        /// 风速预警
        /// </summary>
        public string WindAlarm
        {
            get { return _windAlarm; }
            set { _windAlarm = value; }
        }
        /// <summary>
        /// 倾角前预警
        /// </summary>
        public string AngleFrontWarn
        {
            get { return _angleFrontWarn; }
            set { _angleFrontWarn = value; }
        }
        /// <summary>
        /// 倾角前报警
        /// </summary>
        public string AngleFrontAlarm
        {
            get { return _angleFrontAlarm; }
            set { _angleFrontAlarm = value; }
        }
        /// <summary>
        /// 倾角后预警
        /// </summary>
        public string AngleBackWarn
        {
            get { return _angleBackWarn; }
            set
            {
                _angleBackWarn = value;
            }
        }
        /// <summary>
        /// 倾角后报警
        /// </summary>
        public string AngleBackAlarm
        {
            get { return _angleBackAlarm; }
            set { _angleBackAlarm = value; }
        }
        /// <summary>
        /// 倾角左预警
        /// </summary>
        public string AngleLeftWarn
        {
            get { return _angleLeftWarn; }
            set { _angleLeftWarn = value; }
        }
        /// <summary>
        /// 倾角左报警
        /// </summary>
        public string AngleLeftAlarm
        {
            get { return _angleLeftAlarm; }
            set { _angleLeftAlarm = value; }
        }
        /// <summary>
        /// 倾角右预警
        /// </summary>
        public string AngleRightWarn
        {
            get { return _angleRightWarn; }
            set { _angleRightWarn = value; }
        }
        /// <summary>
        /// 倾角右报警
        /// </summary>
        public string AngleRightAlarm
        {
            get { return _angleRightAlarm; }
            set { _angleRightAlarm = value; }
        }
        /// <summary>
        /// 防碰撞预警
        /// </summary>
        public string HitWarn
        {
            get { return _hitWarn; }
            set { _hitWarn = value; }
        }
        /// <summary>
        /// 防碰撞报警
        /// </summary>
        public string HitAlarm
        {
            get { return _hitAlarm; }
            set { _hitAlarm = value; }
        }
        /// <summary>
        /// 防碰撞设置方式
        /// </summary>
        public string HitSet
        {
            get { return _hitSet; }
            set { _hitSet = value; }
        }
        /// <summary>
        /// 防碰撞设置信息
        /// </summary>
        public string HitSetInfo
        {
            get { return _hitSetInfo; }
            set { _hitSetInfo = value; }
        }
        /// <summary>
        /// 区域保护预警
        /// </summary>
        public string AreaProWarn
        {
            get { return _areaProWarn; }
            set { _areaProWarn = value; }
        }
        /// <summary>
        /// 区域保护报警
        /// </summary>
        public string AreaProAlarm
        {
            get { return _areaProAlarm; }
            set { _areaProAlarm = value; }
        }
        /// <summary>
        /// 区域保护开关信息
        /// </summary>
        public string AreaProSwitch
        {
            get { return _areaProSwitch; }
            set { _areaProSwitch = value; }
        }
        /// <summary>
        /// 区域设备信息
        /// </summary>
        public string AreaSetInfo
        {
            get { return _areaSetInfo; }
            set { _areaSetInfo = value; }
        }
        ///////////////////////////////////end///////////////////////////////
        #endregion Model
    }

    /// <summary>
    /// 控制器运行时间
    /// </summary>
    [Serializable]
    public class CraneRunTime
    {
        private string _craneNo;
        private string _run_second;//秒
        private string _run_second_sum;//秒
        /// <summary>
        /// 设备编号
        /// </summary>
        public string CraneNo
        {
            set { _craneNo = value; }
            get { return _craneNo; }
        }

        /// <summary>
        /// 控制器当前运行时间(秒)
        /// </summary>
        public string Run_second
        {
            set { _run_second = value; }
            get { return _run_second; }
        }

        /// <summary>
        /// 控制器总运行时间(秒)
        /// </summary>
        public string Run_second_sum
        {
            set { _run_second_sum = value; }
            get { return _run_second_sum; }
        }
    }
}



