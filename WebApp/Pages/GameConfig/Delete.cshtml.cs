using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;

namespace WebApp.Pages_GameConfig
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public DeleteModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GameConfigDb = await _context.GameConfigDb.FindAsync(id);

            if (GameConfigDb != null)
            {
                _context.GameConfigDb.Remove(GameConfigDb);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
