// TenantModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models
{
    public class TenantModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public String UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public UserModel? User { get; set; }

        // Add this property to reference the current property
        public Guid? CurrentPropertyId { get; set; }

        [ForeignKey("CurrentPropertyId")]
        public PropertyModel? CurrentProperty { get; set; }

        // Navigation properties
        public ICollection<RentalApplicationModel>? Applications { get; set; }
        public ICollection<RentPaymentModel>? RentPayments { get; set; }
        public ICollection<MaintenanceRequestModel>? MaintenanceRequests { get; set; }
        public ICollection<LeaseModel>? Leases { get; set; }
    }
}

