using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class BookAuthor
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int AuthorId { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }

        [ForeignKey("AuthorId")]
        public virtual Author Author { get; set; }
    }
}