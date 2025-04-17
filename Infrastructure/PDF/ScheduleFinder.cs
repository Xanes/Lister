using Domain.Enums;
using Domain.Models;
using Infrastructure.Settings;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.PDF
{
    public class ScheduleFinder
    {
        private readonly ISettings _settings;
        private readonly Regex _mealTypePattern;
        private readonly Regex _timePattern;
        private readonly Regex _headerPattern;
        private readonly string[] _mealTypes = { "ŚNIADANIE", "DRUGIE ŚNIADANIE", "PRZEKĄSKA", "OBIAD", "KOLACJA" };

        public ScheduleFinder(ISettings settings) 
        {
            _settings = settings;
            _mealTypePattern = new Regex(@"^(ŚNIADANIE|DRUGIE ŚNIADANIE|PRZEKĄSKA|OBIAD|KOLACJA)[E]*(?:\s*\d{2}:\d{2})?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _timePattern = new Regex(@"\d{2}:\d{2}$", RegexOptions.Compiled);
            _headerPattern = new Regex(@"^(Poniedziałek|Wtorek|Środa|Czwartek|Piątek|Sobota|Niedziela|PODSUMOWANIE JADŁOSPISU|ROZPISKA DNI)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public List<MealSchedule> GetMealsSchedule(PdfLoadedDocument pdfLoadedDocument, int shoppingListId)
        {
            for (int i = 0; i < pdfLoadedDocument.Pages.Count; i++)
            {
                pdfLoadedDocument.Pages[i].ExtractText(out TextLineCollection textlineCollection);
                var text = string.Join(" | ", textlineCollection.TextLine.Select(l => l.Text));

                if (text.ToLower().Contains(_settings.SchedulerHeader.ToLower()))
                {
                    return ParseMealSchedule(text, shoppingListId);
                }
            }
            return new List<MealSchedule>();
        }

        private string RemoveDatePatterns(string text)
        {
            var datePattern = @"\d{2}\.\d{2}\.\d{4}";
            return Regex.Replace(text, datePattern, string.Empty).Trim();
        }

        private MealSchedule CreateMealSchedule(
            int shoppingListId,
            string mealTypeText,
            MealTypes mealType,
            TimeSpan? mealTime,
            List<string> mealParts,
            int dayIndex)
        {
            return new MealSchedule
            {
                ShoppingListId = shoppingListId,
                DayOfWeek = GetDayByIndex(dayIndex),
                MealType = mealType,
                MealName = string.Join(" ", mealParts).Trim(),
                Time = mealTime
            };
        }

        public List<MealSchedule> ParseMealSchedule(string scheduleText, int shoppingListId)
        {
            var result = new List<MealSchedule>();
            scheduleText = RemoveDatePatterns(scheduleText);
            
            var parts = scheduleText.Split(" | ", StringSplitOptions.RemoveEmptyEntries)
                                  .Where(p => !string.IsNullOrWhiteSpace(p))
                                  .Select(p => p.Trim())
                                  .ToArray();
            
            if (parts.Length < 3) return result;
            
            var startingIndex = FindStartingIndex(parts);
            var mealTypeCounts = new Dictionary<string, int>();
            
            string currentMealTypeText = null;
            MealTypes currentMealType = MealTypes.Sniadanie;
            TimeSpan? currentMealTime = null;
            List<string> currentMealParts = new List<string>();
            
            for (int i = startingIndex; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                
                if (IsMealTypeWithTime(part))
                {
                    if (currentMealParts.Any() && currentMealTypeText != null)
                    {
                        string baseMealType = GetBaseMealType(currentMealTypeText);
                        if (!mealTypeCounts.ContainsKey(baseMealType))
                        {
                            mealTypeCounts[baseMealType] = 0;
                        }

                        var mealSchedule = CreateMealSchedule(
                            shoppingListId,
                            currentMealTypeText,
                            currentMealType,
                            currentMealTime,
                            currentMealParts,
                            mealTypeCounts[baseMealType]);

                        result.Add(mealSchedule);
                        mealTypeCounts[baseMealType]++;
                    }
                    
                    currentMealTypeText = part;
                    currentMealType = ParseMealType(part);
                    currentMealTime = ExtractTime(part);
                    currentMealParts = new List<string>();
                }
                else if (currentMealTypeText != null && !IsHeaderText(part) && !IsMealTypeWithTime(part))
                {
                    if (!string.IsNullOrWhiteSpace(part) && !part.All(c => char.IsDigit(c) || c == '.'))
                    {
                        if (!_timePattern.IsMatch(part))
                        {
                            currentMealParts.Add(part);
                        }
                    }
                }
            }
            
            if (currentMealParts.Any() && currentMealTypeText != null)
            {
                string baseMealType = GetBaseMealType(currentMealTypeText);
                if (!mealTypeCounts.ContainsKey(baseMealType))
                {
                    mealTypeCounts[baseMealType] = 0;
                }

                var mealSchedule = CreateMealSchedule(
                    shoppingListId,
                    currentMealTypeText,
                    currentMealType,
                    currentMealTime,
                    currentMealParts,
                    mealTypeCounts[baseMealType]);

                result.Add(mealSchedule);
            }
            
            return result;
        }

        private int FindStartingIndex(string[] parts)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsMealTypeWithTime(parts[i]))
                {
                    return i;
                }
            }
            return 2;
        }

        private bool IsHeaderText(string text)
        {
            return _headerPattern.IsMatch(text);
        }
        
        private bool IsMealTypeWithTime(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            string cleanText = text.ToUpper().Trim();
            return _mealTypePattern.IsMatch(cleanText);
        }
        
        private string GetBaseMealType(string mealTypeWithTime)
        {
            string cleanText = Regex.Replace(mealTypeWithTime.ToUpper().Trim(), "E+$", "");
            foreach (var mealType in _mealTypes)
            {
                if (cleanText.StartsWith(mealType, StringComparison.OrdinalIgnoreCase))
                {
                    return mealType;
                }
            }
            return string.Empty;
        }
        
        private MealTypes ParseMealType(string text)
        {
            string cleanText = Regex.Replace(text.ToUpper().Trim(), "E+$", "");
            
            if (cleanText.StartsWith("ŚNIADANIE", StringComparison.OrdinalIgnoreCase))
                return MealTypes.Sniadanie;
            if (cleanText.StartsWith("DRUGIE ŚNIADANIE", StringComparison.OrdinalIgnoreCase) || 
                cleanText.StartsWith("PRZEKĄSKA", StringComparison.OrdinalIgnoreCase))
                return MealTypes.DrugieSniadanie;
            if (cleanText.StartsWith("OBIAD", StringComparison.OrdinalIgnoreCase))
                return MealTypes.Obiad;
            if (cleanText.StartsWith("KOLACJA", StringComparison.OrdinalIgnoreCase))
                return MealTypes.Kolacja;
            
            return MealTypes.Sniadanie; // Default
        }
        
        private DaysOfWeek GetDayByIndex(int index)
        {
            return index switch
            {
                0 => DaysOfWeek.Poniedzialek,
                1 => DaysOfWeek.Wtorek,
                2 => DaysOfWeek.Sroda,
                3 => DaysOfWeek.Czwartek,
                4 => DaysOfWeek.Piatek,
                5 => DaysOfWeek.Sobota,
                6 => DaysOfWeek.Niedziela,
                _ => DaysOfWeek.Poniedzialek
            };
        }

        private TimeSpan? ExtractTime(string text)
        {
            var match = _timePattern.Match(text);
            if (match.Success)
            {
                if (TimeSpan.TryParse(match.Value, out TimeSpan time))
                {
                    return time != TimeSpan.Zero ? time : null;
                }
            }
            return null;
        }
    }
}
