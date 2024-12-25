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
    }
}
