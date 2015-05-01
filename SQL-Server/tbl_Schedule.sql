USE [Support]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [autostarter].[Schedule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ID_Autostart] [int] NOT NULL,
	[Aktion] [nvarchar](10) NOT NULL,
	[Uhrzeit] [nchar](5) NOT NULL,
	[Aktiv] [bit] NOT NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [autostarter].[Schedule] ADD  CONSTRAINT [DF_Schedule_Aktion]  DEFAULT (N'START') FOR [Aktion]
GO

ALTER TABLE [autostarter].[Schedule] ADD  CONSTRAINT [DF_Schedule_Aktiv]  DEFAULT ((1)) FOR [Aktiv]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'START, STOP, RESTART' , @level0type=N'SCHEMA',@level0name=N'autostarter', @level1type=N'TABLE',@level1name=N'Schedule', @level2type=N'COLUMN',@level2name=N'Aktion'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'HH:MM' , @level0type=N'SCHEMA',@level0name=N'autostarter', @level1type=N'TABLE',@level1name=N'Schedule', @level2type=N'COLUMN',@level2name=N'Uhrzeit'
GO


