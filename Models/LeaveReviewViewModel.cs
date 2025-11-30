using System.ComponentModel.DataAnnotations;

namespace TextbookExchange.Models
{
    public class LeaveReviewViewModel
    {
        [Required]
        public string TargetUserId { get; set; }

        public string TargetUserName { get; set; }

        public int? ExchangeId { get; set; }

        [Required(ErrorMessage = "Выберите оценку")]
        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
        [Display(Name = "Оценка")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Напишите отзыв")]
        [StringLength(1000, ErrorMessage = "Отзыв не должен превышать 1000 символов")]
        [Display(Name = "Текст отзыва")]
        public string Comment { get; set; }
    }

    public class CreateComplaintViewModel
    {
        [Required]
        public string TargetUserId { get; set; }

        public string TargetUserName { get; set; }

        public int? ExchangeId { get; set; }

        [Required(ErrorMessage = "Выберите тип жалобы")]
        [Display(Name = "Тип жалобы")]
        public ComplaintType Type { get; set; }

        [Required(ErrorMessage = "Опишите проблему")]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        [Display(Name = "Описание проблемы")]
        public string Description { get; set; }
    }
}