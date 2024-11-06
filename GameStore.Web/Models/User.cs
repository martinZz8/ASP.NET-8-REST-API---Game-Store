using GameStore.Web.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class User: IEntityWithDates
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [InverseProperty(nameof(GameUserCopy.User))]
        public ICollection<GameUserCopy> GameUserCopies { get; set; }

        [InverseProperty(nameof(UserUserRoleConnection.User))]
        public ICollection<UserUserRoleConnection> UserRoleConnections { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
