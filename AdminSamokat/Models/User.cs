using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdminSamokat.Models
{
    public class User
    {
        // Класс, описывающий таблицу users
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("patronymic")]
        public string? Patronymic { get; set; }
        
        [JsonPropertyName("login")]
        public string Login { get; set; }
        
        [JsonPropertyName("password")]
        public string? Password { get; set; } = null;
        
        [JsonPropertyName("api_token")]
        public string? ApiToken { get; set; }
        
        [JsonPropertyName("role_id")]
        public ulong RoleId { get; set; }
        [JsonPropertyName("fine_id")]
        public ulong FineId { get; set; }
        [JsonPropertyName("status_id")]
        public ulong StatusId { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
