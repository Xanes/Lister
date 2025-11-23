using Infrastructure.OpenAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Interfaces
{
    public interface IOpenAIService
    {
        Task<OpenAiApiResponse?> SendRequestAsync(RequestPayload payload, string endpointUrl, string bearerToken);
    }
}
