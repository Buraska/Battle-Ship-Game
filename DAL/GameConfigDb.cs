using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class GameConfigDb
    {
        public int GameConfigDbId { get; set; }


        [MinLength(2)]
        [MaxLength(128)] 
        public string ConfigName { get; set; } = "";

        public string GameConfigJson { get; set; } = "";
    }
}