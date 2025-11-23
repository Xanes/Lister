using Domain.Interfaces;
using Domain.Models;
using Infrastructure.OpenAI.Interfaces;
using Infrastructure.OpenAI.Models;
using Infrastructure.Settings;

using Syncfusion.Pdf.Parsing;

using System.Text;


namespace Infrastructure.PDF
{
    public class PDFDietParserAsync(ISettings settings, IOpenAIRepository repository) : IDietParser<PdfLoadedDocument>
    {
        public async Task<List<Diet>> ParseDietAsync(PdfLoadedDocument[] pdfLoadedDocuments)
        {
            List<Task<Diet?>> tasks = new List<Task<Diet?>>();
            foreach (var document in pdfLoadedDocuments)
            {
                for (int i = 0; i < document.Pages.Count; i++)
                {
                    var text = GetText(document);

                    if (text.ToLower().Contains(settings.SchedulerHeader.ToLower()))
                    {
                        Prompt prompt = new Prompt("pmpt_689137f695508196b7836ce37b9f324001a7aa9136001120", new Dictionary<string, object>(), "16");
                        RequestPayload payload = new RequestPayload(prompt, $"Return as json: {text}", 15000);
                        tasks.Add(repository.GetDataAsync<Diet>(payload));
                        break;
                    }
                }
            }
            List<Diet> mealSchedules = new List<Diet>();
            foreach(var task in tasks)
            {
                var result = await task;
                if (result != null)
                {
                    mealSchedules.Add(result);
                }
            }

            return mealSchedules;
        }

        private string GetText(PdfLoadedDocument document)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < document.Pages.Count; i++)
            {
                
                var text = document.Pages[i].ExtractText();
                sb.Append(text);
                
            }
            return sb.ToString();
        }
    }
}
