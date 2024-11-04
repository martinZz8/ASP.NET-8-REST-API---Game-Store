using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class GameGenre
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [InverseProperty(nameof(GameGenreConnection.GameGenre))]
        public ICollection<GameGenreConnection> GameGenreConnections { get; set; }
    }
}
