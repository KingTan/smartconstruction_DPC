using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace ProtocolAnalysis.TowerCrane._021303
{
    public enum MessageTypesV021303
    {
        Heartbeat,
        Current,
        ParameterUpload,
        IPConfigure,
        RuntimeEp,
        CommandIssued,
        InformationUpload,
        BlackBox,
        TowerCraneBasicInformation,
        PreventCollision,
        LocalityProtection
    }

    public class FrameV021303 : ISEFrame
    {
        #region 实现ISEFrame接口
        /// <summary>
        /// 版本号
        /// </summary>
        public string AVersion { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 数据帧类型  heartbeat，current，other
        /// 主要用于MQTT
        /// </summary>
        public string FrameDataType { get; set; }
        /// <summary>
        /// 具体帧对象
        /// </summary>
        public object FrameObject { get; set; }
        /// <summary>
        /// MQTT数据对象
        /// </summary>
        public object FrameObject_MQTT { get; set; }
        #endregion
        /// <summary>
        /// 数据帧类型
        /// </summary>
        public MessageTypesV021303 DataType { get; set; }
        
        public FrameV021303()
        {
            AVersion = "021303";
            EquipmentID = "";
            FrameDataType = "other";
            FrameObject = null;
        }
    }

    /// <summary>
    /// 心跳
    /// </summary>
    [Serializable]
    public class HeartbeatV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 在线时间
        /// </summary>
        public string OnlineTime{ get; set; }
    }

    /// <summary>
    /// 实时数据
    /// </summary>
    [Serializable]
    public class CurrentV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 工作循环序列号
        /// </summary>
        public ushort WorkCircle { get; set; }
        /// <summary>
        /// 司机工号卡号
        /// </summary>
        public string DriverCardNo { get; set; }
        /// <summary>
        /// RTC
        /// </summary>
        public string RTC { get; set; }
        /// <summary>
        /// 电源状态
        /// </summary>
        public byte PowerState { get; set; }
        /// <summary>
        /// 倍率
        /// </summary>
        public byte Times { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public short Height { get; set; }
        /// <summary>
        /// 幅度
        /// </summary>
        public ushort Range { get; set; }
        /// <summary>
        /// 回转
        /// </summary>
        public short Rotation { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public ushort Weight { get; set; }
        /// <summary>
        /// 风速
        /// </summary>
        public ushort WindSpeed { get; set; }
        /// <summary>
        /// 倾角X
        /// </summary>
        public short DipAngle_X { get; set; }
        /// <summary>
        /// 倾角Y
        /// </summary>
        public short DipAngle_Y { get; set; }
        /// <summary>
        /// 动臂俯仰角
        /// </summary>
        public ushort BoomAngle  { get; set; }
        /// <summary>
        /// 行程
        /// </summary>
        public ushort Stroke { get; set; }
        /// <summary>
        /// 安全幅度
        /// </summary>
        public ushort SafeRange { get; set; }
        /// <summary>
        /// 安全重量
        /// </summary>
        public ushort SafeWeight { get; set; }
        /// <summary>
        /// 安全力矩
        /// </summary>
        public ushort SafeMoment { get; set; }
        /// <summary>
        /// 防碰撞通信状态
        /// </summary>
        public ushort CollisionCommunicationState { get; set; }
        /// <summary>
        /// 模块状态
        /// </summary>
        public ushort ModuleState { get; set; }
        /// <summary>
        /// 继电器状态
        /// </summary>
        public ushort RelayState { get; set; }
        /// <summary>
        /// 传感器状态
        /// </summary>
        public ushort SensorState { get; set; }
        /// <summary>
        /// 预警信息
        /// </summary>
        public uint WarningMessage { get; set; }
        /// <summary>
        /// 报警信息
        /// </summary>
        public uint AlarmMessage { get; set; }
        /// <summary>
        /// 设备显示
        /// </summary>
        public byte EquipmentDisplaying { get; set; }
        /// <summary>
        /// 可设置信息
        /// </summary>
        public byte CanSetMessage { get; set; }
        /// <summary>
        /// 副臂个数/副钩个数
        /// </summary>
        public byte ViceHookCount { get; set; }
        /// <summary>
        /// 副钩数据信息
        /// </summary>
        public ViceHook[] ViceHookMessage { get; set; }

        #region 副钩类
        [Serializable]
        public class ViceHook
        {
            /// <summary>
            /// 倍率
            /// </summary>
            public byte Times { get; set; }
            /// <summary>
            /// 高度
            /// </summary>
            public short Height { get; set; }
            /// <summary>
            /// 幅度
            /// </summary>
            public ushort Range { get; set; }
            /// <summary>
            /// 重量
            /// </summary>
            public ushort Weight { get; set; }
            /// <summary>
            /// 安全幅度
            /// </summary>
            public ushort SafeRange { get; set; }
            /// <summary>
            /// 安全重量
            /// </summary>
            public ushort SafeWeight { get; set; }
            /// <summary>
            /// 安全力矩
            /// </summary>
            public ushort SafeMoment { get; set; }
            /// <summary>
            /// 继电器状态
            /// </summary>
            public ushort RelayState { get; set; }
            /// <summary>
            /// 传感器状态  new
            /// </summary>
            public ushort SensorState { get; set; }
            /// <summary>
            /// 预警信息
            /// </summary>
            public uint WarningMessage { get; set; }
            /// <summary>
            /// 报警信息
            /// </summary>
            public uint AlarmMessage { get; set; }
        }
        #endregion
    }

    /// <summary>
    /// 身份验证
    /// </summary>
    [Serializable]
    public class AuthenticateV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 身份验证方式
        /// </summary>
        public byte IdentificationType { get; set; }
        /// <summary>
        /// 子命令字
        /// </summary>
        public byte Subcommand { get; set; }
        /// <summary>
        /// 数据区
        /// </summary>
        public object SubcommandDistrict { get; set; }
        public class OneSubcommandEP
        {
            /// <summary>
            /// 司机工号卡号
            /// </summary>
            public string DriverCardNo { get; set; }
            /// <summary>
            /// 状态
            /// </summary>
            public byte State { get; set; }

        }
        public class OneSubcommandSE
        {
            /// <summary>
            /// 司机工号卡号
            /// </summary>
            public string DriverCardNo { get; set; }
            /// <summary>
            /// 结果
            /// </summary>
            public byte Result { get; set; }

        }
        public class TwoAndFourSubcommandEP
        {
            /// <summary>
            /// 总包数
            /// </summary>
            public byte AllPackageCount { get; set; }
            /// <summary>
            /// 当前包
            /// </summary>
            public byte PresentPackage { get; set; }
            /// <summary>
            /// 特征库
            /// </summary>
            public string FeatureLibrary { get; set; }
            /// <summary>
            /// 司机工号卡号  主要是为了做考勤记录
            /// </summary>
            public string DriverCardNo { get; set; }

        }
        public class TwoAndFourSubcommandSE
        {
            /// <summary>
            /// 总包数
            /// </summary>
            public byte AllPackageCount { get; set; }
            /// <summary>
            /// 当前包
            /// </summary>
            public byte PresentPackage { get; set; }
            /// <summary>
            /// 结果
            /// </summary>
            public byte Result { get; set; }

        }
        public class ThreeSubcommandEP
        {
            /// <summary>
            /// 结果
            /// </summary>
            public byte Result { get; set; }
            #region 司机信息
            /// <summary>
            /// 工号
            /// </summary>
            public string gh { get; set; }
            /// <summary>
            /// 卡号
            /// </summary>
            public string CardId { get; set; }
            /// <summary>
            /// 身份证号
            /// </summary>
            public string IDCard { get; set; }
            /// <summary>
            /// 手机号
            /// </summary>
            public string CellPhone { get; set; }
            /// <summary>
            /// 姓名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 有效期
            /// </summary>
            public DateTime ValidityTime { get; set; } 
            /// <summary>
            /// 职位/人员类型
            /// </summary>
            public byte PersonnelType { get; set; }
            /// <summary>
            /// 预留
            /// </summary>
            public object obligate { get; set; }
            #endregion
        }
        public class SixSubcommandEP
        {
            /// <summary>
            /// 监理人员工号
            /// </summary>
            public string  Administrator_gh { get; set; }
            /// <summary>
            /// 授权人员身份证号
            /// </summary>
            public string Authorizer_IDCard { get; set; }
        }
    }

    /// <summary>
    /// 参数上传
    /// </summary>
    [Serializable]
    public class ParameterUploadV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 参数修改时间/RTC
        /// </summary>
        public string ChangeRTC { get; set; }
        /// <summary>
        /// 修改标识
        /// </summary>
        public byte ModifyIdentification { get; set; }
        #region 高度设置
        /// <summary>
        /// 高度设置
        /// </summary>
        public object HeightSet { get; set; }
        public abstract class AHeightSet
        {
            /// <summary>
            /// 传感器类型
            /// </summary>
            public byte SensorType { get; set; }
            /// <summary>
            /// 上限位采样值
            /// </summary>
            public uint UpperLimitSampling { get; set; }
            /// <summary>
            /// 下限位采样值
            /// </summary>
            public uint LowerLimitSampling { get; set; }
            /// <summary>
            /// 塔身高度值
            /// </summary>
            public ushort TowerBodyheight { get; set; }
            /// <summary>
            /// 减速值
            /// </summary>
            public ushort ReductionSpeed { get; set; }
            /// <summary>
            /// 限位值
            /// </summary>
            public ushort Limit { get; set; }
            /// <summary>
            /// 顶升标识
            /// </summary>
            public byte JackingMark { get; set; }
        }
        /// <summary>
        /// 电位器时的高度
        /// </summary>
        public class HeightSet_Potentiometer : AHeightSet
        {}
        /// <summary>
        /// 多圈绝对值编码器时的高度
        /// </summary>
        public class HeightSet_Coder : AHeightSet
        {
            /// <summary>
            /// 采样值增加方向
            /// </summary>
            public byte SamplingAddDirection { get; set; }
            /// <summary>
            /// 初始圈数值
            /// </summary>
            public ushort InitialCylinderNumber { get; set; }
        }
        #endregion
        #region 幅度设置
        /// <summary>
        /// 幅度设置
        /// </summary>
        public object RangeSet { get; set; }
        public abstract class ARangeSet
        {
            /// <summary>
            /// 传感器类型
            /// </summary>
            public byte SensorType { get; set; }
            /// <summary>
            /// 内限位采样值
            /// </summary>
            public uint InsideLimitSampling { get; set; }
            /// <summary>
            /// 外限位采样值
            /// </summary>
            public uint OutsideLimitSampling { get; set; }
            /// <summary>
            /// 最小幅度值
            /// </summary>
            public ushort MinARange { get; set; }
            /// <summary>
            /// 最大幅度值
            /// </summary>
            public ushort MaxARange { get; set; }
            /// <summary>
            /// 减速值
            /// </summary>
            public ushort ReductionSpeed { get; set; }
            /// <summary>
            /// 限位值
            /// </summary>
            public ushort Limit { get; set; }
        }
        /// <summary>
        /// 电位器时的高度
        /// </summary>
        public class RangeSet_Potentiometer : ARangeSet
        { }
        /// <summary>
        /// 多圈绝对值编码器时的高度
        /// </summary>
        public class RangeSet_Coder : ARangeSet
        {
            /// <summary>
            /// 采样值增加方向
            /// </summary>
            public byte SamplingAddDirection { get; set; }
            /// <summary>
            /// 初始圈数值
            /// </summary>
            public ushort InitialCylinderNumber { get; set; }
        }
        #endregion
        #region 回转设置
        /// <summary>
        /// 回转设置
        /// </summary>
        public object RotationSet { get; set; }
        public abstract class ARotationSet
        {
            /// <summary>
            /// 回转类型
            /// </summary>
            public byte RotationType { get; set; }
        }
        /// <summary>
        /// 类型A
        /// </summary>
        public class RotationSet_A : ARotationSet
        {
            /// <summary>
            /// 左极限采样值
            /// </summary>
            public uint LeftLimitSampling { get; set; }
            /// <summary>
            /// 右极限采样值
            /// </summary>
            public uint RightLimitSampling { get; set; }
            /// <summary>
            /// 回转左右极限角度和
            /// </summary>
            public ushort EquipmentIDLeftAndRightLimitAngleSum { get; set; }
            /// <summary>
            /// 回转减速值
            /// </summary>
            public ushort ReductionSpeed { get; set; }
            /// <summary>
            /// 回转限位值
            /// </summary>
            public ushort Limit { get; set; }
        }
        /// <summary>
        /// 类型B
        /// </summary>
        public class RotationSet_B : ARotationSet
        {
            /// <summary>
            /// 设置方向
            /// </summary>
            public byte Direction { get; set; }
            /// <summary>
            /// 方向按钮采集值
            /// </summary>
            public uint DirectionButtonGather { get; set; }
            /// <summary>
            /// 确认按钮采集值
            /// </summary>
            public uint ConfirmButtonGather { get; set; }
        }
        /// <summary>
        /// 类型C
        /// </summary>
        public class RotationSet_C : ARotationSet
        {
            /// <summary>
            /// 0度采样值
            /// </summary>
            public uint ZeroAngleSampling { get; set; }
            /// <summary>
            /// 设定角度采样值
            /// </summary>
            public uint SetAngleSampling { get; set; }
            /// <summary>
            /// 设定角度值
            /// </summary>
            public ushort SetAngle { get; set; }
            /// <summary>
            /// 回转减速值
            /// </summary>
            public ushort ReductionSpeed { get; set; }
            /// <summary>
            /// 回转限位值
            /// </summary>
            public ushort Limit { get; set; }
        }
        /// <summary>
        /// 类型D
        /// </summary>
        public class RotationSet_D : ARotationSet
        {
            /// <summary>
            /// 相对/绝对零度标识
            /// </summary>
            public byte RelativeAbsoluteZeroIdentification { get; set; }
            /// <summary>
            /// 旋转方向
            /// </summary>
            public byte RotateDirection { get; set; }
        }
        /// <summary>
        /// 类型E
        /// </summary>
        public class RotationSet_E : ARotationSet
        {
            /// <summary>
            /// 采样值增加方向
            /// </summary>
            public byte SamplingAddDirection { get; set; }
            /// <summary>
            /// 初始圈数值
            /// </summary>
            public ushort InitialCylinderNumber { get; set; }
            /// <summary>
            /// 传感器传动比
            /// </summary>
            public ushort[] SensorDriveRatio { get; set; }
            /// <summary>
            /// 塔吊齿数
            /// </summary>
            public ushort TowerTeeth { get; set; }
            /// <summary>
            /// 传感器齿数
            /// </summary>
            public ushort SensorTeeth { get; set; }
            /// <summary>
            /// 0度传感器值
            /// </summary>
            public uint ZeroSensor { get; set; }
            /// 逆时针传感器值
            /// </summary>
            public uint anticlockwiseSensor { get; set; }
        }
        #endregion
        #region  重量设置
        /// <summary>
        /// 重量设置
        /// </summary>
        public object WeightSet { get; set; }
        public abstract class AWeightSet
        {
            /// <summary>
            /// 传感器类型
            /// </summary>
            public byte SensorType { get; set; }
            /// <summary>
            /// 设置倍率 New
            /// </summary>
            public byte SetRate { get; set; }
            /// <summary>
            /// 空钩采样值
            /// </summary>
            public uint EmptyHookSampling { get; set; }
            /// <summary>
            /// 吊重砝码采样值
            /// </summary>
            public uint CraneWeightSampling { get; set; }
            /// <summary>
            /// 砝码重量
            /// </summary>
            public uint WeightWeight { get; set; }
            /// <summary>
            /// 换速重量
            /// </summary>
            public byte ThrowOverWeight { get; set; }
            /// <summary>
            /// 切断重量
            /// </summary>
            public byte CutOffWeight { get; set; }
        }
        /// <summary>
        /// 传感器类型为模拟信号型时
        /// </summary>
        public class WeightSet_AnalogSignal : AWeightSet
        { }
        #endregion
        #region 力矩设置
        /// <summary>
        /// 力矩设置
        /// </summary>
        public object MomentSet { get; set; }
        /// <summary>
        /// 力矩设置类
        /// </summary>
        public class CMomentSet
        {
            /// <summary>
            /// 换速力矩
            /// </summary>
            public byte ThrowOverMomentSet { get; set; }
            /// <summary>
            /// 切断力矩
            /// </summary>
            public byte CutOffMomentSet { get; set; }
        }
        #endregion
        #region 风速设置
        /// <summary>
        /// 风速设置
        /// </summary>
        public object WindSpeedSet { get; set; }
        /// <summary>
        /// 风速设置类
        /// </summary>
        public class CWindSpeedSet
        {
            /// <summary>
            /// 风速类型
            /// </summary>
            public byte WindSpeedType { get; set; }
            /// <summary>
            /// 风速单位
            /// </summary>
            public byte WindSpeedUnit { get; set; }
            /// <summary>
            /// 风速预警
            /// </summary>
            public ushort WindSpeedWarning { get; set; }
            /// <summary>
            /// 风速报警
            /// </summary>
            public ushort WindSpeedAlarm { get; set; }
        }
        #endregion
        #region 倾角设置
        /// <summary>
        /// 倾角设置
        /// </summary>
        public object DipAngleSet { get; set; }
        /// <summary>
        /// 倾角设置类
        /// </summary>
        public class CDipAngleSet
        {
            /// <summary>
            /// 倾角类型
            /// </summary>
            public byte DipAngleType { get; set; }
            /// <summary>
            /// 倾角相对零度标志
            /// </summary>
            public byte DipAngleRelativelyZeroFlag { get; set; }
            /// <summary>
            /// 倾角预警
            /// </summary>
            public ushort DipAngleWarning { get; set; }
            /// <summary>
            /// 倾角报警
            /// </summary>
            public ushort DipAngleAlarm { get; set; }
        }
        #endregion
        #region 仰角设置
        /// <summary>
        /// 仰角设置
        /// </summary>
        public object BoomAngleSet { get; set; }
        /// <summary>
        /// 仰角设置类
        /// </summary>
        public class CBoomAngleSet
        {
            /// <summary>
            /// 仰角传感器类型
            /// </summary>
            public byte BoomAngleType { get; set; }
            /// <summary>
            /// 仰角相对零度标志
            /// </summary>
            public byte BoomAngleRelativelyZeroFlag { get; set; }
            /// <summary>
            /// 仰角最小角度值
            /// </summary>
            public ushort BoomAngleMin { get; set; }
            /// <summary>
            /// 仰角最大角度值
            /// </summary>
            public ushort BoomAngleMax { get; set; }
            /// <summary>
            /// 仰角换速值 
            /// </summary>
            public ushort BoomAngleThrowOver { get; set; }
            /// <summary>
            /// 仰角限位值
            /// </summary>
            public ushort BoomAngleLimit { get; set; }
        }
        #endregion
        /// <summary>
        /// 塔吊眼数量
        /// </summary>
        public byte CraneEyeNumber { get; set; }
        #region 塔吊眼参数
        /// <summary>
        /// 塔吊眼参数
        /// </summary>
        public CCraneEyeParameter[] CraneEyeParameter { get; set; }
        /// <summary>
        /// 塔吊眼参数类
        /// </summary>
        public class CCraneEyeParameter
        {
            /// <summary>
            /// 水平角
            /// </summary>
            public ushort HorizontalAngle  { get; set; }
            /// <summary>
            /// 俯仰角
            /// </summary>
            public ushort AngleOfPitch { get; set; }
            /// <summary>
            /// 球机放大倍数
            /// </summary>
            public ushort SpeedDomeCamerasMagnification { get; set; }
            /// <summary>
            /// 目标高度
            /// </summary>
            public ushort TargetAltitude { get; set; }
            /// <summary>
            /// 屏幕聚焦系数
            /// </summary>
            public ushort FocusingFactor  { get; set; }
            /// <summary>
            /// 大臂长
            /// </summary>
            public ushort LongArm { get; set; }
            /// <summary>
            /// 算法
            /// </summary>
            public byte Algorithm { get; set; }
            /// <summary>
            /// 聚焦最小值
            /// </summary>
            public ushort FocusingMin { get; set; }
            /// <summary>
            /// 聚焦最大值
            /// </summary>
            public ushort FocusingMax { get; set; }
        }
        #endregion
        /// <summary>
        /// 副臂/副钩个数
        /// </summary>
        public byte ViceHookCount { get; set; }
        #region 副钩设置参数
        /// <summary>
        /// 副钩设置参数
        /// </summary>
        public CViceHookSetParameter[] ViceHookSetParameter { get; set; }
        /// <summary>
        /// 副钩设置参数类
        /// </summary>
        public class CViceHookSetParameter
        {
            /// <summary>
            /// 高度设置
            /// </summary>
            public object HeightSet { get; set; }
            /// <summary>
            /// 重量设置
            /// </summary>
            public object WeightSet { get; set; }
            /// <summary>
            /// 力矩设置
            /// </summary>
            public object MomentSet { get; set; }
        }
        #endregion
    }

    /// <summary>
    /// IP地址配置
    /// </summary>
    [Serializable]
    public class IPConfigureV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 连接标志
        /// </summary>
        public byte IdentificationType { get; set; }
        //平台上传的状态
        public int ResultStatus { get; set; }
    }

    /// <summary>
    /// 设备运行时间
    /// </summary>
    [Serializable]
    public class RuntimeEpV021303
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

    /// <summary>
    /// 命令下发
    /// </summary>
    [Serializable]
    public class CommandIssuedV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 确认标识
        /// </summary>
        public byte IdentificationMark { get; set; }
        /// <summary>
        /// 命令字
        /// </summary>
        public byte Subcommand { get; set; }
        /// <summary>
        /// 具体参数
        /// </summary>
        public object SpecificParameter { get; set; }
        /// <summary>
        /// 功能配置信息类
        /// </summary>
        public class CFunctionConfiguration
        {
            /// <summary>
            /// 功能配置信息
            /// </summary>
            public ushort FunctionConfiguration { get; set; }
        }
        /// <summary>
        /// 限位配置信息类
        /// </summary>
        public class CLimitControl
        {
            /// <summary>
            /// 限位配置信息
            /// </summary>
            public uint LimitControl { get; set; }
        }
        /// <summary>
        /// 时间校准类
        /// </summary>
        public class CTimeCalibration 
        {
            /// <summary>
            /// 时间校准
            /// </summary>
            public string TimeCalibration { get; set; }
        }
    }

    /// <summary>
    /// 信息上传
    /// </summary>
    [Serializable]
    public class InformationUploadV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// RTC
        /// </summary>
        public string RTC { get; set; }
        /// <summary>
        /// 信息类型
        /// </summary>
        public byte InformationType { get; set; }
        /// <summary>
        /// 信息码
        /// </summary>
        public byte InformationCode { get; set; }
    }

    /// <summary>
    /// 黑匣子信息
    /// </summary>
    [Serializable]
    public class BlackBoxV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 功能锁定标识
        /// </summary>
        public byte FunctionLockIdentifier { get; set; }
        /// <summary>
        /// 功能配置
        /// </summary>
        public ushort FunctionConfiguration { get; set; }
        #region 经纬度
        /// <summary>
        /// 经纬度
        /// </summary>
        public object LongitudeAndLatitude { get; set; }
        public  class CLongitudeAndLatitude
        {
            /// <summary>
            /// 北纬或南纬
            /// </summary>
            public byte LatitudeType { get; set; }
            /// <summary>
            /// 纬度
            /// </summary>
            public uint Latitude { get; set; }
            /// <summary>
            /// 东经或西经
            /// </summary>
            public byte LongitudeType { get; set; }
            /// <summary>
            /// 经度
            /// </summary>
            public uint Longitude { get; set; }
        }
        #endregion
        /// <summary>
        /// 身份认证方式锁定标识
        /// </summary>
        public byte IdentificationTypeLockIdentifier { get; set; }
        /// <summary>
        /// 身份认证方式
        /// </summary>
        public byte IdentificationType { get; set; }
        /// <summary>
        /// 身份识别周期开关状态
        /// </summary>
        public byte IdentificationCycleSwitchState { get; set; }
        /// <summary>
        /// 身份识别周期
        /// </summary>
        public ushort IdentificationCycle { get; set; }
        /// <summary>
        /// 亮度设置
        /// </summary>
        public byte[] BrightnessSetting { get; set; }
        /// <summary>
        /// Zigbee版本
        /// </summary>
        public string Version_Zigbee { get; set; }
        /// <summary>
        /// 继电器版本
        /// </summary>
        public string Version_Relay { get; set; }
        /// <summary>
        /// 从机版本
        /// </summary>
        public string Version_Counterpart { get; set; }
        /// <summary>
        /// 身份证版本
        /// </summary>
        public string Version_IDCard { get; set; }
        /// <summary>
        /// 软件版本
        /// </summary>
        public string Version_Software { get; set; }
    }

    /// <summary>
    /// 塔吊基本信息
    /// </summary>
    [Serializable]
    public class TowerCraneBasicInformationV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 塔吊类型
        /// </summary>
        public byte TowerCraneType { get; set; }
        /// <summary>
        /// 起重臂长度
        /// </summary>
        public ushort BoomArmLength { get; set; }
        /// <summary>
        /// 平衡臂长度
        /// </summary>
        public ushort BalanceArmLength { get; set; }
        /// <summary>
        /// 塔身高度
        /// </summary>
        public ushort TowerBodyHeight { get; set; }
        /// <summary>
        /// 塔帽高度（平臂）/最大仰角（动臂）
        /// </summary>
        public ushort towerCapORElevation  { get; set; }
        /// <summary>
        /// 最小仰角
        /// </summary>
        public ushort MinBoomAngle { get; set; }
        #region 力矩曲线设置数据
        /// <summary>
        /// 力矩曲线类型
        /// </summary>
        public byte MomentCurveType { get; set; }
        /// <summary>
        /// 力矩曲线数量
        /// </summary>
        public byte MomentCurveCount { get; set; }
        /// <summary>
        /// 力矩曲线设置数据
        /// </summary>
        public object[] MomentCurveSet { get; set; }
        /// <summary>
        /// 力矩曲线为0x01时的
        /// </summary>
        public class MomentCurveSet_Curve 
        {
            /// <summary>
            /// 倍率
            /// </summary>
            public byte Rate { get; set; }
            /// <summary>
            /// 最大起重量
            /// </summary>
            public ushort MaxWeight { get; set; }
            /// <summary>
            /// 最大起重量幅度
            /// </summary>
            public ushort MaxWeightRange { get; set; }
            /// <summary>
            /// 最大幅度
            /// </summary>
            public ushort MaxRange { get; set; }
            /// <summary>
            /// 最大幅度起重量
            /// </summary>
            public ushort MaxRangeWeight { get; set; }
        }
        /// <summary>
        /// 力矩曲线为0x00时的
        /// </summary>
        public class MomentCurveSet_Icon 
        {
            /// <summary>
            /// 倍率
            /// </summary>
            public byte Rate { get; set; }
            /// <summary>
            /// 曲线设置点个数
            /// </summary>
            public byte CurveSetPointCount { get; set; }
            /// <summary>
            /// 组信息
            /// </summary>
            public MomentCurve_Icon[] MomentCurve_IconAry { get; set; }
            public class MomentCurve_Icon
            {
                /// <summary>
                /// 起重量
                /// </summary>
                public ushort Weight { get; set; }
                /// <summary>
                /// 起重量幅度
                /// </summary>
                public ushort WeightRange { get; set; }
            }
        }
        #endregion
        /// <summary>
        /// 副臂个数/副钩个数
        /// </summary>
        public byte ViceHookCount { get; set; }
        #region 副钩类
        /// <summary>
        /// 副钩数据信息
        /// </summary>
        public ViceHook[] ViceHookMessage { get; set; }
        public class ViceHook
        {
            /// <summary>
            /// 副臂长
            /// </summary>
            public ushort ViceArmLength { get; set; }
            /// <summary>
            /// 副臂夹角
            /// </summary>
            public ushort ViceArmIntersectionAngle { get; set; }
            /// <summary>
            /// 力矩曲线类型
            /// </summary>
            public byte MomentCurveType { get; set; }
            /// <summary>
            /// 力矩曲线数量
            /// </summary>
            public byte MomentCurveCount { get; set; }
            /// <summary>
            /// 力矩曲线设置数据
            /// </summary>
            public object[] MomentCurveSet { get; set; }
        }
        #endregion
    }

    /// <summary>
    /// 防碰撞设置
    /// </summary>
    [Serializable]
    public class PreventCollisionV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public byte Number { get; set; }
        /// <summary>
        /// 频道号
        /// </summary>
        public byte ChannelNo { get; set; }
        /// <summary>
        /// 组号
        /// </summary>
        public byte GroupNo { get; set; }
        /// <summary>
        /// 通信编号映射
        /// </summary>
        public byte[] CommunicationNoMapped { get; set; }
        /// <summary>
        /// 防碰撞预警
        /// </summary>
        public ushort PreventCollisionWarning { get; set; }
        /// <summary>
        /// 防碰撞报警
        /// </summary>
        public ushort PreventCollisionAlarm { get; set; }
        /// <summary>
        /// 防碰撞设置方式
        /// </summary>
        public byte PreventCollisionSetType { get; set; }
        #region 防碰撞设置信息
        /// <summary>
        /// 防碰撞设置
        /// </summary>
        public object PreventCollisionSet { get; set; }
        public abstract class APreventCollisionSet
        {
            /// <summary>
            /// 塔吊编号
            /// </summary>
            public byte TowerNo { get; set; }
            /// <summary>
            /// 塔吊类型
            /// </summary>
            public byte TowerType { get; set; }
             /// <summary>
            /// 起重臂长
            /// </summary>
            public ushort BoomLength { get; set; }
            /// <summary>
            /// 平衡臂长
            /// </summary>
            public ushort BalanceLength { get; set; }
            /// <summary>
            /// 塔身高度
            /// </summary>
            public ushort TowerBodyHeight { get; set; }
            /// <summary>
            /// 平臂：塔帽高度 / 动臂：最大仰角
            /// </summary>
            public ushort TowerCapHeight { get; set; }
            /// <summary>
            /// 最小仰角
            /// </summary>
            public ushort MinBoomAngle { get; set; }
        }
        /// <summary>
        /// 防碰撞类型设置为0时
        /// </summary>
        public class PreventCollisionSet_Zero : APreventCollisionSet
        {
            /// <summary>
            /// 坐标x
            /// </summary>
            public ushort X { get; set; }
            /// <summary>
            /// 坐标y
            /// </summary>
            public ushort Y { get; set; }
        }
        /// <summary>
        /// 防碰撞类型设置为1时
        /// </summary>
        public class PreventCollisionSet_One : APreventCollisionSet
        {
            /// <summary>
            /// 距离
            /// </summary>
            public ushort Distance { get; set; }
            /// <summary>
            /// 方向（目标相对本机）
            /// </summary>
            public ushort DirectionTargetOppositeLocal { get; set; }
            /// <summary>
            /// 方向（本机相对目标）
            /// </summary>
            public ushort DirectionLocalOppositeTarget { get; set; }
        }
        #endregion
    }

    /// <summary>
    /// 区域保护设置
    /// </summary>
    [Serializable]
    public class LocalityProtectionV021303
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 区域保护预警
        /// </summary>
        public ushort LocalityProtectionWarning { get; set; }
        /// <summary>
        /// 区域保护报警
        /// </summary>
        public ushort LocalityProtectionAlarm { get; set; }
        /// <summary>
        /// 区域保护开关信息
        /// </summary>
        public ushort LocalityProtectionSwitch { get; set; }
        #region 区域保护设置信息
        /// <summary>
        /// 区域保护设置
        /// </summary>
        public CLocalityProtectionSet[] LocalityProtectionSet { get; set; }
        public class CLocalityProtectionSet
        {
            /// <summary>
            /// 第1点幅度
            /// </summary>
            public ushort Range_One { get; set; }
            /// <summary>
            /// 第1点回转
            /// </summary>
            public ushort Rotation_One { get; set; }
            /// <summary>
            /// 第2点幅度
            /// </summary>
            public ushort Range_Two { get; set; }
            /// <summary>
            /// 第2点回转
            /// </summary>
            public ushort Rotation_Two { get; set; }
            /// <summary>
            /// 第3点幅度
            /// </summary>
            public ushort Range_Three { get; set; }
            /// <summary>
            /// 第3点回转
            /// </summary>
            public ushort Rotation_Three { get; set; }
            /// <summary>
            /// 第4点幅度
            /// </summary>
            public ushort Range_Four { get; set; }
            /// <summary>
            /// 第4点回转
            /// </summary>
            public ushort Rotation_Four { get; set; }
        }
        #endregion
    }
}
