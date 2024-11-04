using GameStore.Web.Dtos.UserRole;
using GameStore.Web.Models;

namespace GameStore.Web.Helpers
{
    public static class UserRoleMapper
    {
        // UserRole to UserRoleDto
        public static UserRoleDto ToDto(this UserRole userRole)
        {
            return new UserRoleDto()
            {
                Id = userRole.Id,
                Name = userRole.Name
            };
        }

        public static IEnumerable<UserRoleDto> ToDto(this IEnumerable<UserRole> userRoles)
        {
            return userRoles.Select(it => it.ToDto());
        }
    }
}
