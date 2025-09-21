using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace PosLite.Pages.Shop;

public class SettingsModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public SettingsModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    [BindProperty]
    public ShopSettings Settings { get; set; } = new();

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    /// <summary>
    /// Get the file path for the settings JSON file
    /// </summary>
    /// <returns></returns>
    private string GetFilePath()
    {
        var configFolder = Path.Combine(_env.WebRootPath, "config");
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);

        return Path.Combine(configFolder, "shopsettings.json");
    }

    /// <summary>
    /// Load settings
    /// </summary>
    public void OnGet()
    {
        var filePath = GetFilePath();
        if (System.IO.File.Exists(filePath))
        {
            var json = System.IO.File.ReadAllText(filePath);
            Settings = JsonSerializer.Deserialize<ShopSettings>(json) ?? new ShopSettings();
        }
    }

    /// <summary>
    /// Save settings
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAsync()
    {
        var filePath = GetFilePath();
        ShopSettings? oldSettings = null;

        if (System.IO.File.Exists(filePath))
        {
            var oldJson = System.IO.File.ReadAllText(filePath);
            oldSettings = JsonSerializer.Deserialize<ShopSettings>(oldJson);
        }

        if (LogoFile != null && LogoFile.Length > 0)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "logo");

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            if (!string.IsNullOrEmpty(oldSettings?.LogoUrl))
            {
                var oldLogoPath = Path.Combine(_env.WebRootPath, oldSettings.LogoUrl.TrimStart('/'));

                if (System.IO.File.Exists(oldLogoPath))
                {
                    System.IO.File.Delete(oldLogoPath);
                }
            }

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(LogoFile.FileName)}";
            var savedPath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(savedPath, FileMode.Create))
            {
                await LogoFile.CopyToAsync(stream);
            }

            Settings.LogoUrl = "/uploads/logo/" + fileName;
        }
        else if (oldSettings != null)
        {
            Settings.LogoUrl = oldSettings.LogoUrl;
        }

        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(filePath, json);

        TempData["Success"] = "Đã lưu cấu hình!";
        return RedirectToPage();
    }
}
