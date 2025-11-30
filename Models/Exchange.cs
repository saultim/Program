using System.ComponentModel.DataAnnotations;

namespace TextbookExchange.Models
{
    public class Exchange
    {
        public int Id { get; set; }

        // Книга, которую предлагают (от инициатора обмена)
        public int OfferedBookId { get; set; }
        public Book OfferedBook { get; set; }

        // Книга, которую хотят получить (у целевого пользователя)
        public int RequestedBookId { get; set; }
        public Book RequestedBook { get; set; }

        // Участники обмена
        public string InitiatorId { get; set; } // Кто предложил обмен
        public User Initiator { get; set; }

        public string TargetUserId { get; set; } // Кому предложили обмен
        public User TargetUser { get; set; }

        // Статус обмена
        public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;

        // Сообщение от инициатора
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum ExchangeStatus
    {
        Pending,    // Ожидает ответа
        Accepted,   // Принят
        Rejected,   // Отклонен
        Completed   // Завершен
    }
}