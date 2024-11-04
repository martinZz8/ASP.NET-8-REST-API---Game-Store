using GameStore.Web.DbContexts;
using GameStore.Web.Dtos.UserRole;
using GameStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using GameStore.Web.Helpers;

namespace GameStore.Web.Services
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRoleDto>> GetAllUserRoles();
        Task<UserRoleDto?> GetUserRoleById(Guid id);
        Task<UserRoleDto?> GetUserRoleByName(string name);
        Task<UserRoleDto?> AddUserRole(AddUserRoleDto userRoleDto);
        Task<UserRoleDto?> UpdateUserRole(Guid id, UpdateUserRoleDto userRoleDto);
        Task<bool> DeleteUserRoleById(Guid id);
        Task<bool> DeleteUserRoleByName(string name);
    }

    public class UserRoleService : IUserRoleService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRoleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserRoleDto>> GetAllUserRoles()
        {
            IEnumerable<UserRole> userRoles = await _dbContext.UserRoles.ToListAsync();

            return userRoles.ToDto();
        }

        public async Task<UserRoleDto?> GetUserRoleById(Guid id)
        {
            UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(x => x.Id.Equals(id));

            return foundUserRole?.ToDto();
        }

        public async Task<UserRoleDto?> GetUserRoleByName(string name)
        {
            UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(x => x.Name.Equals(name));

            return foundUserRole?.ToDto();
        }

        public async Task<UserRoleDto?> AddUserRole(AddUserRoleDto userRoleDto)
        {
            // Find if we currently have user role that uses given name
            // If yes, return null object (which means error)
            UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(it => it.Name.Equals(userRoleDto.Name));

            if (foundUserRole != null)
            {
                return null;
            }
            // ... otherwise add new user role
            UserRole newUserRole = new UserRole()
            {
                Name = userRoleDto.Name
            };

            await _dbContext.AddAsync(newUserRole);
            await _dbContext.SaveChangesAsync();

            return newUserRole.ToDto();
        }

        public async Task<UserRoleDto?> UpdateUserRole(Guid id, UpdateUserRoleDto userRoleDto)
        {
            // Find if we currently have user role that uses given name
            // If no, return null object (which means error)
            UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundUserRole == null)
            {
                return null;
            }
            // ... otherwise update this user role
            foundUserRole.Name = userRoleDto.Name;

            await _dbContext.SaveChangesAsync();
            return foundUserRole.ToDto();
        }

        public async Task<bool> DeleteUserRoleById(Guid id)
        {
            // Find if we currently have user role that uses given id
            // If no, return false (which means error)
            UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundUserRole == null)
            {
                return false;
            }
            // ... otherwise delete found object
            _dbContext.UserRoles.Remove(foundUserRole);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserRoleByName(string name)
        {
            // Find if we currently have user role that uses given name
            // If no, return false (which means error)
            UserRole? foundUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(it => it.Name.Equals(name));

            if (foundUserRole == null)
            {
                return false;
            }
            // ... otherwise delete found object
            _dbContext.UserRoles.Remove(foundUserRole);
            await _dbContext.SaveChangesAsync();
            return true;
        }        
    }
}
