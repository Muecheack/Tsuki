using cs.Shell;
using Microsoft.AspNetCore.Mvc;

namespace cs.Endpoints;

public sealed class FileEndpoints
{
    // 表单必须用 [FromForm]，[FromBody] 接不了 multipart。
    // 字段都可空，下面自己拦错，保证返回带 code。
    public static async Task<IResult> ReceivedFile(
        [FromForm] IFormFile? file,
        [FromForm] string? aiName,
        [FromForm] string? userName,
        [FromForm] int? sessionId,
        DbIO dbIo)
    {
        // 获取文件 -> 提取文件内容(文本) -> 存文本进数据库
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new
            {
                code = 400,
                message = "根本没有这种文件"
            });
        }

        // 缺哪个就说哪个
        if (string.IsNullOrWhiteSpace(aiName))
        {
            return Results.BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 缺少 aiName（会话对象为空）"
            });
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            return Results.BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 缺少 userName（本地没读到当前用户昵称）"
            });
        }

        // sessionId 是必填列，先拦掉，别等 SaveChanges 报 500。
        if (sessionId is not int sid || sid <= 0)
        {
            return Results.BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 会话 id 无效"
            });
        }

        // 拿文件名 + 文件文本
        var (fileName, fileContent) = await dbIo.GetFileString(file);

        // 拼接
        var fileStruct = Core.JoinFileStruct.JoinStruct(fileContent, fileName);

        // 全部落库
        var (state, userId) = await dbIo.GetUserIdAsync(userName);
        if (state == IOState.NotFound)
        {
            return Results.BadRequest(new
            {
                code = 400,
                message = "文件上传失败, 用户不存在"
            });
        }

        if (userId is { } uid)
        {
            await dbIo.SaveChatInMessageAsync(sid, uid, fileStruct);
        }

        return Results.Ok(new
        {
            code = 200,
            message = "文件上传成功"
        });
    }
}
