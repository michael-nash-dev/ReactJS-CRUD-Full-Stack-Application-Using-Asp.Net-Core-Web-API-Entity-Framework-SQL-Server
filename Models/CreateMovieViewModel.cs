using MovieAPIDemo.Entities;
using System.ComponentModel.DataAnnotations;

namespace MovieAPIDemo.Models
{
    public class CreateMovieViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Name of the movie is required")]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        //list of Actors
        [Required]
        public List<int> Actors { get; set; }
        [Required]
        public string Language { get; set; }
        [Required]
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }
    }
}
