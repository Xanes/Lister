using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Settings
{
    public class OpenAiSettings
    {
        public string Endpoint { get; set; } = default!;
        public string Auth {  get; set; } = default!;
    }
}
