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

        private string[] ignoredWords = { "LISTA ZAKUPÓW",
                                          "Wartość odżywcza produktów pochodzi ze źródła: Baza danych „Tabele wartości odżywczej produktów spożywczych i potraw – Instytut Żywności i Żywienia, Warszawa 2017, Wydanie IV\r\nrozszerzone i uaktualnione (poprzednik prawny Narodowego Instytutu Zdrowia Publicznego PZH – Państwowego Instytutu Badawczego)” i zostały wykorzystane za zgodą Narodowego\r\nInstytutu Zdrowia Publicznego PZH – Państwowego Instytutu Badawczego w Warszawie. "
                                        };

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
