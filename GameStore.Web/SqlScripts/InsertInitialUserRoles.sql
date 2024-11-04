INSERT INTO UserRoles (Id, Name) VALUES (NEWID(), 'administrator'), (NEWID(), 'user');
--Note: We could also set "Id" column in here by fixed ids
-- Old ids:
-- - admin: ('D0522455-A37C-46AC-80B8-9E7C676D349D', 'administrator'),
-- - user: ('740FE3BA-80EB-4812-9752-4F9BE5A53B86', 'user')