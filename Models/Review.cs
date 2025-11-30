using System.ComponentModel.DataAnnotations;

namespace TextbookExchange.Models
{
    public class Review
    {
        public int Id { get; set; }

        // Кто оставляет отзыв
        public string AuthorId { get; set; }
        public User Author { get; set; }

        // На кого отзыв
        public string TargetUserId { get; set; }
        public User TargetUser { get; set; }

        // Связанный обмен (опционально)
        public int? ExchangeId { get; set; }
        public Exchange Exchange { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Текст отзыва не должен превышать 1000 символов")]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsVisible { get; set; } = true;
    }

    public class Complaint
    {
        public int Id { get; set; }

        // Кто жалуется
        public string AuthorId { get; set; }
        public User Author { get; set; }

        // На кого жалоба
        public string TargetUserId { get; set; }
        public User TargetUser { get; set; }

        // Связанный обмен (опционально)
        public int? ExchangeId { get; set; }
        public Exchange Exchange { get; set; }

        [Required]
        public ComplaintType Type { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; }

        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }

        // Для администратора
        public string AdminNotes { get; set; }
    }

    public enum ComplaintType
    {
        [Display(Name = "Невыполнение обмена")]
        ExchangeNotCompleted,
        [Display(Name = "Некорректное поведение")]
        InappropriateBehavior,
        [Display(Name = "Несоответствие описанию")]
        ItemNotAsDescribed,
        [Display(Name = "Другое")]
        Other
    }

    public enum ComplaintStatus
    {
        [Display(Name = "На рассмотрении")]
        Pending,
        [Display(Name = "Рассмотрено")]
        Reviewed,
        [Display(Name = "Отклонено")]
        Rejected,
        [Display(Name = "Решено")]
        Resolved
    }
}