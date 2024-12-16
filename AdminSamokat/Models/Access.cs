using System;
using System.Text.Json.Serialization;

namespace AdminSamokat.Models
{
    internal class Access
    {
        // Класс, описывающий таблицу Accesses
        [JsonPropertyName("id")]
        public ulong Id { get; set; } // Уникальный идентификатор записи

        [JsonPropertyName("date")]
        public DateTime Date { get; set; } // Дата доступности

        [JsonPropertyName("startChange")]
        public TimeSpan StartChange { get; set; } // Время начала смены

        [JsonPropertyName("EndChange")]
        public TimeSpan EndChange { get; set; } // Время окончания смены

        [JsonPropertyName("confirm")]
        public bool Confirm { get; set; } // Подтверждение доступности

        [JsonPropertyName("user_id")]
        public ulong UserId { get; set; } // ID пользователя, связанного с доступностью
    }
}
