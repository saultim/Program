using System.ComponentModel.DataAnnotations;

namespace TextbookExchange.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название учебника")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Автор обязателен")]
        [Display(Name = "Автор")]
        public string Author { get; set; }

        [Display(Name = "Предмет")]
        public string Subject { get; set; }

        [Display(Name = "Курс")]
        public string Course { get; set; }

        [Display(Name = "Состояние")]
        public string Condition { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}