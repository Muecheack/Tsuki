namespace cs.Core;

public static class JoinFileStruct
{
    public static string JoinStruct(string fileContent, string fileName) =>
        $"[file name]: {fileName} [file content begin] {fileContent} [file content end]";
}