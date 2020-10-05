using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer;

namespace AisBuchung_Api.Models
{
    public class AdminsModel
    {
        public bool PostAdmin(long organizerId)
        {
            var adminPost = new Dictionary<string, string> { { "id", organizerId.ToString() } };
            return DatabaseManager.ExecutePost("Nutzerdaten", adminPost) == organizerId;
        }

    }
}
