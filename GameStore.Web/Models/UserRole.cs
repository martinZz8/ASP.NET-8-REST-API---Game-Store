using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class UserRole
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [InverseProperty(nameof(UserUserRoleConnection.UserRole))]
        public ICollection<UserUserRoleConnection> UserRoleConnections { get; set; }
    }
}
