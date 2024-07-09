using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Cloud.Models.Validator;

namespace Cloud.Services
{
	/// <summary>
	/// Interface for MediaService
	/// </summary>
	public interface IMediaService
	{
		Task<MediaDto> CreateMediaAsync(CreateMediaDto createMediaDto, string userId);
		Task<MediaDto?> GetMediaByIdAsync(Guid id, string userId);
		Task<List<MediaDto>> GetAllMediaAsync(string userId);
		Task<bool> DeleteMediaAsync(Guid id, string userId);
	}

	/// <summary>
	/// Service for handling Media operations
	/// </summary>
	public class MediaService : IMediaService
	{
		private readonly ApplicationDbContext _context;
		private readonly IS3Service _s3Service;

		/// <summary>
		/// Initializes a new instance of the MediaService class
		/// </summary>
		/// <param name="context">The database context</param>
		/// <param name="s3Service">The S3 service for file operations</param>
		public MediaService(ApplicationDbContext context, IS3Service s3Service)
		{
			_context = context;
			_s3Service = s3Service;
		}

		/// <summary>
		/// Creates a new media entry
		/// </summary>
		/// <param name="createMediaDto">The DTO containing the file to be uploaded</param>
		/// <param name="userId">The ID of the user creating the media</param>
		/// <returns>A MediaDto representing the created media</returns>
		public async Task<MediaDto> CreateMediaAsync(CreateMediaDto createMediaDto, string userId)
		{
			try
			{
				var _validator = new CreateMediaDtoValidator();
				_validator.ValidateMedia(createMediaDto);
				var file = createMediaDto.File;
				var fileName = string.IsNullOrEmpty(createMediaDto.CustomFileName)
					? file.FileName
					: createMediaDto.CustomFileName;

				var fileExtension = Path.GetExtension(file.FileName);
				var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

				using var stream = file.OpenReadStream();
				var filePath = await _s3Service.UploadFileAsync(stream, uniqueFileName, file.ContentType);

				var media = new MediaModel
				{
					FileName = fileName,
					FilePath = filePath,
					FileType = file.ContentType,
					FileSize = file.Length,
					UploadedAt = DateTime.UtcNow,
					UserId = (userId)
				};

				_context.Medias.Add(media);
				await _context.SaveChangesAsync();

				return MapToDto(media);

			}
			catch (ValidationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		/// <summary>
		/// Retrieves a media entry by its ID
		/// </summary>
		/// <param name="id">The ID of the media to retrieve</param>
		/// <param name="userId">The ID of the user requesting the media</param>
		/// <returns>A MediaDto if found, null otherwise</returns>
		public async Task<MediaDto?> GetMediaByIdAsync(Guid id, string userId)
		{
			var media = await _context.Medias
				.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

			return media != null ? MapToDto(media) : null;
		}

		/// <summary>
		/// Retrieves all media entries for a user
		/// </summary>
		/// <param name="userId">The ID of the user</param>
		/// <returns>A list of MediaDto objects</returns>
		public async Task<List<MediaDto>> GetAllMediaAsync(string userId)
		{
			var media = await _context.Medias
				.Where(m => m.UserId == (userId))
				.ToListAsync();

			return media.Select(MapToDto).ToList();
		}

		// get media by path
		public async Task<MediaDto?> GetMediaByPathAsync(string path, string userId)
		{
			var media = await _context.Medias
				.FirstOrDefaultAsync(m => m.FilePath == path && m.UserId == userId);

			return media != null ? MapToDto(media) : null;
		}

		/// <summary>
		/// Deletes a media entry
		/// </summary>
		/// <param name="id">The ID of the media to delete</param>
		/// <param name="userId">The ID of the user requesting the deletion</param>
		/// <returns>True if the media was deleted, false otherwise</returns>
		public async Task<bool> DeleteMediaAsync(Guid id, string userId)
		{
			var media = await _context.Medias
				.FirstOrDefaultAsync(m => m.Id == id && m.UserId == (userId));

			if (media == null)
			{
				return false;
			}

			var fileName = Path.GetFileName(media.FilePath);
			await _s3Service.DeleteFileAsync(fileName);

			_context.Medias.Remove(media);
			await _context.SaveChangesAsync();

			return true;
		}

		private static MediaDto MapToDto(MediaModel media)
		{
			return new MediaDto
			{
				Id = media.Id,
				FileName = media.FileName,
				FilePath = media.FilePath,
				FileType = media.FileType,
				FileSize = media.FileSize,
				UploadedAt = media.UploadedAt
			};
		}
	}
}