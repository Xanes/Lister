using DietMergerLib.Models;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System.Text.RegularExpressions;

namespace DietMergerLib
{
    public class PDFProductsFinder
    {
        private string[] ignoredWords = { "Lista zakupów",
                                          "PRODUKTILOŚĆWAGAPRODUKTILOŚĆWAGA"
                                        };

        private string[] categories = { "Mięso i produkty mięsne",
                                        "Mrożonki",
                                        "Nabiał i jaja",
                                        "Orzechy i nasiona",
                                        "Owoce",
                                        "Przetwory",
                                        "Przyprawy i zioła",
                                        "Słodycze i przekąski",
                                        "Warzywa",
                                        "Zbożowe",
                                        "Inne",
                                        "Tłuszcze"};

        private string productNamePattern = @"[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]+[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]*[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]*[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]*[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]*[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]*[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]*";
        private string quantityPattern = @"[0-9]+(\.[0-9]+)? *x";
        private string weightPattern = @"[0-9]+(\.[0-9]+)? *g";
        private string quantityUnitPattern = @"x [a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]+[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]*[\s]*[a-zA-ZąĄćĆęĘłŁńŃóÓśŚźŹżŻ]";

        public List<ProductCategoryGroup> FindProducts(Stream fsSource)
        {

            Dictionary<string, List<List<TextLine>>> gruppedLinesWithCategory = new Dictionary<string, List<List<TextLine>>>();

            string currentcategory = "";

            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(fsSource))
            {
                for (int i = 0; i < loadedDocument.Pages.Count; i++)
                {
                    loadedDocument.Pages[i].ExtractText(out TextLineCollection textlineCollection);
                    List<TextLine> line = new List<TextLine>();
                    foreach (var textline in textlineCollection.TextLine)
                    {
                        if (ignoredWords.Contains(textline.Text))
                        {
                            continue;
                        }

                        if (categories.Contains(textline.Text))
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
            }

            List<ProductCategoryGroup> productCategoryGroups = new List<ProductCategoryGroup>();
            foreach (var item in gruppedLinesWithCategory)
            {
                ProductCategoryGroup productCategoryGroup = new ProductCategoryGroup(item.Key, new List<Product>());
                foreach (var item2 in item.Value)
                {
                    string lineText = item2.Select(x => x.Text).Aggregate((x, y) => x + y);
                    Match productNameMatch = Regex.Match(lineText, productNamePattern);
                    Match quantityMatch = Regex.Match(lineText, quantityPattern);
                    Match weightMatch = Regex.Match(lineText, weightPattern);
                    Match quantityUnitMatch = Regex.Match(lineText, quantityUnitPattern);
                    try
                    {
                        Product product = new Product()
                        {
                            Name = productNameMatch.Success ? productNameMatch.Value : null,
                            Quantity = quantityMatch.Success ? double.Parse(quantityMatch.Value.TrimEnd('x').Trim().Replace(".", ",")) : null,
                            QuantityUnit = quantityUnitMatch.Success ? quantityUnitMatch.Value.Substring(2) : null,
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
    }
}