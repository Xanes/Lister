using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Models
{
    public record Prompt(
        string Id,
        Dictionary<string, object> Variables,
        string Version
    );
}
