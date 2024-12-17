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
using System.Diagnostics;


namespace WpfApp4
{
    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DictionaryModel _dictionary;
        private manager manager = new manager();
        private Category _selectedCategory;
        private readonly string _defaultDictionaryPath = "default_dictionary.json";
        private UserStatistics _userStatistics;
        

        // Хранение начальных категорий
        private List<Category> _initialCategories = new List<Category>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Category> Categories => _dictionary.Categories;

        private int _currentQuestionIndex;
        private double _score;


        private string _currentWord;
        public string CurrentWord
        {
            get => _currentWord;
            set
            {
                if (_currentWord != value)
                {
                    _currentWord = value;
                    OnPropertyChanged(nameof(CurrentWord));
                }
            }
        }

        private string _option1;
        public string Option1
        {
            get => _option1;
            set
            {
                if (_option1 != value)
                {
                    _option1 = value;
                    OnPropertyChanged(nameof(Option1));
                }
            }
        }

        private string _option2;
        public string Option2
        {
            get => _option2;
            set
            {
                if (_option2 != value)
                {
                    _option2 = value;
                    OnPropertyChanged(nameof(Option2));
                }
            }
        }

        private string _option3;
        public string Option3
        {
            get => _option3;
            set
            {
                if (_option3 != value)
                {
                    _option3 = value;
                    OnPropertyChanged(nameof(Option3));
                }
            }
        }

        private int _currentQuestionNumber;
        public int CurrentQuestionNumber
        {
            get => _currentQuestionNumber;
            set
            {
                if (_currentQuestionNumber != value)
                {
                    _currentQuestionNumber = value;
                    OnPropertyChanged(nameof(CurrentQuestionNumber));
                }
            }
        }

        public double Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged(nameof(Score));
                }
            }
        }

        private double _maxScore;
        public double MaxScore
        {
            get => _maxScore;
            set
            {
                if (_maxScore != value)
                {
                    _maxScore = value;
                    OnPropertyChanged(nameof(MaxScore));
                }
            }
        }

        private int _testsCompleted;
        public int TestsCompleted
        {
            get => _testsCompleted;
            set
            {
                if (_testsCompleted != value)
                {
                    _testsCompleted = value;
                    OnPropertyChanged(nameof(TestsCompleted));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _dictionary = new DictionaryModel();
            _dictionary.DictionaryUpdated += Dictionary_DictionaryUpdated;
            DataContext = this;  
            LoadUserStatistics(); // Загружаем статистику пользователя
            MessageBox.Show($"Ваш идентификатор: {_userStatistics.Identifier}", "Добро пожаловать", MessageBoxButton.OK, MessageBoxImage.Information);
            if (File.Exists(_defaultDictionaryPath))
            {
                try
                {
                    _dictionary.LoadFromJson(_defaultDictionaryPath);
                    // Сохраняем копию начальных категорий
                    _initialCategories = _dictionary.Categories.Select(c => new Category
                    {
                        Name = c.Name,
                        Words = new ObservableCollection<Word>(c.Words.Select(w => new Word
                        {
                            WordText = w.WordText,
                            Translation = w.Translation,
                            Transcription = w.Transcription
                        }))
                    }).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке словаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                CreateEmptyDictionary(_defaultDictionaryPath);
                _initialCategories = new List<Category>();
            }
            LoadProfile();
            // Обработка события закрытия окна
            this.Closing += MainWindow_Closing;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Dictionary_DictionaryUpdated(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Categories));
            OnPropertyChanged(nameof(SelectedCategory));
            // Сохранение изменений автоматически
            try
            {
                _dictionary.SaveToJson(_defaultDictionaryPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении словаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserStatistics()
        {
            var statsFile = "user_statistics.json";
            if (File.Exists(statsFile))
            {
                var json = File.ReadAllText(statsFile);
                manager = JsonConvert.DeserializeObject<manager>(json);
                _userStatistics = new UserStatistics
                {
                    Identifier = manager.GenerateIdentifier(),
                    AccumulatedScore = 0,
                    TestsCompleted = 0
                };
                manager.Add_New_Profile(_userStatistics);
                manager.StatNow = _userStatistics;
            }
            else
            {
                // Генерируем новый идентификатор от 001 до 999
                _userStatistics = new UserStatistics
                {
                    Identifier = manager.GenerateIdentifier(),
                    AccumulatedScore = 0,
                    TestsCompleted = 0
                };
                manager.Add_New_Profile(_userStatistics);
                manager.StatNow = _userStatistics;
                manager.SaveUserStatistics(MaxScore, TestsCompleted);

            }

            // Инициализируем MaxScore и TestsCompleted из статистики
            MaxScore = manager.StatNow.AccumulatedScore;
            TestsCompleted = manager.StatNow.TestsCompleted;
        }

        private void LoadDictionary_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _dictionary.LoadFromJson(openFileDialog.FileName);
                    // Обновляем начальные категории
                    _initialCategories = _dictionary.Categories.Select(c => new Category
                    {
                        Name = c.Name,
                        Words = new ObservableCollection<Word>(c.Words.Select(w => new Word
                        {
                            WordText = w.WordText,
                            Translation = w.Translation,
                            Transcription = w.Transcription
                        }))
                    }).ToList();

                    MessageBox.Show("Словарь успешно загружен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке словаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveDictionary_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _dictionary.SaveToJson(saveFileDialog.FileName);
                    MessageBox.Show("Словарь успешно сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении словаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
            }
        }
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = Prompt.ShowDialog("Введите название категории:", "Добавить Категоию");
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                if (_dictionary.Categories.Any(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Такая категория уже существует.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _dictionary.AddCategory(categoryName);
            }
        }

        private void AddWord_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для добавления слова.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string wordText = Prompt.ShowDialog($"Введите слово для категории '{SelectedCategory.Name}':", "Добавить Слово");
            if (string.IsNullOrWhiteSpace(wordText))
                return;

            string translation = Prompt.ShowDialog("Введите перевод слова:", "Добавить Перевод");
            if (string.IsNullOrWhiteSpace(translation))
                return;

            string transcription = Prompt.ShowDialog("Введите транскрипцию слова:", "Добавить Транскрипцию");
            if (string.IsNullOrWhiteSpace(transcription))
                return;

            var newWord = new Word
            {
                WordText = wordText,
                Translation = translation,
                Transcription = transcription
            };

            try
            {
                _dictionary.AddWordToCategory(SelectedCategory.Name, newWord);
                MessageBox.Show("Слово успешно добавлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении слова: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MergeDictionaries_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Multiselect = true,
                Title = "Выберите словари для объединения"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var categoriesToMerge = JsonConvert.DeserializeObject<List<Category>>(json);

                        if (categoriesToMerge == null)
                        {
                            MessageBox.Show($"Файл {Path.GetFileName(file)} пуст или имеет неверный формат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }

                        foreach (var category in categoriesToMerge)
                        {
                            var existingCategory = _dictionary.Categories.FirstOrDefault(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase));
                            if (existingCategory != null)
                            {
                                foreach (var word in category.Words)
                                {
                                    if (!existingCategory.Words.Any(w => w.WordText.Equals(word.WordText, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        existingCategory.Words.Add(word);
                                    }
                                }
                            }
                            else
                            {
                                _dictionary.Categories.Add(new Category
                                {
                                    Name = category.Name,
                                    Words = new ObservableCollection<Word>(category.Words)
                                });
                            }
                        }

                        MessageBox.Show($"Словарь {Path.GetFileName(file)} успешно объединён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (JsonReaderException)
                    {
                        MessageBox.Show($"Файл {Path.GetFileName(file)} содержит некорректный JSON.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при объединении файла {Path.GetFileName(file)}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Сохранение объединённого словаря
                try
                {
                    _dictionary.SaveToJson(_defaultDictionaryPath);
                    MessageBox.Show("Все выбранные словари успешно объединены и сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении объединённого словаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Метод для создания пустого словаря
        private void CreateEmptyDictionary(string filePath)
        {
            try
            {
                var emptyCategories = new List<Category>();
                var json = JsonConvert.SerializeObject(emptyCategories, Formatting.Indented);
                File.WriteAllText(filePath, json);
                MessageBox.Show("Пустой словарь успешно создан.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании словаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик события закрытия окна
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Сбрасываем категории к начальным
            _dictionary.Categories.Clear();
            foreach (var category in _initialCategories)
            {
                _dictionary.Categories.Add(new Category
                {
                    Name = category.Name,
                    Words = new ObservableCollection<Word>(category.Words.Select(w => new Word
                    {
                        WordText = w.WordText,
                        Translation = w.Translation,
                        Transcription = w.Transcription
                    }))
                });
            }

            // Сохраняем сброшенные категории
            try
            {
                _dictionary.SaveToJson(_defaultDictionaryPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении словаря при закрытии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик нажатия кнопки "Генератор теста"
        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для начала теста.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCategory.Words.Count < 3) // Минимум 3 слова для теста
            {
                MessageBox.Show("В категории должно быть минимум 3 слова для тестирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            manager.GenerateQuestions(SelectedCategory);
            _currentQuestionIndex = 0;
            _score = 1000;
            CurrentQuestionNumber = 1;
            Score = _score;

            ShowCurrentQuestion();

            // Скрываем все элементы кроме панели теста
            LeftPanel.Visibility = Visibility.Collapsed;
            CategoriesListBox.Visibility = Visibility.Collapsed;
            TestPanel.Visibility = Visibility.Visible;
        }

        // Генерация вопросов на основе выбранной категории
        

        // Отображение текущего вопроса
        private void ShowCurrentQuestion()
        {
            if (_currentQuestionIndex < manager._totalQuestions)
            {
                var currentQuestion = manager._questions[_currentQuestionIndex];
                CurrentWord = currentQuestion.Word.WordText;
                Option1 = currentQuestion.Options[0];
                Option2 = currentQuestion.Options[1];
                Option3 = currentQuestion.Options[2];
            }
            else
            {
                EndTest();
            }
        }

        // Обработчик нажатия на вариант ответа
        private void Option_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            string selectedOption = button.Content.ToString();
            var currentQuestion = manager._questions[_currentQuestionIndex];

            if (selectedOption == currentQuestion.CorrectOption)
            {
                // Правильный ответ - переходим к следующему вопросу
                _currentQuestionIndex++;
                CurrentQuestionNumber++;

                if (_currentQuestionIndex < manager._totalQuestions)
                {
                    ShowCurrentQuestion();
                }
                else
                {
                    EndTest();
                }
            }
            else
            {
                // Неправильный ответ - уменьшаем счет
                double pointsPerQuestion = 1000.0 / manager._totalQuestions;
                _score = Math.Max(0, _score - pointsPerQuestion); // Не даем счету упасть ниже 0
                Score = _score; // Обновляем отображаемый счет

                // Изменяем цвет кнопки на красный
                var clickedButton = sender as Button;
                clickedButton.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Красный цвет

                // Возврщаем исходный цвет через 1 секунду
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                timer.Tick += (s, args) =>
                {
                    clickedButton.ClearValue(Button.BackgroundProperty);
                    timer.Stop();
                };
                timer.Start();
            }
        }

        // Завершение теста и отображение оценки
        private void EndTest()
        {
            string grade;
            if (Score >= 1000)
                grade = "Блистательный Эльф";
            else if (Score >= 750)
                grade = "Эльф";
            else if (Score >= 500)
                grade = "Полируслик";
            else if (Score >= 250)
                grade = "Гном";
            else
                grade = "Не соответствует ни одному уровню";

            MaxScore += Score;
            TestsCompleted++;
            manager.SaveUserStatistics(MaxScore, TestsCompleted);
            MessageBox.Show($"Тест завершён!\n" +
                           $"Ваши очки: {Score:F2}\n" +
                           $"Общий накопленный счет: {MaxScore:F2}\n" +
                           $"Всего пройдено тестов: {TestsCompleted}\n" +
                           $"Оценка: {grade}",
                           "Результат", MessageBoxButton.OK, MessageBoxImage.Information);

            // Восстанавливаем видимость всех элементов
            TestPanel.Visibility = Visibility.Collapsed;
            LeftPanel.Visibility = Visibility.Visible;
            CategoriesListBox.Visibility = Visibility.Visible;
        }

        // Добаить новые методы для сохранения/загрузки максимального счета


   

        // Вспомогательный класс для тображения окна ввода
        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Window prompt = new Window()
                {
                    Width = 400,
                    Height = 200,
                    Title = caption,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                StackPanel stack = new StackPanel() { Margin = new Thickness(10) };
                TextBlock textBlock = new TextBlock() { Text = text };
                TextBox input = new TextBox() { Margin = new Thickness(0, 10, 0, 10) };
                Button okButton = new Button() { Content = "OK", Width = 80, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };

                okButton.Click += (sender, e) => { prompt.DialogResult = true; prompt.Close(); };

                stack.Children.Add(textBlock);
                stack.Children.Add(input);
                stack.Children.Add(okButton);
                prompt.Content = stack;

                return prompt.ShowDialog() == true ? input.Text : string.Empty;
            }
        }

        private void DeleteWord_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для удаления слова.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCategory.Words.Count == 0)
            {
                MessageBox.Show("В выбранной категории нет слов для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var wordToDelete = Prompt.ShowDialog("Введите слово для удаления:", "Удалить слово");
            if (!string.IsNullOrWhiteSpace(wordToDelete))
            {
                var word = SelectedCategory.Words.FirstOrDefault(w =>
                    w.WordText.Equals(wordToDelete, StringComparison.OrdinalIgnoreCase));

                if (word != null)
                {
                    _dictionary.DeleteWord(SelectedCategory.Name, word);
                    MessageBox.Show("Слово успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Слово не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить категорию '{SelectedCategory.Name}' и все её слова?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _dictionary.DeleteCategory(SelectedCategory.Name);
                SelectedCategory = null;
                MessageBox.Show("Категория успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetStatistics_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите обнулить всю статистику?\nЭто действие нельзя отменить.",
                "Подтверждение сброса",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MaxScore = 0;
                TestsCompleted = 0;
                MessageBox.Show("Статистика успешно сброшена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            manager.StatNow = manager.GetProfFromId(Profile_box.SelectedItem.ToString());
            SwithProfile();
        }

        private void LoadProfile()
        {
            Profile_box.Items.Clear();
            Profile_box.ItemsSource = manager.GetProfilesId(manager.profiles);
        }

        private void SwithProfile()
        {
            MaxScore = manager.StatNow.AccumulatedScore;
            TestsCompleted = manager.StatNow.TestsCompleted;
        }
    }
}