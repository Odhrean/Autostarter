USE [Support]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [autostarter].[AUTOSTART](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Hostname] [nvarchar](50) NOT NULL,				-- PC Name
	[Prozess_Name] [nvarchar](50) NOT NULL,			-- Prozess-Name, frei waehlbar... muss in Kombination mit PC-Name eindeutig sein
	[Programm] [nvarchar](255) NOT NULL,			-- Kompletter Pfad auf das zu startende Programm
	[Argumente] [nvarchar](255) NULL,				-- Optionale Argumente
	[WindowStyle] int NULL,							-- Darstellung des neuen Fensters beim starten, Default NORMAL
	[Reload_Time_Sec] [int] NOT NULL,				-- Reload-Zeit für Programm, 0= Immer aktiv
	[Aktiv] [bit] NOT NULL,
	[TIME_NEU] [datetime] NULL,
	[USER_NEU] [nvarchar](100) NULL,
	[TIME_AEN] [datetime] NULL,
	[USER_AEN] [nvarchar](100) NULL,
 CONSTRAINT [PK_AUTOSTART] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Unique_Prozess] UNIQUE NONCLUSTERED 
(
	[Hostname] ASC,
	[Prozess_Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [autostarter].[AUTOSTART] ADD  CONSTRAINT [DF_AUTOSTART_Reload_Time_Sec]  DEFAULT ((0)) FOR [Reload_Time_Sec]
GO

ALTER TABLE [autostarter].[AUTOSTART] ADD  CONSTRAINT [DF_AUTOSTART_Aktiv]  DEFAULT ((1)) FOR [Aktiv]
GO

ALTER TABLE [autostarter].[AUTOSTART]  WITH CHECK ADD  CONSTRAINT [FK_AUTOSTART_WindowStyle] FOREIGN KEY([WindowStyle])
REFERENCES [autostarter].[WindowStyle] ([ID])
GO

ALTER TABLE [autostarter].[AUTOSTART] CHECK CONSTRAINT [FK_AUTOSTART_WindowStyle]
GO

IF OBJECT_ID (N'[autostarter].DWH_AUTOSTART_INSERT') IS NOT NULL
    DROP TRIGGER [autostarter].DWH_AUTOSTART_INSERT;
GO

CREATE TRIGGER [autostarter].DWH_AUTOSTART_INSERT
   ON  [autostarter].[AUTOSTART]
   AFTER INSERT
AS 
BEGIN

	SET NOCOUNT ON;
	UPDATE [autostarter].[AUTOSTART]
		SET USER_NEU = ORIGINAL_LOGIN(),
			TIME_NEU = GETDATE(),
			USER_AEN = ORIGINAL_LOGIN(),
			TIME_AEN = GETDATE()
	WHERE ID in (
		SELECT ID FROM inserted
	)
	SET NOCOUNT OFF;

END
GO

IF OBJECT_ID (N'[autostarter].DWH_AUTOSTART_UPDATE') IS NOT NULL
    DROP TRIGGER [autostarter].DWH_AUTOSTART_UPDATE;
GO
CREATE TRIGGER [autostarter].DWH_AUTOSTART_UPDATE
   ON  [autostarter].[AUTOSTART]
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	UPDATE [autostarter].[AUTOSTART]
		SET USER_AEN = ORIGINAL_LOGIN(),
			TIME_AEN = GETDATE()
	WHERE ID in (
		SELECT ID FROM inserted
	)
	SET NOCOUNT OFF;
END


