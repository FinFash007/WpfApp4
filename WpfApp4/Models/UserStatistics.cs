using System;

namespace WpfApp4.Models
{
    public class UserStatistics
    {
        public string Identifier { get; set; } // ������������� �� 001 �� 999
        public double AccumulatedScore { get; set; } // ����������� ����
        public int TestsCompleted { get; set; } // ���������� ���������� ������
        public DateTime LastTestDate { get; set; } // ���� ���������� �����
    }
} 