using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TextbookExchange.Data;
using TextbookExchange.Models;

namespace TextbookExchange.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Complaints
        public async Task<IActionResult> Complaints()
        {
            var complaints = await _context.Complaints
                .Include(c => c.Author)
                .Include(c => c.TargetUser)
                .Include(c => c.Exchange)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

        // POST: /Admin/UpdateComplaintStatus
        [HttpPost]
        public async Task<IActionResult> UpdateComplaintStatus(int id, ComplaintStatus status, string adminNotes = null)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = status;
            complaint.AdminNotes = adminNotes;
            complaint.ResolvedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Статус жалобы обновлен";
            return RedirectToAction("Complaints");
        }
    }
}