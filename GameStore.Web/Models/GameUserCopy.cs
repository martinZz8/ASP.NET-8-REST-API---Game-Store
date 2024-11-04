using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class GameUserCopy
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }        

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public Guid GameId { get; set; }

        public Guid UserId { get; set; }
        
    }
}
