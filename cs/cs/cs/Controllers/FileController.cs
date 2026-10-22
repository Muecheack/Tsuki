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

    [HttpPost("file1")] // api/File/file1
    public async Task<IActionResult> ReceivedFile([FromBody] FileDto fileDto)
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
            await _dbIo.SaveChatInMessageAsync(sessionId, uid, fileStruct);
        }

        return Ok(new
        {
            code = 200,
            message = "文件上传成功"
        });
    }
}