using System.Collections.Generic;
using System.Threading.Tasks;
using BattleShipBrain;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.GamePlay
{
    public class Index : PageModel
    {
        
        private readonly ApplicationDbContext _context;
        
        public GameStateDb GameStateDb { get;set; }
        public BsBrain BsBrain { get;set; }
        public int X { get; set; }
        public int Y { get; set; }

        public List<int[]> Pointers { get; set; } = new List<int[]>();
        public int BoardId { get; set; }

        public string Winner { get; set; }

        public Index(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id, int? x, int? y, int? oldX, int? oldY, int? boardId)
        {
            if (id == null)
            {
                return NotFound();
            }

            GameStateDb = await _context.GameStateDbs.FirstOrDefaultAsync(m => m != null && m.GameStateDbId == id);

            if (GameStateDb == null)
            {
                return NotFound();
            }

            BsBrain = BsBrain.CreateBsBrainWithString(GameStateDb.GameStateJson);

            if (BsBrain.GameIsFinished)
            {
                Winner = BsBrain.GetAliveBoards()[0].Name;
                return Page();
            }
            
            if (boardId == null || x == null || y == null)
            {
                return Page();
            }

            X = (int) oldX;
            Y = (int) oldY;
            if (x != X || y != Y)
            {
                BoardId = (int) boardId;
                X = (int) x;
                Y = (int) y;
                Pointers = BsBrain.GetCursorSize(X, Y);
                return Page();
            }
            
            var command = new Command()
            {
                ChosenBoard = BsBrain.Players.Find(playerBoard => playerBoard.PlayerId == boardId),
                Pointer = new []{X,Y}
            };

            if (BsBrain.BuildingStage)
            {
                BsBrain.BuildShipAtBoard(command);
            }
            else
            {
                BsBrain.AttackAt(command);
            }

            if (BsBrain.GameIsFinished)
            {
                Winner = BsBrain.GetAliveBoards()[0].Name;
            }
            
            GameStateDb.GameStateJson = BsBrain.GetGameStateString(GameStateDb.ConfigName);
            await _context.SaveChangesAsync();
            return Page();
        }
    }
}