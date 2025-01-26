using System.ComponentModel.DataAnnotations;

namespace MovieAPIDemo.Models
{
    public class MovieListViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name of the movie is required")]
        public string Title { get; set; }
        

        //list of Actors
        public List<ActorViewModel> Actors { get; set; }

        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }
    }
}
