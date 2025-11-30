using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TextbookExchange.Data;
using TextbookExchange.Models;

namespace TextbookExchange.Controllers
{
    [Authorize]
    public class ExchangeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly string _logPath;

        public ExchangeController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), "exchange_debug.log");
        }

        private void Log(string message)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";

            // Пишем в файл
            System.IO.File.AppendAllText(_logPath, logMessage);

            // И в консоль
            Console.WriteLine(message);
        }

        // GET: /Exchange/MyExchanges - мои предложения обмена
        public async Task<IActionResult> MyExchanges()
        {
            var user = await _userManager.GetUserAsync(User);

            var exchanges = await _context.Exchanges
                .Include(e => e.OfferedBook)
                .Include(e => e.RequestedBook)
                .Include(e => e.Initiator)
                .Include(e => e.TargetUser)
                .Where(e => e.InitiatorId == user.Id || e.TargetUserId == user.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(exchanges);
        }

        // GET: /Exchange/Create/{requestedBookId} - форма предложения обмена
        // GET: /Exchange/Create/{requestedBookId} - форма предложения обмена
        public async Task<IActionResult> Create(int requestedBookId)
        {
            var requestedBook = await _context.Books
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == requestedBookId);

            if (requestedBook == null)
            {
                return NotFound();
            }

            // Нельзя предложить обмен на свою же книгу
            var currentUser = await _userManager.GetUserAsync(User);
            if (requestedBook.UserId == currentUser.Id)
            {
                TempData["ErrorMessage"] = "Нельзя предложить обмен на свою же книгу";
                return RedirectToAction("Index", "Books");
            }

            // Получаем книги текущего пользователя для выбора
            // УБРАНА ПРОВЕРКА ForExchange - теперь все книги для обмена
            var myBooks = await _context.Books
                .Where(b => b.UserId == currentUser.Id) // УБРАНО: && b.ForExchange
                .ToListAsync();

            if (!myBooks.Any())
            {
                TempData["ErrorMessage"] = "У вас нет книг для обмена. Сначала добавьте книгу.";
                return RedirectToAction("Create", "Books");
            }

            var viewModel = new CreateExchangeViewModel
            {
                RequestedBook = requestedBook,
                MyBooks = myBooks,
                RequestedBookId = requestedBookId
            };

            return View(viewModel);
        }

        // POST: /Exchange/Create - создание предложения обмена
        [HttpPost]
        public async Task<IActionResult> Create(CreateExchangeViewModel model)
        {
            Log("🎯 === НАЧАЛО СОЗДАНИЯ ОБМЕНА ===");

            // ВРЕМЕННО отключаем валидацию модели для тестирования
            ModelState.Clear();

            try
            {
                Log($"📦 RequestedBookId: {model?.RequestedBookId}");
                Log($"📦 OfferedBookId: {model?.OfferedBookId}");
                Log($"📦 Message: {model?.Message}");

                // Логируем данные формы
                Log("📨 FORM DATA:");
                foreach (var key in Request.Form.Keys)
                {
                    Log($"   {key} = '{Request.Form[key]}'");
                }

                // ПРОВЕРЯЕМ ОБЯЗАТЕЛЬНЫЕ ПОЛЯ ВРУЧНУЮ
                if (model.OfferedBookId == 0 || model.RequestedBookId == 0)
                {
                    Log("❌ Не выбраны книги для обмена!");
                    TempData["ErrorMessage"] = "Выберите книги для обмена";
                    return View(model);
                }

                Log("✅ Данные формы корректны!");

                var currentUser = await _userManager.GetUserAsync(User);
                Log($"👤 Текущий пользователь: {currentUser.UserName} (ID: {currentUser.Id})");

                // Проверяем книги
                var offeredBook = await _context.Books
                    .FirstOrDefaultAsync(b => b.Id == model.OfferedBookId && b.UserId == currentUser.Id);

                var requestedBook = await _context.Books
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == model.RequestedBookId);

                Log($"📚 OfferedBook: {offeredBook?.Title} (ID: {offeredBook?.Id}, Exists: {offeredBook != null})");
                Log($"📖 RequestedBook: {requestedBook?.Title} (ID: {requestedBook?.Id}, Exists: {requestedBook != null})");

                if (offeredBook == null || requestedBook == null)
                {
                    Log("❌ Книга не найдена!");
                    TempData["ErrorMessage"] = "Книга не найдена";
                    return RedirectToAction("Index", "Books");
                }

                // Проверяем что не обмениваемся с самим собой
                if (requestedBook.UserId == currentUser.Id)
                {
                    Log("❌ Попытка обмена с самим собой!");
                    TempData["ErrorMessage"] = "Нельзя предложить обмен на свою же книгу";
                    return RedirectToAction("Index", "Books");
                }

                Log($"🎯 Целевой пользователь: {requestedBook.User.UserName} (ID: {requestedBook.UserId})");

                // Проверяем дубликаты
                var existingExchange = await _context.Exchanges
                    .FirstOrDefaultAsync(e => e.OfferedBookId == model.OfferedBookId &&
                                            e.RequestedBookId == model.RequestedBookId &&
                                            e.Status == ExchangeStatus.Pending);

                if (existingExchange != null)
                {
                    Log("❌ Уже есть активное предложение!");
                    TempData["ErrorMessage"] = "Вы уже предлагали обмен с этими книгами";
                    return RedirectToAction("MyExchanges");
                }

                // СОЗДАЕМ ОБМЕН
                Log("🔄 Создаем новый обмен...");
                var exchange = new Exchange
                {
                    OfferedBookId = model.OfferedBookId,
                    RequestedBookId = model.RequestedBookId,
                    InitiatorId = currentUser.Id,
                    TargetUserId = requestedBook.UserId,
                    Message = model.Message,
                    Status = ExchangeStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                Log($"📝 Обмен создан: {exchange.OfferedBookId} -> {exchange.RequestedBookId}");

                // СОХРАНЯЕМ В БАЗУ
                Log("💾 Сохраняем в базу...");
                _context.Exchanges.Add(exchange);
                var result = await _context.SaveChangesAsync();

                Log($"🎉 УСПЕХ! Сохранено записей: {result}, ID обмена: {exchange.Id}");

                TempData["SuccessMessage"] = "Предложение обмена отправлено!";
                return RedirectToAction("MyExchanges");
            }
            catch (Exception ex)
            {
                Log($"💥 КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
                Log($"🔍 StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return View(model);
        }


        // POST: /Exchange/Accept/{id} - принять обмен
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            // Простейшее логирование которое точно сработает
            System.IO.File.AppendAllText("accept_test.log", $"{DateTime.Now} - Accept called with id: {id}\n");
            Console.WriteLine($"🎯 === ACCEPT CALLED - ID: {id} ===");

            try
            {
                var exchange = await _context.Exchanges
                    .Include(e => e.OfferedBook)
                    .Include(e => e.RequestedBook)
                    .FirstOrDefaultAsync(e => e.Id == id);

                System.IO.File.AppendAllText("accept_test.log", $"Exchange found: {exchange != null}\n");
                Console.WriteLine($"📋 Exchange found: {exchange != null}");

                if (exchange == null)
                {
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                System.IO.File.AppendAllText("accept_test.log", $"Current user: {currentUser?.UserName}, Target user: {exchange.TargetUserId}\n");
                Console.WriteLine($"👤 User: {currentUser?.UserName}, Target: {exchange.TargetUserId}");

                // Проверяем права
                if (exchange.TargetUserId != currentUser.Id)
                {
                    System.IO.File.AppendAllText("accept_test.log", "User mismatch - cannot accept\n");
                    TempData["ErrorMessage"] = "Вы не можете принять это предложение";
                    return RedirectToAction("MyExchanges");
                }

                if (exchange.Status != ExchangeStatus.Pending)
                {
                    System.IO.File.AppendAllText("accept_test.log", "Exchange already processed\n");
                    TempData["ErrorMessage"] = "Это предложение уже обработано";
                    return RedirectToAction("MyExchanges");
                }

                System.IO.File.AppendAllText("accept_test.log", "Starting book transfer...\n");
                Console.WriteLine("🔄 Starting book transfer...");

                // Меняем владельцев книг
                exchange.OfferedBook.UserId = currentUser.Id;
                exchange.RequestedBook.UserId = exchange.InitiatorId;

                exchange.Status = ExchangeStatus.Accepted;
                exchange.UpdatedAt = DateTime.Now;

                var result = await _context.SaveChangesAsync();
                System.IO.File.AppendAllText("accept_test.log", $"Saved changes: {result}\n");
                Console.WriteLine($"💾 Saved: {result} changes");

                TempData["SuccessMessage"] = "Обмен принят! Книги теперь у новых владельцев.";
                return RedirectToAction("MyExchanges");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("accept_test.log", $"ERROR: {ex.Message}\n");
                Console.WriteLine($"💥 Error: {ex.Message}");
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
                return RedirectToAction("MyExchanges");
            }
        }

        // POST: /Exchange/Reject/{id} - отклонить обмен
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var exchange = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exchange == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // Проверяем, что текущий пользователь - целевой пользователь обмена
            if (exchange.TargetUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Вы не можете отклонить это предложение";
                return RedirectToAction("MyExchanges");
            }

            if (exchange.Status != ExchangeStatus.Pending)
            {
                TempData["ErrorMessage"] = "Это предложение уже обработано";
                return RedirectToAction("MyExchanges");
            }

            exchange.Status = ExchangeStatus.Rejected;
            exchange.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Предложение обмена отклонено";
            return RedirectToAction("MyExchanges");
        }

        // GET: /Exchange/Details/{id} - детали обмена
        public async Task<IActionResult> Details(int id)
        {
            var exchange = await _context.Exchanges
                .Include(e => e.OfferedBook)
                .Include(e => e.RequestedBook)
                .Include(e => e.Initiator)
                .Include(e => e.TargetUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exchange == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (exchange.InitiatorId != currentUser.Id && exchange.TargetUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "У вас нет доступа к этому обмену";
                return RedirectToAction("MyExchanges");
            }

            return View(exchange);
        }
        // GET: /Exchange/Completed - завершенные обмены
        [Authorize]
        public async Task<IActionResult> Completed()
        {
            var user = await _userManager.GetUserAsync(User);

            var completedExchanges = await _context.Exchanges
                .Include(e => e.OfferedBook)
                .Include(e => e.RequestedBook)
                .Include(e => e.Initiator)
                .Include(e => e.TargetUser)
                .Where(e => (e.InitiatorId == user.Id || e.TargetUserId == user.Id) &&
                           e.Status == ExchangeStatus.Accepted)
                .OrderByDescending(e => e.UpdatedAt)
                .ToListAsync();

            // Проверяем для каких обменов уже оставлены отзывы
            var reviewedExchangeIds = await _context.Reviews
                .Where(r => r.AuthorId == user.Id && r.ExchangeId.HasValue)
                .Select(r => r.ExchangeId.Value)
                .ToListAsync();

            ViewBag.ReviewedExchangeIds = reviewedExchangeIds;

            return View(completedExchanges);
        }
    }
}