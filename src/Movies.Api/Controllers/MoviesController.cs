﻿using Microsoft.AspNetCore.Mvc;
using Movies.Api.Endpoints;
using Movies.Api.Mappings;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpPost(ApiEndpoint.Movies.Create)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await _movieRepository.CreateAsync(movie, cancellationToken);
        return CreatedAtAction(
            actionName: nameof(GetAsync),
            routeValues: new { id = movie.Id },
            value: movie.MapToMovieResponse());
    }

    [HttpGet(ApiEndpoint.Movies.Get)]
    public async Task<IActionResult> GetAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie is null)
        {
            return NotFound(id);
        }

        return Ok(movie.MapToMovieResponse());
    }

    [HttpGet(ApiEndpoint.Movies.GetAll)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.GetAllAsync(cancellationToken);

        var moviesResponse = movies.MapToMoviesResponse();
        return Ok(moviesResponse);
    }
}