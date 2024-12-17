using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using WpfApp4.Models;
using WpfApp4.Classes;
using WpfApp4.Interfaces;

namespace WpfApp4.Classes
{
    public class DictionaryModel : IDictionaryChanged
    {
        public ObservableCollection<Category> Categories { get; set; }

        public event EventHandler DictionaryUpdated;

        public DictionaryModel()
        {
            Categories = new ObservableCollection<Category>();
            Categories.CollectionChanged += (s, e) => OnDictionaryUpdated();
        }

        public void AddCategory(string categoryName)
        {
            if (!Categories.Any(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
            {
                Categories.Add(new Category { Name = categoryName });
                OnDictionaryUpdated();
            }
        }

        public void AddWordToCategory(string categoryName, Word word)
        {
            var category = Categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (category != null)
            {
                if (!category.Words.Any(w => w.WordText.Equals(word.WordText, StringComparison.OrdinalIgnoreCase)))
                {
                    category.Words.Add(word);
                    OnDictionaryUpdated();
                }
            }
        }

        // Метод загрузки словаря из JSON
        public void LoadFromJson(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);

                // Парсинг JSON с проверкой структуры
                JArray jsonArray = JArray.Parse(json);
                var categories = jsonArray.ToObject<List<Category>>();

                if (categories == null)
                    throw new Exception("JSON-файл пуст или имеет неверный формат.");

                // Дополнительная проверка каждого элемента
                foreach (var category in categories)
                {
                    if (string.IsNullOrWhiteSpace(category.Name))
                        throw new Exception("У одной из категорий отсутствует поле 'Name'.");

                    if (category.Words == null || !category.Words.Any())
                        throw new Exception($"Категория '{category.Name}' не содержит слов.");

                    foreach (var word in category.Words)
                    {
                        if (string.IsNullOrWhiteSpace(word.WordText) ||
                            string.IsNullOrWhiteSpace(word.Translation) ||
                            string.IsNullOrWhiteSpace(word.Transcription))
                        {
                            throw new Exception($"В категории '{category.Name}' обнаружено слово с незаполненными полями.");
                        }
                    }
                }

                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
                OnDictionaryUpdated();
            }
            catch (JsonReaderException)
            {
                throw new Exception("Некорректный JSON формат.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Метод сохранения словаря в JSON
        public void SaveToJson(string filePath)
        {
            var json = JsonConvert.SerializeObject(Categories, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        protected void OnDictionaryUpdated()
        {
            DictionaryUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteWord(string categoryName, Word word)
        {
            var category = Categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (category != null)
            {
                category.Words.Remove(word);
                OnDictionaryUpdated();
            }
        }

        public void DeleteCategory(string categoryName)
        {
            var category = Categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (category != null)
            {
                Categories.Remove(category);
                OnDictionaryUpdated();
            }
        }
    }
}
