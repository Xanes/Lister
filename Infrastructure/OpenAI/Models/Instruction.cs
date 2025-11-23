using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Models
{
    public record Instruction(
       string Type,
       List<MessageContent> Content,
       string Role
   );
}
