using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 50 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна содержать от 2 до 50 символов")]
        public string LastName { get; set; }

        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [StringLength(100, ErrorMessage = "Название страны не должно превышать 100 символов")]
        public string Country { get; set; }

        // Вычисляемое поле для полного имени
        public string FullName => $"{FirstName} {LastName}";

        // Навигационное свойство для связи многие-ко-многим с книгами
        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
    }
}