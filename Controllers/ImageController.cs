using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

public class ImageController : Controller {
  private readonly S3Service _s3Service;

  public ImageController(S3Service s3Service) {
	_s3Service = s3Service;
  }

  [HttpGet]
  public IActionResult Upload() {
	return View();
  }

  [HttpPost]
  public async Task<IActionResult> Upload(IFormFile file) {
	if (file == null || file.Length == 0) {
	  return Content("File not selected");
	}

	var fileName = Path.GetFileName(file.FileName);
	using (var stream = new MemoryStream()) {
	  await file.CopyToAsync(stream);
	  var url = await _s3Service.UploadFileAsync(stream, fileName);
	  ViewBag.Message = $"File uploaded successfully: {url}";
	  ViewBag.ImageUrl = url;
	}

	return View();
  }

  [HttpGet]
  public async Task<IActionResult> Download(string fileName) {
	var response = await _s3Service.GetFileAsync(fileName);
	return File(response.ResponseStream, response.Headers["Content-Type"], fileName);
  }
}