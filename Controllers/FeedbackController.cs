using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TextbookExchange.Data;
using TextbookExchange.Models;

namespace TextbookExchange.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Feedback/LeaveReview/{targetUserId}
        public async Task<IActionResult> LeaveReview(string targetUserId, int? exchangeId = null)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
            {
                return NotFound();
            }

            // Проверяем что был обмен с этим пользователем
            var currentUser = await _userManager.GetUserAsync(User);
            var hasExchange = await _context.Exchanges
                .AnyAsync(e => (e.InitiatorId == currentUser.Id && e.TargetUserId == targetUserId) ||
                              (e.TargetUserId == currentUser.Id && e.InitiatorId == targetUserId));

            if (!hasExchange)
            {
                TempData["ErrorMessage"] = "Вы можете оставить отзыв только пользователям, с которыми был обмен";
                return RedirectToAction("Index", "Books");
            }

            var viewModel = new LeaveReviewViewModel
            {
                TargetUserId = targetUserId,
                TargetUserName = targetUser.FullName,
                ExchangeId = exchangeId
            };

            return View(viewModel);
        }

        // POST: /Feedback/LeaveReview
        [HttpPost]
        public async Task<IActionResult> LeaveReview(LeaveReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var review = new Review
                {
                    AuthorId = currentUser.Id,
                    TargetUserId = model.TargetUserId,
                    ExchangeId = model.ExchangeId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Отзыв успешно оставлен!";
                return RedirectToAction("UserProfile", "Account", new { userId = model.TargetUserId });
            }

            return View(model);
        }

        // GET: /Feedback/CreateComplaint/{targetUserId}
        public async Task<IActionResult> CreateComplaint(string targetUserId, int? exchangeId = null)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
            {
                return NotFound();
            }

            var viewModel = new CreateComplaintViewModel
            {
                TargetUserId = targetUserId,
                TargetUserName = targetUser.FullName,
                ExchangeId = exchangeId
            };

            return View(viewModel);
        }

        // POST: /Feedback/CreateComplaint
        [HttpPost]
        public async Task<IActionResult> CreateComplaint(CreateComplaintViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var complaint = new Complaint
                {
                    AuthorId = currentUser.Id,
                    TargetUserId = model.TargetUserId,
                    ExchangeId = model.ExchangeId,
                    Type = model.Type,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    Status = ComplaintStatus.Pending
                };

                _context.Complaints.Add(complaint);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Жалоба отправлена администраторам. Мы рассмотрим её в ближайшее время.";
                return RedirectToAction("Index", "Books");
            }

            return View(model);
        }

        // GET: /Feedback/MyReviews
        public async Task<IActionResult> MyReviews()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var reviews = await _context.Reviews
                .Include(r => r.Author)
                .Include(r => r.TargetUser)
                .Include(r => r.Exchange)
                .Where(r => r.TargetUserId == currentUser.Id && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Расчет рейтинга
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            var ratingStats = new
            {
                Average = Math.Round(averageRating, 1),
                Count = reviews.Count,
                FiveStars = reviews.Count(r => r.Rating == 5),
                FourStars = reviews.Count(r => r.Rating == 4),
                ThreeStars = reviews.Count(r => r.Rating == 3),
                TwoStars = reviews.Count(r => r.Rating == 2),
                OneStar = reviews.Count(r => r.Rating == 1)
            };

            ViewBag.RatingStats = ratingStats;

            return View(reviews);
        }

        // GET: /Feedback/UserReviews/{userId}
        [AllowAnonymous]
        public async Task<IActionResult> UserReviews(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Include(r => r.Author)
                .Include(r => r.Exchange)
                .Where(r => r.TargetUserId == userId && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Расчет рейтинга
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            var ratingStats = new
            {
                Average = Math.Round(averageRating, 1),
                Count = reviews.Count,
                FiveStars = reviews.Count(r => r.Rating == 5),
                FourStars = reviews.Count(r => r.Rating == 4),
                ThreeStars = reviews.Count(r => r.Rating == 3),
                TwoStars = reviews.Count(r => r.Rating == 2),
                OneStar = reviews.Count(r => r.Rating == 1)
            };

            ViewBag.RatingStats = ratingStats;
            ViewBag.TargetUser = user;

            return View(reviews);
        }
    }
}