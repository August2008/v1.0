﻿CREATE TABLE [dbo].[Donation] (
    [DonationId]			INT IDENTITY (1, 1) NOT NULL,
    [DonationProviderId]	INT				NOT NULL,
    [UserId]				INT				NOT NULL,
	[ExternalId]			NVARCHAR(50)	NULL,
	[ExternalStatus]		NVARCHAR(25)	NULL,
	[IsCompleted]			BIT,
    [Currency]				NVARCHAR (10)	NULL,
    [UserMessage]			NVARCHAR (500)	NULL,
    [DateDonated]			DATETIME		DEFAULT (GETDATE()),
    [Amount]				MONEY			NOT NULL,
    [ProviderData]			XML				NULL,
	[CityId]				INT				NULL,
	[StateId]				INT				NULL,
	[CountryId]				INT				NULL
    CONSTRAINT [PK_Donation] PRIMARY KEY CLUSTERED ([DonationId] ASC),
    CONSTRAINT [FK_Donation_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId]),
    CONSTRAINT [FK_Donation_DonationProvider] FOREIGN KEY ([DonationProviderId]) REFERENCES [dbo].[DonationProvider] ([DonationProviderId]),
	CONSTRAINT [FK_Donation_City] FOREIGN KEY ([CityId]) REFERENCES [dbo].[City]([CityId]),
	CONSTRAINT [FK_Donation_State] FOREIGN KEY ([StateId]) REFERENCES [dbo].[State]([StateId]),
	CONSTRAINT [FK_Donation_Country] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country]([CountryId])
);