using System.ComponentModel.DataAnnotations;

namespace TextbookExchange.Models
{
    public class CreateExchangeViewModel
    {
        [Required(ErrorMessage = "Выберите книгу для обмена")]
        [Display(Name = "Ваша книга для обмена")]
        public int OfferedBookId { get; set; }

        [Required]
        public int RequestedBookId { get; set; }

        [Display(Name = "Сообщение владельцу")]
        [StringLength(500, ErrorMessage = "Сообщение не должно превышать 500 символов")]
        public string Message { get; set; }

        // Навигационные свойства
        public Book RequestedBook { get; set; }
        public List<Book> MyBooks { get; set; } = new List<Book>();
    }
}