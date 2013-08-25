﻿CREATE TABLE [dbo].[Country]
(
	[CountryId]	INT	IDENTITY (1, 1) NOT NULL,
    [Name]		NVARCHAR(10)	NULL, 
    [FullName]	NVARCHAR(50)	NULL,
	[Geo]		GEOGRAPHY		DEFAULT('POINT EMPTY')
	CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED ([CountryId] ASC)
)
