﻿-- 【公共表】
-- 文件名：app_config.sqlite
-- app_xxx 存储 application 级别的信息
-- common_xxx 存储公共信息

-- 所有字段命名都和映射类一致
-- 启动界面管理
-- DataType: 0-Video 1-Picture 2-Game 3-Comics
drop table if exists app_databases;
BEGIN;
create table app_databases (
    DBId INTEGER PRIMARY KEY autoincrement,
    Path TEXT DEFAULT '',
    Name VARCHAR(500),
    Size INTEGER DEFAULT 0,
    Count INTEGER DEFAULT 0,
    DataType INT DEFAULT 0,
    ImagePath TEXT DEFAULT '',
    ViewCount INT DEFAULT 0,

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    unique(DataType, Name),
    unique(Path)
);
CREATE INDEX name_idx ON app_databases (Name);
CREATE INDEX type_idx ON app_databases (DataType);
COMMIT;

insert into app_databases ( Count, Path, Name, Size, ImagePath, ViewCount )
values ( 55, 'C:\123\test.sqlite', 'test', 51344, '123.png', 55);


drop table if exists app_configs;
BEGIN;
create table app_configs (
    ConfigId INTEGER PRIMARY KEY autoincrement,
    ConfigName VARCHAR(100),
    ConfigValue TEXT DEFAULT '',

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX app_configs_name_idx ON app_configs (ConfigName);
COMMIT;


drop table if exists common_recent_view;
create table common_recent_view(
    id INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    ViewDate VARCHAR(30)
);


-- 【存储刮削的图片】
-- PathType: 0-绝对路径 1-相对于Jvedio路径 2-相对于影片路径 3-网络绝对路径
drop table if exists common_images;
create table common_images(
    ImageID INTEGER PRIMARY KEY autoincrement,

    Name VARCHAR(500),
    Path VARCHAR(1000),
    PathType INT DEFAULT 0,
    Ext VARCHAR(100),
    Size INTEGER,
    Height INT,
    Width INT,

    Url TEXT,
    ExtraInfo TEXT,
    Source VARCHAR(100),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),

    unique(PathType,Path)
);

insert into common_images
(Name,Path,PathType,Ext,Size,Height,Width,Url,ExtraInfo,Source)
values ('test','C:\test.jpg',0,'jpg',2431,720,1080,'http://www.demo.com/123.jpg','{"BitDepth":"32"}','IMDB');


-- 【翻译表】
-- Platform 翻译平台：[baidu,youdao,google]
drop table if exists common_transaltions;
create table common_transaltions(
    TransaltionID INTEGER PRIMARY KEY autoincrement,

    SourceLang VARCHAR(100),
    TargetLang VARCHAR(100),
    SourceText TEXT,
    TargetText TEXT,
    Platform VARCHAR(100),

    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);

insert into common_transaltions
(SourceLang,TargetLang,SourceText,TargetText,Platform)
values ('简体中文','English','人是生而自由的','Man is born free','youdao');

-- 【磁力链接】
-- Magnet 40位磁力链接
-- TorrentUrl 种子下载地址
-- VID 视频的 VID
-- Tag 磁力标签
-- DownloadNumber 下载次数
-- ExtraInfo ：{"Seeds":"1","Peers":"0"}
drop table if exists common_magnets;
BEGIN;
create table common_magnets (
    MagnetID INTEGER PRIMARY KEY autoincrement,
    MagnetLink VARCHAR(40),
    TorrentUrl VARCHAR(2000),
    DataID INTEGER,
    Title TEXT,
    Size INTEGER DEFAULT 0,
    Releasedate VARCHAR(10) DEFAULT '1900-01-01',
    Tag TEXT,

    DownloadNumber INT DEFAULT 0,
    ExtraInfo TEXT,

    CreateDate VARCHAR(30) NOT NULL DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) NOT NULL DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),

    unique(MagnetLink)
);
CREATE INDEX common_magnets_idx_DataID ON common_magnets (DataID);
COMMIT;

insert into common_magnets
(Magnet,TorrentUrl,DataID,Title,Size,Releasedate,Tag,DownloadNumber,ExtraInfo)
values ('7c5cd6144ae373fec931f20deabcf25eda85cb40','种子下载地址',5,'磁力链接1',1034.24,'2014-10-30','高清 中文',15,'{"Seeds":"1","Peers":"0"}');

-- 【db和library等识别码和网址的对应关系】
-- web_type : 所属网址 => [db,library,bus]
-- CodeType ：演员对应或影片对应 => [actor,video]
drop table if exists common_url_code;
BEGIN;
create table common_url_code (
    CodeId INTEGER PRIMARY KEY autoincrement,
    LocalValue VARCHAR(500),
    ValueType  VARCHAR(20) DEFAULT 'video',
    RemoteValue VARCHAR(100),
    WebType VARCHAR(100),
    
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
CREATE INDEX common_url_code_idx_VID ON common_url_code (ValueType,WebType,LocalValue);
COMMIT;
insert into common_url_code(LocalValue,RemoteValue,WebType)
values ('ABCD-123','1BKY9','db');


-- Beauty 颜值打分
-- Gender 性别
-- Race 人种
-- Mask 口罩/面具 0-否 1-是
-- Glasses 是否戴眼镜
drop table if exists common_ai_face;
BEGIN;
create table common_ai_face (
    AIId INTEGER PRIMARY KEY autoincrement,
    Age INT DEFAULT 0,
    Beauty FLOAT DEFAULT 0,
    Expression VARCHAR(100),
    FaceShape VARCHAR(100),
    Gender INT DEFAULT 0,
    Glasses INT DEFAULT 0,
    Race VARCHAR(100),
    Emotion VARCHAR(100),
    Mask INT DEFAULT 0,
    Platform VARCHAR(100),
    
    ExtraInfo TEXT,
    CreateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime')),
    UpdateDate VARCHAR(30) DEFAULT(STRFTIME('%Y-%m-%d %H:%M:%S', 'NOW', 'localtime'))
);
COMMIT;