using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class GameGenreConnection
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; }

        [ForeignKey(nameof(GameGenreId))]
        public GameGenre GameGenre { get; set; }

        public Guid GameId { get; set; }

        public Guid GameGenreId { get; set; }
    }
}
