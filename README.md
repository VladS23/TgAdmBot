# TgAdmBot
 Update the Bottoken before using it
 ### Database structure
Table structure for table `chats`

CREATE TABLE `chats` (
  `Number` int(10) UNSIGNED NOT NULL,
  `ID` varchar(1024) NOT NULL,
  `IsVIP` tinyint(1) NOT NULL,
  `voiceMessangeBlock` tinyint(1) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=utf8;


Table structure for table `users`

CREATE TABLE `users` (
  `Number` int(11) NOT NULL,
  `ID` varchar(1024) NOT NULL,
  `Admin` varchar(100) NOT NULL,
  `Chat_id` varchar(1024) NOT NULL,
  `Nickname` varchar(1024) NOT NULL,
  `User_ID` varchar(1024) NOT NULL,
  `IsMute` tinyint(1) NOT NULL,
  `Warns` int(5) NOT NULL,
  `MessageCount` int(11) NOT NULL,
  `VoiceMessageCount` int(11) NOT NULL,
  `StikerCount` int(11) NOT NULL,
  `LastActivity` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
