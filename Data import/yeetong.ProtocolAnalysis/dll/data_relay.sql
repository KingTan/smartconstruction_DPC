/*
Navicat MySQL Data Transfer

Source Server         : 本机
Source Server Version : 50173
Source Host           : 10.10.10.48:3306
Source Database       : data_relay

Target Server Type    : MYSQL
Target Server Version : 50173
File Encoding         : 65001

Date: 2018-02-02 11:26:32
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for discharge
-- ----------------------------
DROP TABLE IF EXISTS `discharge`;
CREATE TABLE `discharge` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `deviceid` varchar(20) DEFAULT NULL COMMENT '设备编号',
  `datatype` varchar(50) NOT NULL COMMENT '数据类型，判断这个包的类型，比如为心跳，实时数据等',
  `contentjson` text COMMENT '数据内容 json 对象标识，可以直接被序列化的',
  `contenthex` text COMMENT '数据内容原包hex',
  `version` varchar(20) NOT NULL COMMENT '版本号',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  `usetype` int(10) DEFAULT '0' COMMENT '使用标记',
  `dbtype` int(11) NOT NULL DEFAULT '0' COMMENT '数据库标识 0未进行处理1处理过了',
  `mqtttype` int(11) NOT NULL DEFAULT '0' COMMENT 'mqtttype标识 0未进行处理 1处理过了',
  `forwardtype` int(11) NOT NULL DEFAULT '0' COMMENT '平台转发标识 0未进行处理 1处理过了',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for dust
-- ----------------------------
DROP TABLE IF EXISTS `dust`;
CREATE TABLE `dust` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `deviceid` varchar(20) DEFAULT NULL COMMENT '设备编号',
  `datatype` varchar(50) NOT NULL COMMENT '数据类型，判断这个包的类型，比如为心跳，实时数据等',
  `contentjson` text COMMENT '数据内容 json 对象标识，可以直接被序列化的',
  `contenthex` text COMMENT '数据内容原包hex',
  `version` varchar(20) NOT NULL COMMENT '版本号',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  `usetype` int(10) DEFAULT '0' COMMENT '使用标记',
  `dbtype` int(11) NOT NULL DEFAULT '0' COMMENT '数据库标识 0未进行处理1处理过了',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=57 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for foggun
-- ----------------------------
DROP TABLE IF EXISTS `foggun`;
CREATE TABLE `foggun` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `deviceid` varchar(20) DEFAULT NULL COMMENT '设备编号',
  `datatype` varchar(50) NOT NULL COMMENT '数据类型，判断这个包的类型，比如为心跳，实时数据等',
  `contentjson` text COMMENT '数据内容 json 对象标识，可以直接被序列化的',
  `contenthex` text COMMENT '数据内容原包hex',
  `version` varchar(20) NOT NULL COMMENT '版本号',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  `usetype` int(10) DEFAULT '0' COMMENT '使用标记',
  `dbtype` int(11) NOT NULL DEFAULT '0' COMMENT '数据库标识 0未进行处理1处理过了',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=73 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for lift
-- ----------------------------
DROP TABLE IF EXISTS `lift`;
CREATE TABLE `lift` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `deviceid` varchar(20) DEFAULT NULL COMMENT '设备编号',
  `datatype` varchar(50) NOT NULL COMMENT '数据类型，判断这个包的类型，比如为心跳，实时数据等',
  `contentjson` text COMMENT '数据内容 json 对象标识，可以直接被序列化的',
  `contenthex` text COMMENT '数据内容原包hex',
  `version` varchar(20) NOT NULL COMMENT '版本号',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  `usetype` int(10) DEFAULT '0' COMMENT '使用标记',
  `dbtype` int(11) NOT NULL DEFAULT '0' COMMENT '数据库标识 0未进行处理1处理过了',
  `mqtttype` int(11) NOT NULL DEFAULT '0' COMMENT 'mqtttype标识 0未进行处理 1处理过了',
  `forwardtype` int(11) NOT NULL DEFAULT '0' COMMENT '平台转发标识 0未进行处理 1处理过了',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=212 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for massconcrete
-- ----------------------------
DROP TABLE IF EXISTS `massconcrete`;
CREATE TABLE `massconcrete` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `deviceid` varchar(20) DEFAULT NULL COMMENT '设备编号',
  `datatype` varchar(50) NOT NULL COMMENT '数据类型，判断这个包的类型，比如为心跳，实时数据等',
  `contentjson` text COMMENT '数据内容 json 对象标识，可以直接被序列化的',
  `contenthex` text COMMENT '数据内容原包hex',
  `version` varchar(20) NOT NULL COMMENT '版本号',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  `usetype` int(10) DEFAULT '0' COMMENT '使用标记',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for massconcrete_equipment
-- ----------------------------
DROP TABLE IF EXISTS `massconcrete_equipment`;
CREATE TABLE `massconcrete_equipment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `equipmentID` varchar(10) NOT NULL COMMENT '设备id 4位',
  `onlinetime` datetime DEFAULT NULL COMMENT '最后一次更新的时间',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建的时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for massconcrete_realtimedata
-- ----------------------------
DROP TABLE IF EXISTS `massconcrete_realtimedata`;
CREATE TABLE `massconcrete_realtimedata` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `equipmentID` varchar(10) NOT NULL COMMENT '设备id 4位',
  `updatetime` datetime DEFAULT NULL COMMENT '最后一次更新的时间',
  `SubEquipmentCount` int(11) DEFAULT NULL COMMENT '子机数',
  `SubEquipmentID` varchar(50) DEFAULT NULL COMMENT '子机编号 多个编号中间用&隔开 举例：1&2',
  `PassTemperatureMaxCount` int(11) DEFAULT NULL COMMENT '温度通道最大数',
  `PassTemperature` varchar(255) DEFAULT NULL COMMENT '温度值。多个温度值之间用&隔开。举例12.54&16.21',
  `SubCellVoltage` decimal(10,2) DEFAULT NULL COMMENT '子机电池电压',
  `PassHumidityMaxCount` int(11) DEFAULT NULL COMMENT '湿度通道最大数',
  `PassHumidity` varchar(255) DEFAULT NULL COMMENT '湿度度值。多个湿度值之间用&隔开。举例12.54&16.21',
  `CellVoltage` varchar(255) DEFAULT NULL COMMENT '主机电池电压',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建的时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for towercrane
-- ----------------------------
DROP TABLE IF EXISTS `towercrane`;
CREATE TABLE `towercrane` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `deviceid` varchar(20) DEFAULT NULL COMMENT '设备编号',
  `datatype` varchar(50) NOT NULL COMMENT '数据类型，判断这个包的类型，比如为心跳，实时数据等',
  `contentjson` text COMMENT '数据内容 json 对象标识，可以直接被序列化的',
  `contenthex` text COMMENT '数据内容原包hex',
  `version` varchar(20) NOT NULL COMMENT '版本号',
  `creattime` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '创建时间',
  `usetype` int(10) DEFAULT '0' COMMENT '使用标记',
  `dbtype` int(11) NOT NULL DEFAULT '0' COMMENT '数据库标识 0未进行处理1处理过了',
  `mqtttype` int(11) NOT NULL DEFAULT '0' COMMENT 'mqtttype标识 0未进行处理 1处理过了',
  `forwardtype` int(11) NOT NULL DEFAULT '0' COMMENT '平台转发标识 0未进行处理 1处理过了',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for v59442479
-- ----------------------------
DROP TABLE IF EXISTS `v59442479`;
CREATE TABLE `v59442479` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `DeviceNo` char(20) NOT NULL DEFAULT '59442479',
  `CreateTime` datetime NOT NULL,
  `NOISE` float NOT NULL DEFAULT '0',
  `NOISE_XZ` float DEFAULT NULL COMMENT '修正值',
  `HUMI` float NOT NULL DEFAULT '0',
  `HUMI_XZ` float DEFAULT NULL COMMENT '修正值',
  `WS` float NOT NULL DEFAULT '0',
  `WS_XZ` float DEFAULT NULL COMMENT '修正值',
  `PM25` float NOT NULL DEFAULT '0',
  `PM25_XZ` float DEFAULT NULL COMMENT '修正值',
  `PM10` float NOT NULL DEFAULT '0',
  `PM10_XZ` float DEFAULT NULL COMMENT '修正值',
  `WD` float NOT NULL DEFAULT '0',
  `WD_XZ` float DEFAULT NULL COMMENT '修正值',
  `TEMP` float NOT NULL DEFAULT '0',
  `TEMP_XZ` float DEFAULT NULL COMMENT '修正值',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `CreateTime` (`CreateTime`),
  KEY `Search_index` (`CreateTime`,`NOISE`,`HUMI`,`WS`,`PM25`,`PM10`,`WD`,`TEMP`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8;
