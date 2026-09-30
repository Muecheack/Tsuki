namespace cs.Shell;

public class HttpIO
{
    private readonly HttpClient _httpClient;

    public HttpIO(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 发给AI
    public async Task<PrePersonReplyDto> ToAi(PrePersonPayload payload, string url)
    {
        try
        {
            var reply = await _httpClient.PostAsJsonAsync(url, payload);
            reply.EnsureSuccessStatusCode();
            var replyContent = await reply.Content.ReadFromJsonAsync<PrePersonReplyDto>();
            return replyContent;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}