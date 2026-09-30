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
            Console.WriteLine($"URL=[{url}] len={url?.Length}");
            Console.WriteLine($"IsAbsolute={Uri.IsWellFormedUriString(url, UriKind.Absolute)}");
            if (!string.IsNullOrEmpty(url))
            {
                foreach (var c in url)
                    Console.Write($"{(int)c} ");
                Console.WriteLine();
            }
            var reply = await _httpClient.PostAsJsonAsync(url, payload);
            // TODO: 未来会返回列表, 到时候把AIChatDto改一下
            var replyContent = await reply.Content.ReadFromJsonAsync<AIChatDto>();
            return replyContent;
        }
    }
