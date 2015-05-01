USE [Support]
GO

/****** Object:  Table [autostarter].[WindowStyle]    Script Date: 28.04.2015 11:50:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [autostarter].[WindowStyle](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WindowStyle] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_WindowStyle] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


INSERT INTO [autostarter].[WindowStyle] (WindowStyle) Values ('Hidden');
INSERT INTO [autostarter].[WindowStyle] (WindowStyle) Values ('Maximized');
INSERT INTO [autostarter].[WindowStyle] (WindowStyle) Values ('Minimized');
INSERT INTO [autostarter].[WindowStyle] (WindowStyle) Values ('Normal');