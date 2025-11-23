using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Models
{
    public record OpenAiMessage(
        string Id,
        string Type,
        string Status,
        List<OpenAiContent> Content,
        string Role
    );
}