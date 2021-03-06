﻿CREATE TABLE [dbo].[MstTags] (
    [TagId]            INT          IDENTITY (0, 1) NOT NULL,
    [TagName]          VARCHAR (50) NOT NULL,
    [InsertedDateTime] DATETIME     NOT NULL,
    [UpdatedDateTime]  DATETIME     NOT NULL,
    CONSTRAINT [PK_MstTags] PRIMARY KEY NONCLUSTERED ([TagId] ASC)
);

