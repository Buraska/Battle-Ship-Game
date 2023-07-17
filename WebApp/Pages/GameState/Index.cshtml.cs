using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;

namespace WebApp.Pages_GameState
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<GameStateDb> GameStateDb { get;set; } = null!;

        public async Task OnGetAsync()
        {
            GameStateDb = await _context.GameStateDbs.ToListAsync();
        }
    }
}
