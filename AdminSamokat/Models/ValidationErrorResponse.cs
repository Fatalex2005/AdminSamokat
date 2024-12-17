using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminSamokat.Models
{
    public class ValidationErrorResponse
    {
        public int Code { get; set; } // Код ошибки
        public string Message { get; set; } // Общее сообщение об ошибке
        public Dictionary<string, List<string>> Errors { get; set; } // Детализированные ошибки
    }
}
