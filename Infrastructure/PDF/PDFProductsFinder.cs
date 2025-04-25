using Domain.Models;
using Infrastructure.Settings;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System.Text.RegularExpressions;
using Domain.Interfaces;

namespace Infrastructure.PDF
{
    public class PDFProductsFinder : IPDFProductsFinder<PdfLoadedDocument>
    {
        private readonly ISettings _settings;

        public PDFProductsFinder(ISettings settings)
        {
            _settings = settings;
        }

        private string quantityPattern = @"[0-9]+([.,][0-9]+)? *x";
        private string weightPattern = @"[0-9]+(?: [0-9]+)?(\.[0-9]+)? *g";
        private string quantityUnitPattern = @"x\s*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]+(?:\s*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]+)*";

        public List<ProductCategoryGroup> FindProducts(PdfLoadedDocument loadedDocument)
        {
            Dictionary<string, List<List<TextLine>>> gruppedLinesWithCategory = new Dictionary<string, List<List<TextLine>>>();

            string currentcategory = "";

            var pagesToProcess = GetShoppingListPages(loadedDocument);
            for (int i = 0; i < pagesToProcess.Count; i++)
            {
                pagesToProcess[i].ExtractText(out TextLineCollection textlineCollection);
                List<TextLine> line = new List<TextLine>();
                var textlines = textlineCollection.TextLine.Select(t =>
                {
                    t.Text = t.Text.Trim();
                    return t;
                }).ToList();
                foreach (var textline in textlineCollection.TextLine)
                {
                    if (_settings.IgnoreWords.Any(i => textline.Text.ToLower().Contains(i.ToLower())))
                    {
                        continue;
                    }

                    if (_settings.Categories.Contains(textline.Text))
                    {
                        currentcategory = textline.Text;
                        gruppedLinesWithCategory[currentcategory] = new List<List<TextLine>>();
                        continue;
                    }

                    if (textline.Text.EndsWith("g"))
                    {
                        line.Add(textline);
                        gruppedLinesWithCategory[currentcategory].Add(line);
                        line = new List<TextLine>();
                        continue;
                    }

                    line.Add(textline);
                }
            }

            List<ProductCategoryGroup> productCategoryGroups = new List<ProductCategoryGroup>();
            foreach (var item in gruppedLinesWithCategory)
            {
                ProductCategoryGroup productCategoryGroup = new ProductCategoryGroup()
                {
                    Category = new Category() { Name = item.Key },
                    Products = new List<Product>()
                };

                foreach (var item2 in item.Value)
                {
                    string lineText = item2.Select(x => x.Text).Aggregate((x, y) => x + " " + y);
                    lineText = lineText.Replace(",", "");
                    lineText = Regex.Replace(lineText, @"\u00A0", " ");
                    var name = ExtractProductName(lineText);
                    Match quantityMatch = Regex.Match(lineText, quantityPattern);
                    Match weightMatch = Regex.Match(lineText, weightPattern);
                    Match quantityUnitMatch = Regex.Match(lineText, quantityUnitPattern);

                    try
                    {
                        Product product = new Product()
                        {
                            Name = name,
                            Quantity = quantityMatch.Success ? double.Parse(quantityMatch.Value.TrimEnd('x').Trim().Replace(".", ",")) : null,
                            QuantityUnit = quantityUnitMatch.Success ? quantityUnitMatch.Value.Substring(1).Trim() : null,
                            Weight = weightMatch.Success ? double.Parse(weightMatch.Value.TrimEnd('g').Trim().Replace(".", ",")) : null,
                            WeightUnit = "g"
                        };
                        productCategoryGroup.Products.Add(product);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                productCategoryGroups.Add(productCategoryGroup);
            }

            return productCategoryGroups;
        }

        private static string ExtractProductName(string input)
        {
            var matchX = Regex.Match(input, @"\d+\s*x\s*[A-Za-z]*", RegexOptions.IgnoreCase);
            var matchG = Regex.Match(input, @"\d+\s*g\b", RegexOptions.IgnoreCase);

            if (matchX.Success)
            {
                return Regex.Replace(input.Substring(0, matchX.Index).Trim(), @"(?<=[a-zA-Z])\d+|\d+(?=[a-zA-Z])", "");
            }
            else if (matchG.Success)
            {
                return Regex.Replace(input.Substring(0, matchG.Index).Trim(), @"(?<=[a-zA-Z])\d+|\d+(?=[a-zA-Z])", "");
            }
            else
            {
                return input;
            }
        }

        private List<PdfPageBase> GetShoppingListPages(PdfLoadedDocument pdfLoadedDocument)
        {
            var pagesToProcess = new List<PdfPageBase>();
            bool addPages = false;

            for (int i = 0; i < pdfLoadedDocument.Pages.Count; i++)
            {
                pdfLoadedDocument.Pages[i].ExtractText(out TextLineCollection textlineCollection);
                var text = string.Join(string.Empty, textlineCollection.TextLine.Select(l => l.Text)).Trim().Trim().ToLower();

                if (text.Contains(_settings.ShoppingListFooter.Trim().ToLower()))
                {
                    return pagesToProcess;
                }

                if (text.Contains(_settings.ShoppingListHeader.ToLower()))
                {
                    addPages = true;
                    pagesToProcess.Add(pdfLoadedDocument.Pages[i]);
                }
                else if (addPages)
                {
                    pagesToProcess.Add(pdfLoadedDocument.Pages[i]);
                }
            }
            return pagesToProcess;
        }
    }
}