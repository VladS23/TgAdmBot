CREATE TABLE `chats` (
  `Number` int(10) UNSIGNED NOT NULL,
  `ID` varchar(1024) NOT NULL,
  `IsVIP` tinyint(1) NOT NULL,
  `voiceMessangeBlock` tinyint(1) NOT NULL,
  `Chat_Rules` varchar(10000) NOT NULL DEFAULT 'Правила чата не созданы /setrules для создания',
  `WarnLimAction` varchar(1000) NOT NULL DEFAULT 'mute'
) ENGINE=MyISAM DEFAULT CHARSET=utf8;  
ALTER TABLE `chats`
  ADD UNIQUE KEY `Id` (`Number`);  
ALTER TABLE `chats`
  MODIFY `Number` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;
  
  
  
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
  `LastActivity` varchar(100) NOT NULL,
  `UnMuteTime` varchar(1000) NOT NULL DEFAULT 'NotInMute'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;  

ALTER TABLE `users`
  ADD PRIMARY KEY (`Number`);  

ALTER TABLE `users`
  MODIFY `Number` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;
