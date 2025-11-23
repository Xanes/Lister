using Domain.Interfaces;
using Domain.Models;
using Infrastructure.OpenAI.Interfaces;
using Infrastructure.OpenAI.Models;
using Infrastructure.Settings;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System.Text;

namespace Infrastructure.PDF
{
    public class RecipeFinderAsync(ISettings settings, IOpenAIRepository repository) : IRecipeFinderAsync<PdfLoadedDocument>
    {
        public async Task<List<List<Recipe>>> FindRecipes(PdfLoadedDocument[] loadedDocuments)
        {
            List<Task<List<Recipe>?>> tasks = new List<Task<List<Recipe>?>>();
            foreach (var document in loadedDocuments) 
            {
                var text = GetReceipes(document);
                Prompt prompt = new Prompt("pmpt_688fe67b3db48195b750d72e54b81e850887b9e16f7a35a3", new Dictionary<string, object>(), "3");
                RequestPayload payload = new RequestPayload(prompt, text, 1500);
                tasks.Add(repository.GetDataAsync<List<Recipe>>(payload));
            }

            List<List<Recipe>> recipes = new List<List<Recipe>>();
            foreach(var task in tasks)
            {
                var result = await task;
                if (result != null)
                {
                    recipes.Add(result);
                }
                
            }

            return recipes;
        }

        private string GetReceipes(PdfLoadedDocument document)
        {
            bool addPages = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < document.Pages.Count; i++)
            {
                document.Pages[i].ExtractText(out TextLineCollection textlineCollection);
                var text = string.Join(" | ", textlineCollection.TextLine.Select(l => l.Text));

                if (text.ToLower().Contains(settings.ShoppingListFooter.ToLower()))
                {
                    addPages = true;
                }

                if (addPages)
                {
                    sb.Append(text);
                }
            }
            return sb.ToString();
        }
    }
}
