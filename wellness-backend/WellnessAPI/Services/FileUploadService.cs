namespace WellnessAPI.Services;

public class FileUploadService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env) => _env = env;

    public async Task<string?> UploadFileAsync(IFormFile? file, string folder = "uploads")
    {
        if (file == null || file.Length == 0 || file.Length > MaxFileSizeBytes)
            return null;

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension) || !AllowedContentTypes.Contains(file.ContentType))
            return null;

        await using var sniffStream = file.OpenReadStream();
        if (!await HasValidImageSignatureAsync(sniffStream))
            return null;

        var uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", folder);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        return $"/{folder}/{fileName}";
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return;

        var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", relativePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private static async Task<bool> HasValidImageSignatureAsync(Stream stream)
    {
        var buffer = new byte[12];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
        if (bytesRead < 4)
            return false;

        var isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8;
        var isPng = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
        var isWebp = bytesRead >= 12
                     && buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46
                     && buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;

        return isJpeg || isPng || isWebp;
    }
}
