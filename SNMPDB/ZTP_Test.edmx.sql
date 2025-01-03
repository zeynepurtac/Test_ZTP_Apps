
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 12/11/2024 08:58:29
-- Generated from EDMX file: C:\Users\Mustafa.Tugtekin\Documents\GitHub\Test-ZTP\SNMPDB\ZTP_Test.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ZTP_Test];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Basic_Setup]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Basic_Setup];
GO
IF OBJECT_ID(N'[dbo].[Company]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Company];
GO
IF OBJECT_ID(N'[dbo].[Data]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Data];
GO
IF OBJECT_ID(N'[dbo].[Device]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Device];
GO
IF OBJECT_ID(N'[dbo].[GsmNumber]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GsmNumber];
GO
IF OBJECT_ID(N'[dbo].[Install]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Install];
GO
IF OBJECT_ID(N'[dbo].[Sites]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sites];
GO
IF OBJECT_ID(N'[dbo].[sysdiagrams]', 'U') IS NOT NULL
    DROP TABLE [dbo].[sysdiagrams];
GO
IF OBJECT_ID(N'[dbo].[UserLogin]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserLogin];
GO
IF OBJECT_ID(N'[dbo].[UserLogin45]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserLogin45];
GO
IF OBJECT_ID(N'[ZTP_TestModelStoreContainer].[Registered_Router]', 'U') IS NOT NULL
    DROP TABLE [ZTP_TestModelStoreContainer].[Registered_Router];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Company'
CREATE TABLE [dbo].[Company] (
    [Company_ID] int IDENTITY(1,1) NOT NULL,
    [Company_Name] nvarchar(255)  NOT NULL,
    [ID] int IDENTITY(1,1) NOT NULL,
    [Company_Domain] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'GsmNumber'
CREATE TABLE [dbo].[GsmNumber] (
    [GSM_No_ID] int IDENTITY(1,1) NOT NULL,
    [GSM_No] nvarchar(255)  NOT NULL,
    [Status] int  NULL,
    [Company_ID] int  NULL
);
GO

-- Creating table 'Install'
CREATE TABLE [dbo].[Install] (
    [Install_ID] int IDENTITY(1,1) NOT NULL,
    [Ricon_SN] nvarchar(50)  NULL,
    [GSM_No] nvarchar(50)  NULL,
    [Site_Name] nvarchar(50)  NULL,
    [WAN_ip] nvarchar(255)  NULL,
    [Username] nvarchar(50)  NULL,
    [Company_ID] int  NULL,
    [Date_Time] datetime  NULL
);
GO

-- Creating table 'Sites'
CREATE TABLE [dbo].[Sites] (
    [Site_ID] int IDENTITY(1,1) NOT NULL,
    [Site_Name] nvarchar(255)  NULL,
    [Company_ID] int  NULL,
    [Status] int  NULL
);
GO

-- Creating table 'sysdiagrams'
CREATE TABLE [dbo].[sysdiagrams] (
    [name] nvarchar(128)  NOT NULL,
    [principal_id] int  NOT NULL,
    [diagram_id] int IDENTITY(1,1) NOT NULL,
    [version] int  NULL,
    [definition] varbinary(max)  NULL
);
GO

-- Creating table 'Device'
CREATE TABLE [dbo].[Device] (
    [Device_ID] int IDENTITY(1,1) NOT NULL,
    [Device_Type] nvarchar(10)  NULL,
    [Ricon_SN] nvarchar(50)  NULL,
    [Company_ID] int  NULL,
    [Status] int  NULL
);
GO

-- Creating table 'Data'
CREATE TABLE [dbo].[Data] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [GMS_No] nvarchar(255)  NULL,
    [APN_Name] nvarchar(255)  NULL,
    [APN_User] nvarchar(255)  NULL,
    [APN_Password] nvarchar(255)  NULL,
    [NAT] tinyint  NULL,
    [DHCP_Status] tinyint  NULL,
    [DHCP_Start] nvarchar(255)  NULL,
    [DHCP_User] int  NULL,
    [LAN_Eski] nvarchar(250)  NULL,
    [LAN_IP] nvarchar(255)  NULL,
    [LAN_Subnet] nvarchar(255)  NULL,
    [Serial_no] nvarchar(255)  NULL,
    [Datetime] datetime  NULL,
    [Status] tinyint  NULL,
    [UserName] nvarchar(255)  NULL,
    [remark] nvarchar(255)  NULL,
    [Flag] tinyint  NULL,
    [Lokasyon] nvarchar(255)  NULL
);
GO

-- Creating table 'Registered_Router'
CREATE TABLE [dbo].[Registered_Router] (
    [Registered_ID] int IDENTITY(1,1) NOT NULL,
    [Ricon_SN] nvarchar(50)  NULL,
    [GSM_No] nvarchar(50)  NULL,
    [Company_Name] nvarchar(50)  NULL,
    [Site_Name] nvarchar(255)  NULL
);
GO

-- Creating table 'UserLogin'
CREATE TABLE [dbo].[UserLogin] (
    [User_ID] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(255)  NULL,
    [Password] nvarchar(255)  NULL,
    [IsAdmin] bit  NULL,
    [Creator] nvarchar(255)  NULL,
    [Status] nvarchar(max)  NULL,
    [Create_DateTime] datetime  NULL,
    [LocationStatus] nvarchar(max)  NULL,
    [Company_ID] int  NOT NULL
);
GO

-- Creating table 'Basic_Setup'
CREATE TABLE [dbo].[Basic_Setup] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Serial_no] nvarchar(255)  NULL,
    [GMS_No] nvarchar(255)  NULL,
    [APN_Name] nvarchar(255)  NULL,
    [LAN_IP] nvarchar(255)  NULL,
    [LAN_Subnet] nvarchar(255)  NULL,
    [LAN_Eski] nvarchar(250)  NULL,
    [NAT] tinyint  NULL,
    [DHCP_Status] tinyint  NULL,
    [DHCP_Start] nvarchar(255)  NULL,
    [DHCP_User] nvarchar(255)  NULL,
    [Datetime] datetime  NULL,
    [Status] tinyint  NULL,
    [UserName] nvarchar(255)  NULL,
    [remark] nvarchar(255)  NULL,
    [Flag] tinyint  NOT NULL,
    [Company_ID] int  NULL
);
GO

-- Creating table 'UserLogin45'
CREATE TABLE [dbo].[UserLogin45] (
    [User_ID] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(255)  NULL,
    [Password] nvarchar(255)  NULL,
    [IsAdmin] bit  NULL,
    [Creator] nvarchar(255)  NULL,
    [Status] nvarchar(255)  NULL,
    [Create_DateTime] datetime  NULL,
    [LocationStatus] nvarchar(255)  NULL,
    [Company_ID] nvarchar(255)  NOT NULL,
    [Company_Name] nvarchar(255)  NULL,
    [Company_Domain] nvarchar(255)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Company_ID], [Company_Name], [ID] in table 'Company'
ALTER TABLE [dbo].[Company]
ADD CONSTRAINT [PK_Company]
    PRIMARY KEY CLUSTERED ([Company_ID], [Company_Name], [ID] ASC);
GO

-- Creating primary key on [GSM_No_ID] in table 'GsmNumber'
ALTER TABLE [dbo].[GsmNumber]
ADD CONSTRAINT [PK_GsmNumber]
    PRIMARY KEY CLUSTERED ([GSM_No_ID] ASC);
GO

-- Creating primary key on [Install_ID] in table 'Install'
ALTER TABLE [dbo].[Install]
ADD CONSTRAINT [PK_Install]
    PRIMARY KEY CLUSTERED ([Install_ID] ASC);
GO

-- Creating primary key on [Site_ID] in table 'Sites'
ALTER TABLE [dbo].[Sites]
ADD CONSTRAINT [PK_Sites]
    PRIMARY KEY CLUSTERED ([Site_ID] ASC);
GO

-- Creating primary key on [diagram_id] in table 'sysdiagrams'
ALTER TABLE [dbo].[sysdiagrams]
ADD CONSTRAINT [PK_sysdiagrams]
    PRIMARY KEY CLUSTERED ([diagram_id] ASC);
GO

-- Creating primary key on [Device_ID] in table 'Device'
ALTER TABLE [dbo].[Device]
ADD CONSTRAINT [PK_Device]
    PRIMARY KEY CLUSTERED ([Device_ID] ASC);
GO

-- Creating primary key on [ID] in table 'Data'
ALTER TABLE [dbo].[Data]
ADD CONSTRAINT [PK_Data]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [Registered_ID] in table 'Registered_Router'
ALTER TABLE [dbo].[Registered_Router]
ADD CONSTRAINT [PK_Registered_Router]
    PRIMARY KEY CLUSTERED ([Registered_ID] ASC);
GO

-- Creating primary key on [User_ID] in table 'UserLogin'
ALTER TABLE [dbo].[UserLogin]
ADD CONSTRAINT [PK_UserLogin]
    PRIMARY KEY CLUSTERED ([User_ID] ASC);
GO

-- Creating primary key on [ID] in table 'Basic_Setup'
ALTER TABLE [dbo].[Basic_Setup]
ADD CONSTRAINT [PK_Basic_Setup]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [User_ID] in table 'UserLogin45'
ALTER TABLE [dbo].[UserLogin45]
ADD CONSTRAINT [PK_UserLogin45]
    PRIMARY KEY CLUSTERED ([User_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------