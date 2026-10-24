using cs.Shell;
using Microsoft.AspNetCore.Mvc;

namespace cs.Controllers;

[ApiController]
[Route("api/[controller]")] // api/File
public class FileController : ControllerBase
{
    private DbIO _dbIo;

    public FileController(DbIO dbIo)
    {
        _dbIo = dbIo;
    }

    // [FromForm] 不能写成 [FromBody]：multipart/form-data 没有对应的 body formatter，
    // 用 [FromBody] 会直接回 415 Unsupported Media Type，IFormFile 永远绑不上（文件永远传不进来）
    // 注：Dto 字段都是可空类型，所有失败分支都在下面手动拦，保证响应一定带 code（前端好报错）
    [HttpPost("file1")] // api/File/file1
    public async Task<IActionResult> ReceivedFile([FromForm] FileDto fileDto)
    {
        // 获取文件 -> 提取文件内容(文本) -> 存文本进数据库
        var file = fileDto.file;
        var aiName = fileDto.aiName;
        var userName = fileDto.userName;
        var sessionId = fileDto.sessionId;

        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                code = 400,
                message = "根本没有这种文件"
            });
        }

        // aiName/userName 为空多半是前端本地缓存没读到（没登录 / 旧版缓存没有 nickname 字段），
        // 说明白是哪个少了，别只回一句「参数不合法」
        if (string.IsNullOrWhiteSpace(aiName))
        {
            return BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 缺少 aiName（会话对象为空）"
            });
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            return BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 缺少 userName（本地没读到当前用户昵称）"
            });
        }

        // 会话 id 在 Messages 表是非空列：缺了/非法就早点拒，别等 SaveChanges 抛外键异常变成 500
        if (sessionId is not int sid || sid <= 0)
        {
            return BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 会话 id 无效"
            });
        }

        // 拿文件名 + 文件文本
        var (fileName, fileContent) = await _dbIo.GetFileString(file);

        // 拼接
        var fileStruct = Core.JoinFileStruct.JoinStruct(fileContent, fileName);

        // 全部落库
        var (state, userId) = await _dbIo.GetUserIdAsync(userName);
        if (state == IOState.NotFound)
        {
            return BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 用户不存在"
            });
        }

        if (userId is {} uid)
        {
            await _dbIo.SaveChatInMessageAsync(sid, uid, fileStruct);
        }

        return Ok(new
        {
            code = 200,
            message = "文件上传成功"
        });
    }
}