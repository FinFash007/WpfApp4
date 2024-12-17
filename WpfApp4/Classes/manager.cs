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
using System.Runtime;

namespace WpfApp4.Classes
{
    public class manager
    {
        public UserStatistics StatNow;
        public List<Question> _questions = new List<Question>();
        public List<UserStatistics> profiles = new List<UserStatistics>();
        public int _totalQuestions { get; set; }
        public static string[] GetProfilesId(List<UserStatistics> profiles)
        {
            return profiles.Select(profile => profile.Identifier).ToArray();
        }
        public void GenerateQuestions(Category category)
        {
            if (category.Words.Count < 3)
            {
                throw new InvalidOperationException("Недостаточно слов для создания теста");
            }
            _questions = new List<Question>();
            var words = category.Words.ToList();
            Random rand = new Random();

            foreach (var word in words)
            {
                // Получение неправильных вариантов из других слов в категории
                var incorrectTranslations = words.Where(w => w.WordText != word.WordText)
                                                 .Select(w => w.Translation)
                                                 .OrderBy(x => rand.Next())
                                                 .Take(2)
                                                 .ToList();

                // Варианты ответа
                var options = new List<string>
                {
                    word.Translation,
                    incorrectTranslations.ElementAtOrDefault(0) ?? "Неизвестно",
                    incorrectTranslations.ElementAtOrDefault(1) ?? "Неизвестно"
                };

                // Перемешиваем варианты
                options = options.OrderBy(x => rand.Next()).ToList();

                _questions.Add(new Question
                {
                    Word = word,
                    Options = options,
                    CorrectOption = word.Translation
                });
            }

            _totalQuestions = _questions.Count;
        }

        public string GenerateIdentifier()
        {
            // Проверяем, существует ли файл статистики
            if (!File.Exists("user_statistics.json"))
            {
                return "001"; // При первом запуске присваиваем идентификатор '001'
            }
            else
            {
                // При последующих запусках генерируем случайный идентификатор от 002 до 999
                Random rand = new Random();
                int id = rand.Next(2, 1000); // Генерируем число от 2 до 999
                return id.ToString("D3"); // Преобразуем в строку с ведущими нулями
            }
        }


     public void Add_New_Profile(UserStatistics profile)
        {
            profiles.Add(profile);
        }



     public UserStatistics GetProfFromId(string id) 
     {
            return profiles.Find(profile => profile.Identifier == id);
     }
    public void SaveUserStatistics(double MaxScore, int TestsCompleted)
    {
            StatNow.AccumulatedScore = MaxScore;
            StatNow.TestsCompleted = TestsCompleted;

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText("user_statistics.json", json);
    }
















    }


}
