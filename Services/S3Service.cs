using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IConfiguration configuration)
    {
        _bucketName = configuration["AWS:BucketName"];
        var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
        _s3Client = new AmazonS3Client(region);
    }

    public async Task<string> UploadFileAsync(Stream inputStream, string fileName)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);

        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
        {
            InputStream = inputStream,
            Key = fileName,
            BucketName = _bucketName,
            ContentType = "image/jpeg"
        };

        await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
        return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{fileName}";
    }

    public async Task<GetObjectResponse> GetFileAsync(string fileName)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName
        };

        return await _s3Client.GetObjectAsync(request);
    }
}
