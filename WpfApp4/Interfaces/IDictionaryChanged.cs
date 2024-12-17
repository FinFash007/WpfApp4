using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Interfaces
{
    public interface IDictionaryChanged
    {
        event EventHandler DictionaryUpdated;
    }
}
