using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Settings
{
    public interface ISettings
    {
        string[] IgnoreWords { get; }
        string[] Categories { get; }

        string ShoppingListHeader { get; }
        string ShoppingListFooter { get; }
    }
}
