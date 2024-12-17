using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Classes
{
    public class Category
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ObservableCollection<Word> Words { get; set; }

        public Category()
        {
            Words = new ObservableCollection<Word>();
        }
    }
}
