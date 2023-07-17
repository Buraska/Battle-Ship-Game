using System.Threading.Tasks;
using BattleShipBrain;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.GamePlay
{
    public class CreateNewGame : PageModel
    {
        private readonly ApplicationDbContext _context;
        
        public CreateNewGame(ApplicationDbContext context)
        {
            _context = context;
        }

        public BsBrain BsBrain { get; set; }
        public GameConfigDb GameConfigDb { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GameConfigDb = await _context.GameConfigDb.FirstOrDefaultAsync(m => m.GameConfigDbId == id);

            if (GameConfigDb == null)
            {
                return NotFound();
            }
            BsBrain = new BsBrain(BsBrain.GetGameConfigFromString(GameConfigDb.GameConfigJson));

            var gameState = new GameStateDb()
            {
                ConfigName = GameConfigDb.ConfigName,
                GameStateJson = BsBrain.GetGameStateString(GameConfigDb.ConfigName)
            };

            _context.GameStateDbs.Add(gameState);
            _context.SaveChanges();

            gameState.ConfigName += gameState.GameStateDbId;
            _context.SaveChanges();
            

            return Redirect($"../GamePlay?id={gameState.GameStateDbId}");
        }
    }
}