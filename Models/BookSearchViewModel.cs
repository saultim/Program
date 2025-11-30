using System.ComponentModel.DataAnnotations;

namespace TextbookExchange.Models
{
    public class BookSearchViewModel
    {
        // Результаты поиска
        public List<Book> Books { get; set; } = new List<Book>();

        // Параметры поиска
        [Display(Name = "Поиск")]
        public string Search { get; set; }

        [Display(Name = "Предмет")]
        public string SelectedSubject { get; set; }

        [Display(Name = "Курс")]
        public string SelectedCourse { get; set; }

        [Display(Name = "Состояние")]
        public string SelectedCondition { get; set; }

        // УБРАНО: public string SelectedType { get; set; }

        // Списки для выпадающих меню
        public List<string> Subjects { get; set; } = new List<string>();
        public List<string> Courses { get; set; } = new List<string>();
        public List<string> Conditions { get; set; } = new List<string>();

        // Статистика
        public int TotalCount => Books?.Count ?? 0;

        // Проверка есть ли активные фильтры
        public bool HasActiveFilters =>
            !string.IsNullOrEmpty(Search) ||
            (!string.IsNullOrEmpty(SelectedSubject) && SelectedSubject != "all") ||
            (!string.IsNullOrEmpty(SelectedCourse) && SelectedCourse != "all") ||
            (!string.IsNullOrEmpty(SelectedCondition) && SelectedCondition != "all");
    }
}