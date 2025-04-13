using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Settings
{
    public class MakeMeDietSettings : ISettings
    {
        public string[] IgnoreWords => ignoredWords;

        public string[] Categories => categories;

        public string ShoppingListHeader => string.Empty;

        public string ShoppingListFooter => string.Empty;

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

    }
}
