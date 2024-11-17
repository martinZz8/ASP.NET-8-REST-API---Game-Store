using GameStore.Web.DbContexts;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.User;
using GameStore.Web.Models;
using GameStore.Web.Enums;
using Microsoft.EntityFrameworkCore;
using GameStore.Web.Helpers;
using GameStore.Web.Consts;
using GameStore.Web.Helpers.AppsettingsLoader;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using GameStore.Web.Dtos.User.Full;

namespace GameStore.Web.Services
{
    public interface IUserService
    {
        Task<ResultRegisterUserDto> RegisterUser(RegisterUserDto userDto);
        Task<ResultLoginUserDto> LoginUser(LoginUserDto userDto);
        Task<UserDtoFull?> GetUserById(Guid id);
        Task<IEnumerable<UserDtoFull>> GetAllUsers();
        Task<ResultUpdateUserDto> UpdateUser(Guid id, UpdateUserDto userDto);
        Task<bool> DeleteUserById(Guid id);
    }

    public class UserService: IUserService
    {        
        private readonly ApplicationDbContext _dbContext;
        private readonly Regex _emailRegex;

        public UserService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _emailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        }

        public async Task<ResultRegisterUserDto> RegisterUser(RegisterUserDto userDto)
        {
            // Check if user with given email exists. If yes, return an error (becasue email need to be unique)
            User? foundUser = await _dbContext.Users.FirstOrDefaultAsync(it => it.Email.Equals(userDto.Email));

            if (foundUser != null)
            {
                return new ResultRegisterUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.REGISTER),
                        Message = "Error during register of new user. The user with given email already exists."
                    }
                };
            }

            // Check if there's at least one roleName specified (there's need to specify at least one roleName)
            if (userDto.Roles.Count() == 0)
            {
                return new ResultRegisterUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.REGISTER),
                        Message = "Error during register of new user. There's need to specify at least one role."
                    }
                };
            }

            // Check if inside given roles there is an "administrator". If yes, return an error (we cannot create admin directly by this endpoint - create admin user manually using SQL INSERT query defined in "InsertAdminUser.sql").
            bool doesContainAdministratorRole = false;
            foreach (string roleName in userDto.Roles)
            {
                if (roleName == UserRoleConsts.ADMINISTRATOR)
                {
                    doesContainAdministratorRole = true;
                    break;
                }
            }

            if (doesContainAdministratorRole)
            {
                return new ResultRegisterUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.REGISTER),
                        Message = "Error during register of new user. Cannot create administrator user using this endpoint."
                    }
                };
            }

            // Check if given email is valid
            // from: https://stackoverflow.com/questions/5342375/regex-email-validation
            if (!IsValidEmail(userDto.Email))
            {
                return new ResultRegisterUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.REGISTER),
                        Message = "Error during register of new user. Given email has invalid format."
                    }
                };
            }

            // Gather roles entities from db
            List<UserRole> userRolesToAdd = new List<UserRole>();

            foreach (string roleName in userDto.Roles)
            {
                // Note: probably we can use here "AsNoTracking()" to "UserRoles" entities (because we don't change them) - to be checked
                UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(it => it.Name.Equals(roleName));

                if (foundUserRole == null)
                {
                    return new ResultRegisterUserDto()
                    {
                        ErrorDto = new ErrorDto()
                        {
                            ErrorTypeName = Enum.GetName(ErrorTypeEnum.REGISTER),
                            Message = $"Error during register of new user. Specified at least one user role ('{roleName}') that doesn't exist in db."
                        }
                    };
                }

                userRolesToAdd.Add(foundUserRole);
            }

            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
            {
                // Count password hash (using salt)
                // from: https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp
                string passwordSalt = GenerateSalt();
                string passwordHash = GenerateSaltedHashString(userDto.Password, passwordSalt);

                // Create user (and save to db)
                User newUser = new User()
                {
                    Email = userDto.Email,
                    Username = userDto.Username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    DateOfBirth = userDto.DateOfBirth
                };

                await _dbContext.Users.AddAsync(newUser);
                await _dbContext.SaveChangesAsync();

                // ... and add to him found user roles (and save to db)
                foreach (UserRole userRole in userRolesToAdd)
                {
                    UserUserRoleConnection newUserUserRoleConnection = new UserUserRoleConnection()
                    {
                        User = newUser,
                        UserRole = userRole
                    };

                    await _dbContext.UserUserRoleConnections.AddAsync(newUserUserRoleConnection);
                }

                await _dbContext.SaveChangesAsync();
                await dbContextTransaction.CommitAsync();
                return new ResultRegisterUserDto();
            }            
        }

        public async Task<ResultLoginUserDto> LoginUser(LoginUserDto userDto)
        {
            // Check if user with given email exists. If no, return error
            User? foundUser = await _dbContext.Users
                .Include(it => it.UserRoleConnections)
                .ThenInclude(it => it.UserRole)
                .FirstOrDefaultAsync(it => it.Email.Equals(userDto.Email));

            if (foundUser == null)
            {
                return new ResultLoginUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.LOGIN),
                        Message = "Error during login of user. User with given email doesn't exist."
                    }
                };
            }

            // Check user's password. If they're not consistend with given password, return an error
            // Note: We compare here byte arrays. But we could also retrieve Base64String from db, compute hash based on givern user's password, change it to Base64String and compare both strings.
            byte[] userStoredPasswordHashBytes = Convert.FromBase64String(foundUser.PasswordHash);
            byte[] usersGivenPasswordHashBytes = GenerateSaltedHash(userDto.Password, foundUser.PasswordSalt);

            if (!CompareByteArrays(userStoredPasswordHashBytes, usersGivenPasswordHashBytes))
            {
                return new ResultLoginUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.LOGIN),
                        Message = "Error during login of user. Given user's password is wrong."
                    }
                };
            }

            // If successfully logged in, update last user's "LastLoginDate" to current UTC date
            foundUser.LastLoginDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Create JWT token (based on JWTSecret stored in file "appsettings.json"
            // from: https://www.ais.com/how-to-generate-a-jwt-token-using-net-6/
            string JWT = CreateJWT(foundUser);

            return new ResultLoginUserDto()
            {
                JWT = JWT,
                User = foundUser.ToDto()
            };
        }

        public async Task<UserDtoFull?> GetUserById(Guid id)
        {
            User? foundUser = await _dbContext.Users
                .Include(it => it.UserRoleConnections)
                .ThenInclude(it => it.UserRole)
                .Include(it => it.GameUserCopies)
                .ThenInclude(it => it.Game)
                .AsNoTracking()
                .FirstOrDefaultAsync(it => it.Id.Equals(id));

            return foundUser?.ToDtoFull();
        }

        public async Task<IEnumerable<UserDtoFull>> GetAllUsers()
        {
            IEnumerable<User> users = await _dbContext.Users
                .Include(it => it.UserRoleConnections)
                .ThenInclude(it => it.UserRole)
                .Include(it => it.GameUserCopies)
                .ThenInclude(it => it.Game)
                .AsNoTracking()
                .ToListAsync();

            return users.ToDtoFull();
        }        

        public async Task<ResultUpdateUserDto> UpdateUser(Guid id, UpdateUserDto userDto)
        {
            // Check if user with given id exists
            User? foundUser = await _dbContext.Users
                .Include(it => it.UserRoleConnections)
                .ThenInclude(it => it.UserRole)
                .FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundUser == null)
            {
                return new ResultUpdateUserDto();
            }

            // Check if given email is valid
            if (!IsValidEmail(userDto.Email))
            {
                return new ResultUpdateUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE),
                        Message = "Error during update of user. Given email has invalid format."
                    }
                };
            }

            // Check if given email is not in use
            User? foundUserWithSameEmail = await _dbContext.Users.FirstOrDefaultAsync(it => it.Email.Equals(userDto.Email));

            if (foundUserWithSameEmail != null && foundUserWithSameEmail.Id != id)
            {
                return new ResultUpdateUserDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE),
                        Message = "Error during update of user. User with given email already exists."
                    }
                };
            }

            // Begin database transaction (for user and its userRoles)
            // based on: https://learn.microsoft.com/en-us/ef/ef6/saving/transactions
            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
            {
                // Update user fields (beside the user role connections)
                foundUser.Email = userDto.Email;
                foundUser.Username = userDto.Username;
                foundUser.DateOfBirth = userDto.DateOfBirth;

                // Check if "userDto.Roles" is not null
                if (userDto.Roles != null)
                {
                    // Check if account to be changed has administrator priviledges
                    IEnumerable<string> foundUserRoles = foundUser.UserRoleConnections.Select(it => it.UserRole.Name);
                    if (foundUserRoles.Contains(UserRoleConsts.ADMINISTRATOR))
                    {
                        // ... if yes, check if roles contain "administrator" (it's mandatory, in case to not change admin to not be admin anymore).
                        // If roles doesn't contain this role, return error
                        if (!userDto.Roles.Contains(UserRoleConsts.ADMINISTRATOR))
                        {
                            // Note: "dbContextTransaction.RollbackAsync()" is probably not needed here, since we haven't perfomed "_dbContext.SaveChangesAsync()" together with "dbContextTransaction.CommitAsync()"
                            //await dbContextTransaction.RollbackAsync();
                            return new ResultUpdateUserDto()
                            {
                                ErrorDto = new ErrorDto()
                                {
                                    ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE),
                                    Message = "Error during update of user. Cannot remove 'administrator' role from administrator account."
                                }
                            };
                        }
                    }

                    // Adjust connections to remove missing ones and add new ones
                    // Remove all user role connections, that are not present in given "userDto.Roles"
                    foreach (UserUserRoleConnection userUserRoleConnection in foundUser.UserRoleConnections)
                    {
                        if (!userDto.Roles.Contains(userUserRoleConnection.UserRole.Name))
                        {
                            _dbContext.UserUserRoleConnections.Remove(userUserRoleConnection);
                        }
                    }

                    // Add all user role connections, that are not present in current "foundUser.UserRoleConnections"
                    IEnumerable<string> currentlyExistingUserRoleConnections = foundUser.UserRoleConnections.Select(it => it.UserRole.Name);
                    foreach (string userRoleName in userDto.Roles)
                    {
                        if (!currentlyExistingUserRoleConnections.Contains(userRoleName))
                        {
                            // Find user role by name (if exists, connect it with user)
                            UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(it => it.Name.Equals(userRoleName));

                            if (foundUserRole == null)
                            {
                                return new ResultUpdateUserDto()
                                {
                                    ErrorDto = new ErrorDto()
                                    {
                                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.UPDATE),
                                        Message = $"Error during update of user. Cannot add non existing role '{userRoleName}' for this user."
                                    }
                                }; ;
                            }

                            UserUserRoleConnection newUserRoleConnection = new UserUserRoleConnection()
                            {
                                User = foundUser,
                                UserRole = foundUserRole
                            };

                            await _dbContext.UserUserRoleConnections.AddAsync(newUserRoleConnection);
                        }
                    }
                }
                // ... otherwise don't touch user role connections for this user                               

                await _dbContext.SaveChangesAsync();
                await dbContextTransaction.CommitAsync();
                return new ResultUpdateUserDto()
                {
                    User = foundUser.ToDto()
                };
            }
        }

        public async Task<bool> DeleteUserById(Guid id)
        {
            User? foundUser = await _dbContext.Users.FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundUser == null)
            {
                return false;
            }

            _dbContext.Users.Remove(foundUser);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Check if given email is valid
        // from: https://stackoverflow.com/questions/5342375/regex-email-validation
        private bool IsValidEmail(string email)
        {            
            Match match = _emailRegex.Match(email);
            return match.Success;
        }
        
        // Generate salt function
        // from: https://stackoverflow.com/questions/3063116/how-to-easily-salt-a-password-in-a-c-sharp-windows-form-application
        private string GenerateSalt()
        {
            return Guid.NewGuid().ToString();
        }

        // Generate salted hash function. Salting means adding salt at the end of plain text, that needs to be hashed.
        // from 1 (slightly modified): https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp
        // from 2: https://stackoverflow.com/questions/65036928/why-are-classes-like-sha1managed-sha256managed-hidden-from-my-intellisense
        // from 3: https://stackoverflow.com/questions/7304695/why-does-this-method-for-computing-a-sha-256-hash-always-return-a-string-of-44-c
        private byte[] GenerateSaltedHash(string plainText, string? salt = null)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltBytes = salt != null ?
                Encoding.UTF8.GetBytes(salt)
            :
                Encoding.UTF8.GetBytes(GenerateSalt()); // Generate salt when not given             

            HashAlgorithm algorithm = SHA256.Create(); // old: new SHA256Managed();

            byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];

            for (int i = 0; i < plainTextBytes.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainTextBytes[i];
            }
            for (int i = 0; i < saltBytes.Length; i++)
            {
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        private string GenerateSaltedHashString(string plainText, string? salt = null)
        {
            return Convert.ToBase64String(GenerateSaltedHash(plainText, salt));
        }

        // Compare byte arrays (can be computed hash and stored hash for password)
        // from: https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp
        private bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }

        // Create JWT
        // from 1: https://www.ais.com/how-to-generate-a-jwt-token-using-net-6/
        // from 2: https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
        private string CreateJWT(User user, int? expirationDays = null)
        {
            // At first, check if "AppsettingsLoader.JWTSecret" is not null
            if (AppsettingsLoader.JWTSecret == null)
            {
                throw new Exception("Provide value for key 'JWTSecret' into 'appsettings.json' file");
            }

            // Specify claims for JWT (with roles)
            // specifying multiple roles from: https://stackoverflow.com/questions/49426781/jwt-authentication-by-role-claims-in-asp-net-core-identity
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            foreach (string roleName in user.UserRoleConnections.Select(it => it.UserRole.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppsettingsLoader.JWTSecret));
            SigningCredentials cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // Note: We can add also 'issuer: "https://localhost:7176"' and 'audience: "https://localhost:7176"' arguments to constructor of "JwtSecurityToken"
            // Note2: We can enable/disable requirement of "expires" clumn in payload of JWT during validation of JWT (when used for authentication).
            // This can be done in "Program.cs" file during definint builder service "AddJwtBearer" (in setting parameter "TokenValidationParameters" - "RequireExpirationTime")
            // for more see: https://code-maze.com/authentication-aspnetcore-jwt-1/
            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: expirationDays != null ?
                    DateTime.UtcNow.AddDays((double)expirationDays)
                :
                    null,
                signingCredentials: cred
            );

            string JWT = new JwtSecurityTokenHandler().WriteToken(token);
            return JWT;
        }
    }
}
