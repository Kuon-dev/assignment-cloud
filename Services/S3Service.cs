// File: S3Service.cs
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
// using Microsoft.Extensions.Configuration;
// using System.IO;
// using System.Threading.Tasks;

public class S3Service
{
	private readonly IAmazonS3 _s3Client;
	private readonly string? _bucketName;

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

	public async Task<bool> UpdateFileAsync(Stream inputStream, string fileName, string contentType = "image/jpeg")
	{
		var existingFile = await GetFileAsync(fileName);
		if (existingFile == null)
		{
			return false;
		}
		await UploadFileAsync(inputStream, fileName, contentType);
		return true;
	}

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