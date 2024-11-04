Project based on tutorial: https://www.youtube.com/watch?v=AhAxLiGC7Pc

-- Useful PackageManagerConsole (PM) commands: --
** Code-First approach **
1. add-migration "<name>"
EF: Create new migration with the specified name.

2. update-database
EF: Update the database with created migrations.

3. update-database <previous_migration_name_or_id>
EF: Update database by rolling-back changes. We specify the destination migration (that should be used for now) by it's id (first numerous part before underscore) or name (second part after underscore).
from: https://code-maze.com/efcore-how-to-revert-a-migration/

4. remove-migration
EF: Removes last migration (there's need to use 3rd option before this operation, if "update-database" for this migration was run)
from: https://code-maze.com/efcore-how-to-revert-a-migration/

5. update-package -reinstall
Reinstal the packages with NuGet (e.g. when project doesn't run after cloning)

** Database-First approach **
6. Scaffold-DbContext `
-Connection "Server=(localdb)\\MSSQLLocalDB;Database=GameStoreApi;Trusted_Connection=True;TrustServerCertificate=True" `
-Provider Microsoft.EntityFrameworkCore.SqlServer `
-OutputDir Models `
-Context ApplicationDbContext
Creating model from database (from: https://www.youtube.com/watch?v=SrEjoJ_G6tc)
Note: Here model classes will be created together with context class (and placed inside the "Models" folder)

Available connection strings:
- Server=(localdb)\\MSSQLLocalDB;Database=GameStoreApi;Trusted_Connection=True;TrustServerCertificate=True
- Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameStoreApi;Integrated Security=SSPI

-- Administrator account credentials --
email: admin@gmail.com
password: abc