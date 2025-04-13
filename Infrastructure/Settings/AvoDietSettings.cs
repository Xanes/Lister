using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Settings
{
    public class AvoDietSettings : ISettings
    {
        public string[] IgnoreWords => ignoredWords;

        public string[] Categories => categories;

        public string ShoppingListHeader => ignoredWords[0];

        public string ShoppingListFooter => "ROZPISKA DNI";

        public string SchedulerHeader => "PODSUMOWANIE JADŁOSPISU";

        private string[] ignoredWords = { "LISTA ZAKUPÓW",
                                          "wartość odżywcza produktów pochodzi ze źródła: baza danych „tabele wartości odżywczej produktów spożywczych i potraw – instytut żywności i żywienia,warszawa 2017" };

        private string[] categories = { "Pieczywo",
                                        "Zbożowe",
                                        "Nabiał",
                                        "Napoje",
                                        "Tłuszcze",
                                        "Zioła i przyprawy",
                                        "Mięso",
                                        "Pozostałe",
                                        "Owoce i warzywa",
                                        "Orzechy i nasiona",
                                        "Słodycze",
                                        "Sumplementy"
                                        };
    }
}

