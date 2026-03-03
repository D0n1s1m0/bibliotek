using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название жанра обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Название жанра должно содержать от 2 до 50 символов")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }

        // Навигационное свойство для связи многие-ко-многим с книгами
        public virtual ICollection<BookGenre> BookGenres { get; set; }
    }
}