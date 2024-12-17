using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Classes
{
    public class Question
    {
        public Word Word { get; set; }
        public List<string> Options { get; set; }
        public string CorrectOption { get; set; }
    }

}
