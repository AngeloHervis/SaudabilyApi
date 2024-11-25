using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace SaudabilyApi;

[Route("api/whatsapp")]
[ApiController]
public class ApiController : TwilioController
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ApiController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [HttpPost("message")]
    public async Task<IActionResult> ReceiveMessage([FromBody] ChatRequest request)
    {
        var response = new MessagingResponse();
    
        var chatGptResponse = await GetChatGptResponse(request.Body);
    
        chatGptResponse += "\n\nDeseja continuar sobre esse assunto ou fazer outra pergunta?";
    
        response.Message(chatGptResponse);
        return TwiML(response);
    }
    
    private async Task<string> GetChatGptResponse(string userMessage)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        const string apiUrl = "https://api.openai.com/v1/chat/completions";
        
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "Você é um assistente que oferece dicas úteis sobre saúde, bem-estar e sustentabilidade." },
                new { role = "user", content = userMessage }
            },
            max_tokens = 150,
            temperature = 0.7
        };
        
        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        
        var response = await _httpClient.PostAsync(apiUrl, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erro ao se comunicar com o ChatGPT: {errorDetails}");
            return "Houve um erro ao se comunicar com o ChatGPT.";
        }
        
        var responseBody = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseBody);
        var chatGptResponse = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return chatGptResponse?.Trim() ?? "Desculpe, não consegui gerar uma resposta no momento.";

    }
}
