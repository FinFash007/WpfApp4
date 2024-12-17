using System;

namespace WpfApp4.Models
{
    public class UserStatistics
    {
        public string Identifier { get; set; } // Идентификатор от 001 до 999
        public double AccumulatedScore { get; set; } // Накопленные очки
        public int TestsCompleted { get; set; } // Количество пройденных тестов
        public DateTime LastTestDate { get; set; } // Дата последнего теста
    }
} 