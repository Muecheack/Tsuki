using Microsoft.AspNetCore.Mvc;

namespace cs.Controllers;

[ApiController]
[Route("api/[controller]")] // api/File
public class FileController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ReceivedFile(IFormFile file)
    {
        // 获取文件 -> 提取文件内容(文本) -> 存文本进数据库
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                code = 400,
                message = "根本没有这种文件"
            });
        }
    }
}