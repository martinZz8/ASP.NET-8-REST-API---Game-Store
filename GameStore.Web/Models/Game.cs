using GameStore.Web.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class Game : IEntityWithDates
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public bool OnSale { get; set; }

        [Required]
        public DateOnly ReleaseDate { get; set; }

        [InverseProperty(nameof(GameGenreConnection.Game))]
        public ICollection<GameGenreConnection> GameGenreConnections { get; set; }

        [InverseProperty(nameof(GameUserCopy.Game))]
        public ICollection<GameUserCopy> GameUserCopies { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
