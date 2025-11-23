using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Models
{
    public record RequestPayload(
     Prompt prompt,
     string input,
     int max_output_tokens,
     ReasoningPayload? reasoning = null,
     TextPayload? text = null
 );
}