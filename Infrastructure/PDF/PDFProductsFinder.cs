using Domain.Models;
using Infrastructure.Settings;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System.Text.RegularExpressions;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Globalization;

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

        public List<List<ProductCategoryGroup>> FindProducts(PdfLoadedDocument[] loadedDocuments)
        {
            List<List<ProductCategoryGroup>> listToReturn = new List<List<ProductCategoryGroup>>(); 
            foreach (var loadedDocument in loadedDocuments)
            {
                Dictionary<string, List<List<string>>> gruppedLinesWithCategory = new Dictionary<string, List<List<string>>>();

                string currentcategory = "";

                var pagesToProcess = GetShoppingListPages(loadedDocument);
                for (int i = 0; i < pagesToProcess.Count; i++)
                {
                    string extractedText = pagesToProcess[i].ExtractText(true);
                    var textLinesArray = extractedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    
                    List<string> line = new List<string>();
                    var textlines = textLinesArray.Select(t => t.Trim()).ToList();
                    
                    foreach (var textline in textlines)
                    {
                        if (_settings.IgnoreWords.Any(i => textline.ToLower().Contains(i.ToLower())))
                        {
                            continue;
                        }

                        if (_settings.Categories.Contains(textline))
                        {
                            currentcategory = textline;
                            gruppedLinesWithCategory[currentcategory] = new List<List<string>>();
                            continue;
                        }

                        if (textline.EndsWith("g"))
                        {
                            line.Add(textline);
                            gruppedLinesWithCategory[currentcategory].Add(line);
                            line = new List<string>();
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
                        string lineText = item2.Aggregate((x, y) => x + " " + y);
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
                                Quantity = quantityMatch.Success ? double.Parse(quantityMatch.Value.TrimEnd('x').Trim().Replace(",", ".").Replace(" ", ""), CultureInfo.InvariantCulture) : null,
                                QuantityUnit = quantityUnitMatch.Success ? quantityUnitMatch.Value.Substring(1).Trim() : null,
                                Weight = weightMatch.Success ? double.Parse(weightMatch.Value.TrimEnd('g').Trim().Replace(",", ".").Replace(" ", ""), CultureInfo.InvariantCulture) : null,
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

                 listToReturn.Add(productCategoryGroups);
            }
            return listToReturn;
        }

        private static string ExtractProductName(string input)
        {
            var matchX = Regex.Match(input, @"\d+\s*x\s*[A-Za-z]*", RegexOptions.IgnoreCase);
            var matchG = Regex.Match(input, @"\d+\s*g\b", RegexOptions.IgnoreCase);

            if (matchX.Success)
            {
                return Regex.Replace(input.Substring(0, matchX.Index).Trim(), @"(?<=[a-zA-Z])\d+|\d+(?=[a-zA-Z])|\s+\d+$", "");
            }
            else if (matchG.Success)
            {
                return Regex.Replace(input.Substring(0, matchG.Index).Trim(), @"(?<=[a-zA-Z])\d+|\d+(?=[a-zA-Z])|\s+\d+$", "");
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
                string extractedText = pdfLoadedDocument.Pages[i].ExtractText(true);
                // Join with empty string to match original behavior: string.Join(string.Empty, textlineCollection.TextLine.Select(l => l.Text))
                // But wait, original code joined with empty string? 
                // "var text = string.Join(string.Empty, textlineCollection.TextLine.Select(l => l.Text)).Trim().Trim().ToLower();"
                // Yes.
                var textLinesArray = extractedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var text = string.Join(string.Empty, textLinesArray).Trim().Trim().ToLower();

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