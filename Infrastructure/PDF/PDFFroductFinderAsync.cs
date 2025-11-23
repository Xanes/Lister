using Domain.Interfaces;
using Domain.Models;
using Infrastructure.OpenAI.Interfaces;
using Infrastructure.OpenAI.Models;
using Infrastructure.Settings;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.PDF
{
    public class PDFFroductFinderAsync(IOpenAIRepository openAIRepository, ISettings settings) : IPDFProductFinderAsync<PdfLoadedDocument>
    {

        public async Task<List<List<ProductCategoryGroup>>> FindProductsAsync(PdfLoadedDocument[] loadedDocuments)
        {
            List<Task<List<ProductCategoryGroup>?>> tasks = new List<Task<List<ProductCategoryGroup>?>>();
            foreach (var document in loadedDocuments) {
                var prompt = new Prompt("pmpt_6887d4845f1081908dfb34291df98ef900a1a027b93b9a53", new Dictionary<string, object>(), "9");
                var payload = new RequestPayload(prompt, GetShoppingListPages(document),15000);
                tasks.Add(openAIRepository.GetDataAsync<List<ProductCategoryGroup>>(payload));
            }

            await Task.WhenAll(tasks);
            List<List<ProductCategoryGroup>> toReturn = new List<List<ProductCategoryGroup>>();
            foreach (var task in tasks)
            {
                var result = await task;
                if (result != null)
                {
                    toReturn.Add(result);
                }
            }
            return toReturn;
        }

        private string GetShoppingListPages(PdfLoadedDocument pdfLoadedDocument)
        {
            var pagesToProcess = new List<PdfPageBase>();
            bool addPages = false;
            var builder = new StringBuilder();

            for (int i = 0; i < pdfLoadedDocument.Pages.Count; i++)
            {
                pdfLoadedDocument.Pages[i].ExtractText(out TextLineCollection textlineCollection);
                var text = string.Join(string.Empty, textlineCollection.TextLine.Select(l => l.Text)).Trim().Trim().ToLower();

                if (text.Contains(settings.ShoppingListFooter.Trim().ToLower()))
                {
                    return builder.ToString();
                }

                if (text.Contains(settings.ShoppingListHeader.ToLower()))
                {
                    addPages = true;
                    builder.Append(pdfLoadedDocument.Pages[i].ExtractText());
                }
                else if (addPages)
                {
                    builder.Append(pdfLoadedDocument.Pages[i].ExtractText());
                }
            }
            return builder.ToString();
        }
    }
}
