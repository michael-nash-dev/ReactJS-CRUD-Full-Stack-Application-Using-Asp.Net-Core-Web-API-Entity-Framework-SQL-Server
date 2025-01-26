using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Data;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;

namespace MovieAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly MovieDBContext _context;
        private readonly IMapper _mapper;

        public PersonController(MovieDBContext context, IMapper mapper)
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

                var actorCount = _context.Person.Count();
                var actorList =_mapper.Map<List<ActorViewModel>>( _context.Person.Skip(pageIndex * pageSize).Take(pageSize).ToList());

                response.status = true;
                response.message = "Success";
                response.Data = new { Movies = actorList, count = actorCount };

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
        public IActionResult GetPersonById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {


                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();

                if (person == null)
                {
                    response.status = false;
                    response.message = "Record does not Exist";

                    return BadRequest(response);
                }

                var personData = new ActorDetailsViewModel {
                    Id = person.Id,
                    Name = person.Name, 
                    DateOfBirth = person.DateOfBirth,
                    Movies = _context.Movie.Where(x =>x.Actors.Contains(person)).Select(x => x.Title).ToArray(),
                };

                response.status = true;
                response.message = "Success";
                response.Data = personData;

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

        [HttpGet]
        [Route("Search/{searchText}")]
        public IActionResult Get(string searchText)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var searchedPerson = _context.Person.Where(x =>x.Name.Contains(searchText)).Select(x =>new 
                {
                     x.Id,
                     x.Name,
                
                }).ToList();

                response.status = true;
                response.message = "Success";
                response.Data = searchedPerson;

                return Ok(response);

            }
            catch (Exception)
            {

                response.status = false;
                response.message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        public IActionResult Post(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                   

                    var postedModel = new Person()
                    {
                        Name = model.Name,
                        DateOfBirth = model.DateOfBirth
                       
                    };

                    _context.Person.Add(postedModel);
                    _context.SaveChanges();

                    model.Id = postedModel.Id;

                    response.status = true;
                    response.message = "Created Successfully";
                    response.Data = model;

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
        public IActionResult Put(ActorViewModel model) { 
          
            BaseResponseModel response = new BaseResponseModel();


            try
            {
                if (ModelState.IsValid)
                {
                    var postedModel = _mapper.Map<Person>(model);

                    if (model.Id <=0)
                    {
                        response.status = false;
                        response.message = "Invalid Person data.";
                       

                        return BadRequest(response);

                    }

                    var personDetails = _context.Person.Where(x => x.Id == model.Id).AsNoTracking().FirstOrDefault();
                   
                    if (personDetails == null)
                    {
                        response.status = false;
                        response.message = "Invalid Person data.";


                        return BadRequest(response);
                    }

                    _context.Person.Update(postedModel);
                    _context.SaveChanges();

                    response.status = true;
                    response.message = "Updated Successfully";
                    response.Data = postedModel;

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
            catch (Exception)
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
                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();

                if (person == null)
                {
                    response.status = false;
                    response.message = "Invalid Person Record";

                    return BadRequest(response);
                }

                _context.Person.Remove(person);
                _context.SaveChanges();

                response.status = true;
                response.message = "Deleted Successfully";

                return Ok(response);

            }
            catch (Exception ex)
            {

                response.status = false;
                response.message = "Something went wrong";

                return BadRequest(response);
            }
        }
    }
}
