using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Settings;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Infrastructure.PDF
{
    public class ReceipeFinder : IReceipeFinder<PdfLoadedDocument>
    {
        private readonly ISettings _settings;
        private readonly string[] _dayNames = { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota", "Niedziela" };

        public ReceipeFinder(ISettings settings)
        {
            _settings = settings;
        }

        public List<Recipe> GetReceipes(PdfLoadedDocument pdfLoadedDocument)
        {
            StringBuilder sb = new StringBuilder();
            bool addPages = false;
            for (int i = 0; i < pdfLoadedDocument.Pages.Count; i++)
            {
                string extractedText = pdfLoadedDocument.Pages[i].ExtractText(true);
                var textLines = extractedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var text = string.Join(" | ", textLines);

                if (text.ToLower().Contains(_settings.ShoppingListFooter.ToLower()))
                {
                    addPages = true;
                }

                if (addPages)
                {
                    sb.Append(text);
                }
            }

            string receipesText = sb.Replace("ok.", "około").Replace("min.", "min").Replace("np.", "na przykład").ToString();
            return ParseReceipes(receipesText);
        }

        private List<Recipe> ParseReceipes(string receipesText)
        {
            var recipes = new List<Recipe>();

            // Remove "ROZPISKA DNI" parts
            receipesText = receipesText.Replace("ROZPISKA DNI", "");

            // Remove standalone dates (not part of meal headers)
            receipesText = Regex.Replace(receipesText, @"\|\s*\d{2}\.\d{2}\.\d{4}\s*\|", "|");

            // Remove day names
            foreach (var day in _dayNames)
            {
                receipesText = receipesText.Replace($" | {day} ", " | ");
            }

            // Split the text by the delimiter
            var parts = receipesText.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries)
                                  .Where(p => !string.IsNullOrWhiteSpace(p))
                                  .Select(p => p.Trim())
                                  .ToArray();

            // Process each part
            for (int i = 0; i < parts.Length; i++)
            {
                // Check if the current part is a meal header
                var mealHeaderMatch = Regex.Match(parts[i], @"^(?:(.+?)(?:\s+\d{2}:\d{2})?\s*(?:E:|\s*:)\s*(\d+)kcal|E:\s*(\d+)kcal)(?:\s*B:\s*([\d,]+)g)?(?:\s*T:\s*([\d,]+)g)?(?:\s*W:\s*([\d,]+)g)?(?:\s*F:\s*([\d,]+)g)?$");
                var simpleMealHeaderMatch = Regex.Match(parts[i], @"^(ŚNIADANIE|DRUGIE ŚNIADANIE|PRZEKĄSKA|OBIAD|KOLACJA)(?:\s+\d{2}:\d{2})?$");

                if (mealHeaderMatch.Success || simpleMealHeaderMatch.Success)
                {
                    string mealType;
                    double calories = 0, protein = 0, fat = 0, carbs = 0, fiber = 0;

                    if (mealHeaderMatch.Success)
                    {
                        mealType = mealHeaderMatch.Groups[1].Value.Trim();
                        // If first format didn't match (groups 2), use second format (group 3)
                        string caloriesStr = mealHeaderMatch.Groups[2].Success ? mealHeaderMatch.Groups[2].Value : mealHeaderMatch.Groups[3].Value;
                        calories = double.Parse(caloriesStr.Replace(" ", ""), CultureInfo.InvariantCulture);

                        if (mealHeaderMatch.Groups[4].Success)
                            double.TryParse(mealHeaderMatch.Groups[4].Value.Replace(",", ".").Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out protein);
                        if (mealHeaderMatch.Groups[5].Success)
                            double.TryParse(mealHeaderMatch.Groups[5].Value.Replace(",", ".").Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out fat);
                        if (mealHeaderMatch.Groups[6].Success)
                            double.TryParse(mealHeaderMatch.Groups[6].Value.Replace(",", ".").Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out carbs);
                        if (mealHeaderMatch.Groups[7].Success)
                            double.TryParse(mealHeaderMatch.Groups[7].Value.Replace(",", ".").Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out fiber);
                    }
                    else
                    {
                        mealType = simpleMealHeaderMatch.Groups[1].Value.Trim();
                    }

                    // Check if we have enough parts for at least a name and some instructions
                    if (i + 2 >= parts.Length) continue;

                    // Next part is the recipe name
                    string recipeName = parts[i + 1].Trim();

                    // Check if there are instructions (looking for sentences with periods)
                    bool hasInstructions = false;
                    for (int checkIdx = i + 2; checkIdx < parts.Length; checkIdx++)
                    {
                        if (parts[checkIdx].Contains(".") && !IsJustNumberWithDot(parts[checkIdx]))
                        {
                            hasInstructions = true;
                            break;
                        }
                    }

                    // Skip recipes without instructions
                    if (!hasInstructions) continue;

                    // Create recipe
                    var recipe = new Recipe
                    {
                        Name = recipeName,
                        Calories = calories,
                        Protein = protein,
                        Fat = fat,
                        Carbohydrates = carbs,
                        Fiber = fiber
                    };

                    // Extract instructions and ingredients
                    List<string> instructions = new List<string>();
                    List<RecipeIngredient> ingredients = new List<RecipeIngredient>();

                    // Current position in the parts array
                    int currentIdx = i + 2;
                    StringBuilder instructionBuffer = new StringBuilder();

                    // Process parts until we hit a new meal header
                    while (currentIdx < parts.Length && !IsNewMealHeader(parts[currentIdx]))
                    {
                        string currentPart = parts[currentIdx].Trim();
                        currentIdx++;

                        // Skip parts that are just numbers with dots (like "1.")
                        if (IsJustNumberWithDot(currentPart))
                        {
                            continue;
                        }

                        // Check if it's a potential ingredient
                        if (!currentPart.Contains(".") &&
                            (currentPart.Contains("x") ||
                             (currentIdx < parts.Length &&
                              !IsNewMealHeader(parts[currentIdx]) &&
                              parts[currentIdx].Contains("x"))))
                        {
                            // Buffer to collect ingredient parts
                            List<string> ingredientBuffer = new List<string>
                            {
                                currentPart
                            };

                            // Collect parts until we have a complete ingredient (ends with weight unit)
                            while (currentIdx < parts.Length &&
                                  !IsNewMealHeader(parts[currentIdx]) &&
                                  !ingredientBuffer.Last().EndsWith("g") &&
                                  !parts[currentIdx].Contains("."))
                            {
                                ingredientBuffer.Add(parts[currentIdx].Trim());
                                currentIdx++;
                            }

                            // Join the buffer parts
                            string combinedIngredient = string.Join(" ", ingredientBuffer);

                            // Process the combined ingredient text
                            if (IsIngredientText(combinedIngredient))
                            {
                                ProcessIngredient(combinedIngredient, recipe, ingredients);
                            }
                            else if (!string.IsNullOrWhiteSpace(instructionBuffer.ToString()))
                            {
                                // If we have pending instruction text and this isn't an ingredient,
                                // add it to the instruction buffer
                                instructionBuffer.Append(" ").Append(combinedIngredient);
                            }
                        }
                        else
                        {
                            // It's potentially an instruction part
                            if (currentPart.Contains("."))
                            {
                                // This part contains complete sentences

                                // If we have pending instruction text, combine it with current part
                                if (instructionBuffer.Length > 0)
                                {
                                    instructionBuffer.Append(" ").Append(currentPart);
                                    string fullInstructionText = instructionBuffer.ToString();
                                    ProcessInstructions(fullInstructionText, instructions);
                                    instructionBuffer.Clear();
                                }
                                else
                                {
                                    // Process this part directly
                                    ProcessInstructions(currentPart, instructions);
                                }

                                // Check if the part ends with an incomplete sentence
                                int lastDotIndex = currentPart.LastIndexOf('.');
                                if (lastDotIndex < currentPart.Length - 1)
                                {
                                    // There's text after the last period - it's an incomplete sentence
                                    string remainingText = currentPart.Substring(lastDotIndex + 1).Trim();
                                    if (!string.IsNullOrWhiteSpace(remainingText))
                                    {
                                        instructionBuffer.Append(remainingText);
                                    }
                                }
                            }
                            else
                            {
                                // This part doesn't contain a period, add it to the buffer
                                if (instructionBuffer.Length > 0)
                                {
                                    instructionBuffer.Append(" ");
                                }
                                instructionBuffer.Append(currentPart);
                            }
                        }
                    }

                    // Process any remaining instruction text
                    if (instructionBuffer.Length > 0)
                    {
                        string remainingInstruction = instructionBuffer.ToString().Trim();
                        if (!string.IsNullOrEmpty(remainingInstruction))
                        {
                            instructions.Add(remainingInstruction + ".");
                        }
                    }

                    // Add instructions to recipe
                    for (int instrIdx = 0; instrIdx < instructions.Count; instrIdx++)
                    {
                        recipe.Instructions.Add(new RecipeInstruction
                        {
                            RecipeId = recipe.Id,
                            StepNumber = instrIdx + 1,
                            Instruction = instructions[instrIdx].Trim()
                        });
                    }

                    // Add ingredients to recipe
                    foreach (var ingredient in ingredients)
                    {
                        recipe.Ingredients.Add(ingredient);
                    }

                    recipes.Add(recipe);

                    // Update i to continue from where we left off
                    i = currentIdx - 1;
                }
            }

            return recipes.DistinctBy(r => r.Name).ToList();
        }

        private void ProcessIngredient(string text, Recipe recipe, List<RecipeIngredient> ingredients)
        {
            // Parse ingredient using detailed pattern
            // Format example: "Płatki owsiane 4 x Łyżka 40g"
            var ingredientMatch = Regex.Match(text, @"^(.+?)\s+(\d+(?:,\d+)?)\s+x\s+(.+?)\s+(\d+)(\w+)$");
            if (ingredientMatch.Success)
            {
                string ingredientName = ingredientMatch.Groups[1].Value.Trim();
                // Parse quantity directly
                double quantity = double.Parse(ingredientMatch.Groups[2].Value.Replace(",", ".").Replace(" ", ""), CultureInfo.InvariantCulture);
                string quantityUnit = ingredientMatch.Groups[3].Value.Trim();
                double weight = double.Parse(ingredientMatch.Groups[4].Value.Replace(" ", ""), CultureInfo.InvariantCulture);
                string weightUnit = ingredientMatch.Groups[5].Value.Trim();

                ingredients.Add(new RecipeIngredient
                {
                    RecipeId = recipe.Id,
                    Name = ingredientName,
                    Quantity = quantity,
                    QuantityUnit = quantityUnit,
                    Weight = weight,
                    WeightUnit = weightUnit
                });
            }
        }

        private void ProcessInstructions(string text, List<string> instructions)
        {
            // It's an instruction - split by periods
            var sentences = text.Split('.');
            foreach (var sentenceRaw in sentences)
            {
                string sentence = sentenceRaw.Trim();
                if (string.IsNullOrEmpty(sentence)) continue;

                instructions.Add(sentence + ".");
                return;
            }
        }

        private bool IsJustNumberWithDot(string text)
        {
            // Check if the text is just a number followed by a dot (e.g., "1." or "2.")
            return Regex.IsMatch(text.Trim(), @"^\d+\.$");
        }

        private bool IsIngredientText(string text)
        {
            // Check if the text matches an ingredient pattern (contains "x" and ends with a weight)
            return Regex.IsMatch(text, @"\d+(?:,\d+)?\s+x\s+.+\d+\w+$");
        }

        private bool IsNewMealHeader(string text)
        {
            // Check if the text is a new meal header - handle both formats
            return Regex.IsMatch(text, @"^(?:.+?(?:\s+\d{2}:\d{2})?\s*(?:E:|\s*:)\s*\d+kcal|E:\s*\d+kcal)") ||
                   Regex.IsMatch(text, @"^(ŚNIADANIE|DRUGIE ŚNIADANIE|PRZEKĄSKA|OBIAD|KOLACJA)(?:\s+\d{2}:\d{2})?$");
        }
    }
}