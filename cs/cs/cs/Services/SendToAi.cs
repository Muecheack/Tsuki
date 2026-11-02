namespace cs.Services;

    public class SendToAi
    {
        private readonly HttpClient _httpClient;

        public SendToAi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<AIChatDto> SendAiAsync(string url, AiPayload payload)
        {
            var reply = await _httpClient.PostAsJsonAsync(url, payload);
            // TODO: 未来会返回列表, 到时候把AIChatDto改一下
            var replyContent = await reply.Content.ReadFromJsonAsync<AIChatDto>();
            return replyContent;
        }
    }
