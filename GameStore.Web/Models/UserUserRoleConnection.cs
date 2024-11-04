using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class UserUserRoleConnection
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(UserRoleId))]
        public UserRole UserRole { get; set; }

        public Guid UserId { get; set; }

        public Guid UserRoleId { get; set; }
    }
}
