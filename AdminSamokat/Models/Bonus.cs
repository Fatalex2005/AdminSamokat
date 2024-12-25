using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdminSamokat.Models
{
    public class Bonus
    {
        // Класс, описывающий таблицу bonuses
        [JsonPropertyName("id")]
        public ulong Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("price")]
        public string Price { get; set; }
        [JsonPropertyName("role_id")]
        public ulong RoleId { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        // Свойство для связанной роли
        [JsonPropertyName("role")]
        public Role Role { get; set; }
        // Свойство для форматированной цены
        public string FormattedPrice
        {
            get
            {
                if (string.IsNullOrEmpty(Price))
                    return "0 ₽";

                // Разделяем рубли и копейки
                var parts = Price.Split('.');
                var rubles = parts[0];
                var kopecks = parts.Length > 1 ? parts[1] : "00";

                // Если копейки "00", отображаем только рубли
                if (kopecks == "00")
                    return $"{rubles} ₽";

                // Иначе отображаем рубли и копейки
                return $"{rubles}.{kopecks} ₽";
            }
        }
    }
}
