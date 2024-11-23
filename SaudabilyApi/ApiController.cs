using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace SaudabilyApi;

[Route("/api/whatsapp")]
[ApiController]
public class ApiController(IConfiguration configuration) : TwilioController
{
    
    private readonly string accountSid = configuration["Twilio:AccountSid"];
    private readonly string authToken = configuration["Twilio:AuthToken"];
    private readonly string fromNumber = configuration["Twilio:FromNumber"];

    [HttpPost]
    public IActionResult ReceiveMessage([FromForm] string body, [FromForm] string from)
    {
        var response = new MessagingResponse();
        
        if (body.Contains("saúde", StringComparison.CurrentCultureIgnoreCase))
        {
            response.Message("Dica de Saúde: Beba água regularmente e mantenha uma alimentação equilibrada!");
        }
        else if (body.Contains("sustentabilidade", StringComparison.CurrentCultureIgnoreCase))
        {
            response.Message("Dica de Sustentabilidade: Separe seu lixo reciclável e economize água ao lavar louças.");
        }
        else
        {
            response.Message("Olá! Sou seu assistente sobre ODS. Envie 'saúde' para dicas de bem-estar ou 'sustentabilidade' para práticas sustentáveis.");
        }

        return TwiML(response);
    }
}