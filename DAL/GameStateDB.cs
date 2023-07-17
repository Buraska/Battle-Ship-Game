using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class GameStateDb
    {
        public int GameStateDbId { get; set; }
        
        [MinLength(2)]
        [MaxLength(128)] 
        public string ConfigName { get; set; } = "";
        public string GameStateJson { get; set; } = "";
    }
}