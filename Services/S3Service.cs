using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Cloud.Services
{
	/// <summary>
	/// Interface for S3 service operations
	/// </summary>
	public interface IS3Service
	{
		/// <summary>
		/// Uploads a file to S3
		/// </summary>
		/// <param name="inputStream">The input stream of the file</param>
		/// <param name="fileName">The name of the file</param>
		/// <param name="contentType">The content type of the file</param>
		/// <returns>The URL of the uploaded file</returns>
		Task<string> UploadFileAsync(Stream inputStream, string fileName, string contentType = "image/jpeg");

		/// <summary>
		/// Gets a file from S3
		/// </summary>
		/// <param name="fileName">The name of the file to retrieve</param>
		/// <returns>The S3 object response</returns>
		Task<GetObjectResponse> GetFileAsync(string fileName);

		/// <summary>
		/// Updates an existing file in S3
		/// </summary>
		/// <param name="inputStream">The input stream of the updated file</param>
		/// <param name="fileName">The name of the file to update</param>
		/// <param name="contentType">The content type of the file</param>
		/// <returns>True if the update was successful, false otherwise</returns>
		Task<bool> UpdateFileAsync(Stream inputStream, string fileName, string contentType = "image/jpeg");

		/// <summary>
		/// Deletes a file from S3
		/// </summary>
		/// <param name="fileName">The name of the file to delete</param>
		/// <returns>True if the deletion was successful, false otherwise</returns>
		Task<bool> DeleteFileAsync(string fileName);
	}

	/// <summary>
	/// Service for interacting with Amazon S3
	/// </summary>
	public class S3Service : IS3Service
	{
		private readonly IAmazonS3 _s3Client;
		private readonly string? _bucketName;

		/// <summary>
		/// Initializes a new instance of the S3Service
		/// </summary>
		/// <param name="configuration">The configuration to use for S3 settings</param>
		public S3Service(IConfiguration configuration)
		{
			_bucketName = configuration["AWS:BucketName"];
			if (string.IsNullOrEmpty(_bucketName))
			{
				throw new ArgumentException("AWS:BucketName configuration value is required.");
			}

			var regionName = configuration["AWS:Region"];
			if (string.IsNullOrEmpty(regionName))
			{
				throw new ArgumentException("AWS:Region configuration value is required.");
			}

			var region = RegionEndpoint.GetBySystemName(regionName);
			_s3Client = new AmazonS3Client(region);
		}

		/// <inheritdoc/>
		public async Task<string> UploadFileAsync(Stream inputStream, string fileName, string contentType = "image/jpeg")
		{
			if (_bucketName == null)
			{
				throw new InvalidOperationException("Bucket name is not set.");
			}

			var fileTransferUtility = new TransferUtility(_s3Client);
			var fileTransferUtilityRequest = new TransferUtilityUploadRequest
			{
				InputStream = inputStream,
				Key = fileName,
				BucketName = _bucketName,
				ContentType = contentType
			};

			await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
			return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{fileName}";
		}

		/// <inheritdoc/>
		public async Task<GetObjectResponse> GetFileAsync(string fileName)
		{
			if (_bucketName == null)
			{
				throw new InvalidOperationException("Bucket name is not set.");
			}

			var request = new GetObjectRequest
			{
				BucketName = _bucketName,
				Key = fileName
			};

			return await _s3Client.GetObjectAsync(request);
		}

		/// <inheritdoc/>
		public async Task<bool> UpdateFileAsync(Stream inputStream, string fileName, string contentType = "image/jpeg")
		{
			try
			{
				var existingFile = await GetFileAsync(fileName);
				await UploadFileAsync(inputStream, fileName, contentType);
				return true;
			}
			catch (AmazonS3Exception)
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public async Task<bool> DeleteFileAsync(string fileName)
		{
			if (_bucketName == null)
			{
				throw new InvalidOperationException("Bucket name is not set.");
			}

			var deleteObjectRequest = new DeleteObjectRequest
			{
				BucketName = _bucketName,
				Key = fileName
			};

			var response = await _s3Client.DeleteObjectAsync(deleteObjectRequest);
			return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
		}
	}
}