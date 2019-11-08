/*
Navicat MariaDB Data Transfer

Source Server         : localhost_3306
Source Server Version : 100309
Source Host           : localhost:3306
Source Database       : trickemu

Target Server Type    : MariaDB
Target Server Version : 100309
File Encoding         : 65001

Date: 2019-11-08 01:00:03
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for characters
-- ----------------------------
DROP TABLE IF EXISTS `characters`;
CREATE TABLE `characters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `authority` int(11) DEFAULT 0,
  `level` int(3) DEFAULT 1,
  `money` int(10) unsigned DEFAULT 0,
  `health` int(11) DEFAULT 100,
  `mana` int(11) DEFAULT 100,
  `map` int(11) DEFAULT 33,
  `pos_x` int(11) DEFAULT 768,
  `pos_y` int(11) DEFAULT 768,
  `job` tinyint(3) NOT NULL,
  `type` tinyint(3) NOT NULL,
  `ftype` tinyint(3) NOT NULL,
  `hair` tinyint(3) NOT NULL,
  `build` varchar(7) NOT NULL,
  `slot` tinyint(3) NOT NULL,
  `job2` tinyint(3) DEFAULT 0,
  `job2_type` tinyint(3) DEFAULT 0,
  `job3` tinyint(3) DEFAULT 0,
  `job3_type` tinyint(3) DEFAULT 0,
  `create_time` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=100000010 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of characters
-- ----------------------------
INSERT INTO `characters` VALUES ('100000001', '100000003', 'Raymonf', '0', '1', '0', '100', '100', '33', '768', '768', '2', '2', '0', '10', '4,1,1,4', '0', '0', '0', '0', '0', '2019-11-05 13:41:22');

-- ----------------------------
-- Table structure for char_equip
-- ----------------------------
DROP TABLE IF EXISTS `char_equip`;
CREATE TABLE `char_equip` (
  `id` int(11) NOT NULL,
  `ears` bigint(11) DEFAULT 0,
  `tail` bigint(11) DEFAULT 0,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of char_equip
-- ----------------------------

-- ----------------------------
-- Table structure for item
-- ----------------------------
DROP TABLE IF EXISTS `item`;
CREATE TABLE `item` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `owner` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `item_count` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2010000001 DEFAULT CHARSET=latin1 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Records of item
-- ----------------------------

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL DEFAULT '0',
  `password` varchar(255) NOT NULL DEFAULT '0',
  `char_slots` int(11) NOT NULL DEFAULT 4,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=100000004 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of users
-- ----------------------------
INSERT INTO `users` VALUES ('100000003', 'raymonf', 'testing', '4');
