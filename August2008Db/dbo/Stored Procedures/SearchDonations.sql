﻿CREATE PROCEDURE [dbo].[SearchDonations]
	@UserId		INT				= NULL,
	@Name		NVARCHAR(50)	= NULL,
	@FromDate	DATETIME		= NULL,
	@ToDate		DATETIME		= NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP(100)
		d.DonationId,
		u.UserId,
		d.Amount,		
		d.DateDonated,
		d.UserMessage,
		UPPER(d.Currency)					AS Currency,
		ISNULL(u.DisplayName, 'Anonymous')	AS DisplayName
	FROM dbo.Donation d WITH (NOLOCK)
	LEFT JOIN dbo.[User] u WITH (NOLOCK) ON d.UserId = u.UserId
	WHERE	IsCompleted = 1	
	AND		(@UserId	IS NULL OR u.UserId			=		@UserId)
	AND		(@Name		IS NULL OR u.DisplayName	LIKE	@Name + '%')
	AND		(@FromDate	IS NULL OR d.DateDonated	>=		@FromDate)
	AND		(@ToDate	IS NULL OR d.DateDonated	<=		@ToDate)
	ORDER BY d.DonationId DESC;
END;
