using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.Data {
  public interface IBaseEntity {
	Guid Id { get; }
	DateTime CreatedAt { get; }
	DateTime? UpdatedAt { get; }
	bool IsDeleted { get; }
	DateTime? DeletedAt { get; }

	void UpdateCreationProperties(DateTime createdAt);
	void UpdateModifiedProperties(DateTime? updatedAt);
	void UpdateIsDeleted(DateTime? deletedAt, bool isDeleted);
  }

  public abstract class BaseEntity : IBaseEntity {
	[Key]
	public Guid Id { get; private set; } = Guid.NewGuid();

	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; private set; } = DateTime.UtcNow;
	/*public string UpdatedBy { get; private set; } = null!;*/

	public bool IsDeleted { get; private set; }
	public DateTime? DeletedAt { get; private set; }

	public void UpdateCreationProperties(DateTime createdAt) {
	  CreatedAt = createdAt;
	  /*CreatedBy = createdBy;*/
	}

	public void UpdateModifiedProperties(DateTime? updatedAt) {
	  UpdatedAt = updatedAt;
	  /*UpdatedBy = lastModifiedBy;*/
	}

	public void UpdateIsDeleted(DateTime? deletedAt, bool isDeleted) {
	  IsDeleted = isDeleted;
	}
  }
}