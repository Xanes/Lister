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
        public ScheduleFinder(ISettings settings) 
        {
            _settings = settings;
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

        public List<MealSchedule> ParseMealSchedule(string scheduleText, int shoppingListId)
        {
            var result = new List<MealSchedule>();
            
            var parts = scheduleText.Split(" | ", StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 3)
            {
                return result;
            }
            
            var startingIndex = 2;
            var mealTypeCounts = new Dictionary<string, int>();
            
            string currentMealTypeText = null;
            MealTypes currentMealType = MealTypes.Sniadanie;
            TimeSpan? currentMealTime = null;
            string currentMealName = "";
            DaysOfWeek currentDay = DaysOfWeek.Poniedzialek;
            
            for (int i = startingIndex; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                
                if (IsMealTypeWithTime(part))
                {
                    if (!string.IsNullOrEmpty(currentMealName) && currentMealTypeText != null)
                    {
                        string baseMealType = GetBaseMealType(currentMealTypeText);
                        if (!mealTypeCounts.ContainsKey(baseMealType))
                        {
                            mealTypeCounts[baseMealType] = 0;
                        }
                        
                        currentDay = GetDayByIndex(mealTypeCounts[baseMealType]);
                        
                        var mealSchedule = new MealSchedule
                        {
                            ShoppingListId = shoppingListId,
                            DayOfWeek = currentDay,
                            MealType = currentMealType,
                            MealName = currentMealName.Trim(),
                            Time = currentMealTime
                        };
                        
                        result.Add(mealSchedule);
                        mealTypeCounts[baseMealType]++;
                        currentMealName = "";
                    }
                    
                    currentMealTypeText = part;
                    currentMealType = ParseMealType(part);
                    currentMealTime = ExtractTime(part);
                }
                else
                {
                    if (currentMealTypeText != null)
                    {
                        if (!string.IsNullOrEmpty(currentMealName))
                        {
                            currentMealName += " ";
                        }
                        currentMealName += part;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(currentMealName) && currentMealTypeText != null)
            {
                string baseMealType = GetBaseMealType(currentMealTypeText);
                if (!mealTypeCounts.ContainsKey(baseMealType))
                {
                    mealTypeCounts[baseMealType] = 0;
                }
                
                currentDay = GetDayByIndex(mealTypeCounts[baseMealType]);
                
                var mealSchedule = new MealSchedule
                {
                    ShoppingListId = shoppingListId,
                    DayOfWeek = currentDay,
                    MealType = currentMealType,
                    MealName = currentMealName.Trim(),
                    Time = currentMealTime
                };
                
                result.Add(mealSchedule);
            }
            
            return result;
        }
        
        private bool IsMealTypeWithTime(string text)
        {
            // Match meal types with optional time (e.g., "Śniadanie08:00" or just "Śniadanie")
            var regex = new Regex(@"(Śniadanie|Drugie śniadanie|Obiad|Kolacja)(?:\d{2}:\d{2})?");
            return regex.IsMatch(text);
        }
        
        private string GetBaseMealType(string mealTypeWithTime)
        {
            // Extract just the meal type part (e.g., "Śniadanie" from "Śniadanie08:00")
            var regex = new Regex(@"(Śniadanie|Drugie śniadanie|Obiad|Kolacja)");
            var match = regex.Match(mealTypeWithTime);
            return match.Success ? match.Value : string.Empty;
        }
        
        private MealTypes ParseMealType(string text)
        {
            string baseMealType = GetBaseMealType(text);
            
            switch (baseMealType.ToLower())
            {
                case "śniadanie":
                    return MealTypes.Sniadanie;
                case "drugie śniadanie":
                    return MealTypes.DrugieSniadanie;
                case "obiad":
                    return MealTypes.Obiad;
                case "kolacja":
                    return MealTypes.Kolacja;
                default:
                    return MealTypes.Sniadanie; // Default
            }
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
                _ => DaysOfWeek.Poniedzialek // Default to Monday for overflow
            };
        }

        private TimeSpan? ExtractTime(string text)
        {
            var regex = new Regex(@"\d{2}:\d{2}");
            var match = regex.Match(text);
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
