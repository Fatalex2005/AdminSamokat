using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AdminSamokat.Models
{
    public class Access
    {
        // Класс, описывающий таблицу Accesses
        [JsonPropertyName("id")]
        public ulong Id { get; set; } // Уникальный идентификатор записи

        [JsonPropertyName("date")]
        public DateTime Date { get; set; } // Дата доступности

        [JsonPropertyName("startChange")]
        public TimeSpan StartChange { get; set; }

        [JsonPropertyName("endChange")]
        public TimeSpan EndChange { get; set; }

        [JsonPropertyName("confirm")]
        public int Confirm { get; set; } // Подтверждение доступности

        [JsonPropertyName("user_id")]
        public ulong UserId { get; set; } // ID пользователя, связанного с доступностью

        public Tuple<int, DateTime> ConfirmAndDate => new Tuple<int, DateTime>(Confirm, Date); // Свойство для комбинирования Confirm и Date
        public string UserFullName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsVisible { get; set; } = false; // По умолчанию доступности скрыты
    }
}
