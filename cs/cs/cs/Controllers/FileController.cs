using Microsoft.AspNetCore.Mvc;

namespace cs.Controllers;

/// 文件上传
/// 落盘位置: ContentRoot/Resource/Uploads —— Program.cs 里已经把 Resource 挂成静态文件服务,
/// 所以存进去就等于能通过 http://localhost:5108/Resource/Uploads/xxx 直接下载, 不用再写下载接口
/// 返回的 url 是相对路径(和 AvatarUrl 表里的约定一致), 前端自己拼服务器地址
[ApiController]
[Route("api/[controller]")] // api/File
public class FileController : ControllerBase
{
    /// 单个文件上限 20MB(Kestrel 默认 30MB, 这里收得更紧一些)
    private const long MaxFileSize = 20 * 1024 * 1024;

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileController> _logger;

    public FileController(IWebHostEnvironment env, ILogger<FileController> logger)
    {
        _env = env;
        _logger = logger;
    }

    [HttpPost("upload")] // api/File/upload
    [RequestSizeLimit(MaxFileSize + 1024 * 1024)] // 留 1MB 余量给 multipart 边界/头部, 超限由下面的判断给出可读提示
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("上传请求没有带上文件");
            return BadRequest(new { code = 400, message = "没有拿到文件" });
        }

        if (file.Length > MaxFileSize)
        {
            _logger.LogWarning("文件超过上限: size={Size} bytes", file.Length);
            return BadRequest(new { code = 400, message = $"文件太大了, 单个最多 {MaxFileSize / 1024 / 1024}MB" });
        }

        // 存盘名用 Guid: 既避免同名互相覆盖, 也让 URL 不可猜; 原始文件名只作为显示名回给前端
        var displayName = Path.GetFileName(file.FileName);
        var storedName = $"{Guid.NewGuid():N}{NormalizeExtension(Path.GetExtension(displayName))}";
        var dir = Path.Combine(_env.ContentRootPath, "Resource", "Uploads");
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, storedName);

        try
        {
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件落盘失败: {Path}", fullPath);
            return BadRequest(new { code = 400, message = $"保存失败: {ex.Message}" });
        }

        _logger.LogInformation("文件已落盘: {Stored} 原名={Name} 大小={Size} 类型={ContentType}",
            storedName, displayName, file.Length, file.ContentType);

        return Ok(new
        {
            code = 200,
            message = "上传成功",
            data = new
            {
                name = displayName,
                size = file.Length,
                url = $"Resource/Uploads/{storedName}",
                contentType = file.ContentType
            }
        });
    }

    /// 扩展名白名单化: 只留 ASCII 字母数字, 最长 10 位, 其余(空格/点/斜杠等)一律丢掉
    /// 例子: ".tar.gz" -> ".gz", ".php " -> ".php", "../x" -> ""
    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension)) return string.Empty;

        var cleaned = new string(extension
            .TrimStart('.')
            .Where(c => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9'))
            .ToArray());

        return cleaned.Length is > 0 and <= 10 ? "." + cleaned.ToLowerInvariant() : string.Empty;
    }
}
