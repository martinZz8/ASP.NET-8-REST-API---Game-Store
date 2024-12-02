using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Web.Models
{
    public class GameFileDescription
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; }

        [Required]
        public Guid GameId { get; set; }
    }
}
