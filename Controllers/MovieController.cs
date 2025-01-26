using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Data;
using MovieAPIDemo.Models;
using MovieAPIDemo.Entities;
using AutoMapper;
using System.Net.Http.Headers;

namespace MovieAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDBContext _context;
        private readonly IMapper _mapper;

        public MovieController(MovieDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {

                var movieCount = _context.Movie.Count();
                var movieList = _mapper.Map<List<MovieListViewModel>>( _context.Movie.Include(x => x.Actors).Skip(pageIndex * pageSize)).Take(pageSize).ToList();

                response.status = true;
                response.message = "Success";
                response.Data = new { Movies = movieList, count = movieCount };

                return Ok(response);
            }

            catch (Exception ex)
            {
                //TODO: logging exceptions

                response.status = false;
                response.message = "Something went wrong";

                return BadRequest(response);

            }

        }

        [HttpGet("{id}")]
        public IActionResult GetMovieById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {


                var movie = _context.Movie.Include(x => x.Actors).Where(x => x.Id == id).FirstOrDefault();
                   

                if (movie == null)
                {
                    response.status = false;
                    response.message = "Record does not Exist";

                    return BadRequest(response);
                }

                var movieData = _mapper.Map<MovieDetailsViewModel>(movie);

                response.status = true;
                response.message = "Success";
                response.Data = movieData;

                return Ok(response);
            }

            catch (Exception ex)
            {
                //TODO: logging exceptions

                response.status = false;
                response.message = "Something went wrong";

                return BadRequest(response);

            }

        }

        [HttpPost]
        public IActionResult Post(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();


                    if (actors.Count != model.Actors.Count)
                    {

                        response.status = false;
                        response.message = "Invalid Actor Assigned";

                        return BadRequest(response);

                    }

                    var postedModel = _mapper.Map<Movie>(model);
                    postedModel.Actors = actors;

                    _context.Movie.Add(postedModel);
                    _context.SaveChanges();

                    var responseData = _mapper.Map<MovieDetailsViewModel>(postedModel);
                    response.status = true;
                    response.message = "Created Successfully";
                    response.Data = responseData;   
                    
                    return Ok(response);
                }

                else
                {
                    response.status = false;
                    response.message = "Validation failed";
                    response.Data = ModelState;

                   return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong";

                return BadRequest(response);
                
            }
        }


        [HttpPut]
        public IActionResult Put(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if(model.Id <=0){
                    response.status = false;
                    response.message = "Invalid Movie Record";

                    return BadRequest(response);
                }

                if (ModelState.IsValid)
                {
                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();


                    if (actors.Count != model.Actors.Count)
                    {

                        response.status = false;
                        response.message = "Invalid Actor Assigned";

                        return BadRequest(response);

                    }

                    var movieDetails = _context.Movie.Include(x => x.Actors).Where(x => x.Id == model.Id).FirstOrDefault();

                    if (movieDetails == null)
                    {
                        response.status = false;
                        response.message = "Invalid Movie Record";

                        return BadRequest(response);
                    }

                    movieDetails.CoverImage = model.CoverImage;
                    movieDetails.Title = model.Title;
                    movieDetails.Description = model.Description;
                    movieDetails.Language = model.Language;
                    movieDetails.ReleaseDate = model.ReleaseDate;

                    //Find removed actor

                    var removedActors = movieDetails.Actors.Where(x => !model.Actors.Contains(x.Id)).ToList();

                    foreach (var actor in removedActors)
                    {
                        movieDetails.Actors.Remove(actor);

                    }

                    //find added actor

                    var addedActors = actors.Except(movieDetails.Actors).ToList();
                    foreach (var actor in addedActors)
                    {
                        movieDetails.Actors.Add(actor);

                    }

                    _context.SaveChanges();

                    var responseData = new MovieDetailsViewModel
                    {
                        Id = movieDetails.Id,
                        Title = movieDetails.Title,
                        Actors = movieDetails.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth,

                        }).ToList(),

                        CoverImage = movieDetails.CoverImage,
                        Language = movieDetails.Language,
                        ReleaseDate = movieDetails.ReleaseDate,
                        Description = movieDetails.Description,

                    };



                    response.status = true;
                    response.message = "Updated Successfully";
                    response.Data = responseData;

                    return Ok(response);
                }

                else
                {
                    response.status = false;
                    response.message = "Validation failed";
                    response.Data = ModelState;

                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong";

                return BadRequest(response);

            }
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _context.Movie.Where(x => x.Id  == id).FirstOrDefault();

                if (movie == null)
                {
                    response.status = false;
                    response.message = "Invalid Movie Record";

                    return BadRequest(response);
                }

                _context.Movie.Remove(movie);
                _context.SaveChanges();

                response.status = true;
                response.message = "Deleted Successfully";

                return Ok(response);

            }
            catch (Exception ex)
            {

                response.status= false;
                response.message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("upload-movie-poster")]
        public async Task<IActionResult> UploadMoviePoster(IFormFile imageFile) {

            try
            {
                var filename = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');
                string newPath = @"C:\to-delete";

                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);

                }

                string[] allowedImageExtensions = new string[] { ".jpg", ".jpeg", ".png",".svg" };

                if (!allowedImageExtensions.Contains(Path.GetExtension(filename)))
                {
                    return BadRequest(new BaseResponseModel { 
                        status= false,
                        message="Only .jpg, .jpeg and png type files are Allowed."
                    
                    });

                }

                string newFileName = Guid.NewGuid() + Path.GetExtension(filename);
                string fullFilePath = Path.Combine(newPath, newFileName);

                using(var stream=new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return Ok(new { ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles/{newFileName}" });
                
            }
            catch (Exception ex)
            {

                return BadRequest(new BaseResponseModel
                {
                    status = false,
                    message = "An error occured."

                });
            }

        }

        [HttpGet]
        [Route("Search/{searchText}")]
        public IActionResult Get(string searchText)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var searchedMovie = _context.Movie.Where(x => x.Title.Contains(searchText)).Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.ReleaseDate,
                    x.Description,
                    x.CoverImage,
                    x.Actors

                }).ToList();

                response.status = true;
                response.message = "Success";
                response.Data = searchedMovie;

                return Ok(response);

            }
            catch (Exception)
            {

                response.status = false;
                response.message = "Something went wrong";

                return BadRequest(response);
            }
        }

    }
	
}
