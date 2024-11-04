DECLARE @UserId UNIQUEIDENTIFIER;
SET @UserId = NEWID();
INSERT INTO Users (Id, Email, Username, PasswordHash, PasswordSalt, DateOfBirth, CreateDate, UpdateDate) VALUES (@UserId, 'admin@gmail.com', 'admin', 'ww8K5+wULYQ4YSchu62dFuracr6dFYn7wBeoNKaQ0eI=', '6b0b9f43-4cba-4e6d-93fc-134366c7c03c', '1999-01-17', GETUTCDATE(), GETUTCDATE());
INSERT INTO UserUserRoleConnections (Id, UserId, UserRoleId) VALUES (NEWID(), @UserId, (SELECT Id from UserRoles WHERE Name = 'administrator'));
--Note: We use here password "abc" stored in database as Base64 string (one character is 6 bits long, and we store 44 characters in db)