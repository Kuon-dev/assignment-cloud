// UnitModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models
{
    public class UnitModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public PropertyModel? Property { get; set; }

        [Required]
        public string UnitNumber { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        // Navigation properties
        public ICollection<LeaseModel>? Leases { get; set; }
    }
}
