using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TextbookExchange.Data;
using TextbookExchange.Models;

namespace TextbookExchange.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Books - с поиском и фильтрацией
        public async Task<IActionResult> Index(string search, string subject, string course, string condition)
        {
            var booksQuery = _context.Books
                .Include(b => b.User)
                .AsQueryable();

            // Поиск по названию и автору
            if (!string.IsNullOrEmpty(search))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    b.Subject.Contains(search));
            }

            // Фильтрация по предмету
            if (!string.IsNullOrEmpty(subject) && subject != "all")
            {
                booksQuery = booksQuery.Where(b => b.Subject == subject);
            }

            // Фильтрация по курсу
            if (!string.IsNullOrEmpty(course) && course != "all")
            {
                booksQuery = booksQuery.Where(b => b.Course == course);
            }

            // Фильтрация по состоянию
            if (!string.IsNullOrEmpty(condition) && condition != "all")
            {
                booksQuery = booksQuery.Where(b => b.Condition == condition);
            }

            // УБРАНА ФИЛЬТРАЦИЯ ПО ТИПУ

            var books = await booksQuery
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Получаем уникальные значения для фильтров
            var subjects = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Subject))
                .Select(b => b.Subject)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var courses = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Course))
                .Select(b => b.Course)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var conditions = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.Condition))
                .Select(b => b.Condition)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var viewModel = new BookSearchViewModel
            {
                Books = books,
                Search = search,
                SelectedSubject = subject,
                SelectedCourse = course,
                SelectedCondition = condition,
                // УБРАН SelectedType
                Subjects = subjects,
                Courses = courses,
                Conditions = conditions
            };

            return View(viewModel);
        }

        // GET: /Books/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Books/Create
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Book book)
        {
            Console.WriteLine("=== СОЗДАНИЕ УЧЕБНИКА ===");

            // ВРЕМЕННО отключаем валидацию для User и UserId
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("✅ Модель валидна");

                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    book.UserId = user.Id;
                    book.CreatedAt = DateTime.Now;

                    Console.WriteLine($"📚 Добавляем книгу: '{book.Title}', Автор: '{book.Author}'");
                    Console.WriteLine($"👤 Пользователь: {user.UserName}, ID: {user.Id}");

                    _context.Books.Add(book);
                    var result = await _context.SaveChangesAsync();

                    Console.WriteLine($"🎉 Книга сохранена! ID: {book.Id}, Результат: {result}");

                    TempData["SuccessMessage"] = $"Учебник '{book.Title}' успешно добавлен!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥 Ошибка сохранения: {ex.Message}");
                    Console.WriteLine($"Stack: {ex.StackTrace}");
                    TempData["ErrorMessage"] = $"Ошибка при сохранении: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("❌ Модель не валидна. Ошибки:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }

                TempData["ErrorMessage"] = "Исправьте ошибки в форме";
            }

            return View(book);
        }

        // GET: /Books/MyBooks
        [Authorize]
        public async Task<IActionResult> MyBooks()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var books = await _context.Books
                    .Where(b => b.UserId == user.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"📚 Найдено моих книг: {books.Count} для пользователя {user.UserName}");
                return View(books);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Ошибка в MyBooks: {ex.Message}");
                TempData["ErrorMessage"] = "Ошибка при загрузке ваших учебников";
                return View(new List<Book>());
            }
        }

        // GET: /Books/CheckOwnership - для отладки
        [Authorize]
        public async Task<IActionResult> CheckOwnership()
        {
            var user = await _userManager.GetUserAsync(User);
            var myBooks = await _context.Books
                .Where(b => b.UserId == user.Id)
                .ToListAsync();

            string result = $"<h1>Мои книги (пользователь: {user.UserName})</h1>";
            result += $"<p>ID пользователя: {user.Id}</p>";
            result += $"<p>Всего книг: {myBooks.Count}</p>";

            foreach (var book in myBooks)
            {
                result += $"<p>📚 {book.Title} (ID: {book.Id}, Владелец: {book.UserId})</p>";
            }

            return Content(result, "text/html");
        }
    }
}