using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Models
{
    public record TokenUsage(
     int InputTokens,
     TokenDetails InputTokensDetails,
     int OutputTokens,
     TokenDetails OutputTokensDetails,
     int TotalTokens
 );
}
