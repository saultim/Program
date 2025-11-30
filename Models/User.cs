using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TextbookExchange.Models
{
    public class User : IdentityUser
    {
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Университет")]
        public string University { get; set; }

        [Display(Name = "Курс")]
        public int? Course { get; set; }

        // Навигационное свойство
        public List<Book> Books { get; set; } = new List<Book>();

        // Вычисляемое свойство для полного имени
        [Display(Name = "Полное имя")]
        public string FullName => $"{FirstName} {LastName}";
    }
}