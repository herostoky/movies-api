using Microsoft.AspNetCore.Mvc;
using Movies.Application.Repositories;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieService;

    public MoviesController(IMovieRepository movieService)
    {
        _movieService = movieService;
    }


}