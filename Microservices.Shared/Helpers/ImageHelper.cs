using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Microservices.Shared.Helpers;

public partial class ImageHelper
{
    public static string SaveImageWithName(IFormFile file, string name, string uploadRootPath)
    {
        if (file == null || file.Length == 0) return "";

        // Sanitize name to make it file-system safe
        string safeName = MyRegex().Replace(name.Trim(), "");

        string extension = Path.GetExtension(file.FileName);
        string uniqueFileName = $"{safeName}_{Guid.NewGuid()}{extension}";

        var uploadsFolder = Path.Combine(uploadRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return $"/uploads/{uniqueFileName}";
    }

    [GeneratedRegex("[^a-zA-Z0-9_-]")]
    private static partial Regex MyRegex();
}
