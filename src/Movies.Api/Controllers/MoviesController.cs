using Microsoft.AspNetCore.Mvc;
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
            routeValues: new { idOrSlug = movie.Id },
            value: movie.MapToMovieResponse());
    }

    [HttpGet(ApiEndpoint.Movies.Get)]
    public async Task<IActionResult> GetAsync(
        [FromRoute] string idOrSlug,
        CancellationToken cancellationToken)
    {
        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieRepository.GetByIdAsync(id, cancellationToken)
            : await _movieRepository.GetBySlugAsync(idOrSlug, cancellationToken);

        if (movie is null)
        {
            return NotFound(new
            {
                idOrSlug
            });
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

    [HttpPut(ApiEndpoint.Movies.Update)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie(id);
        var isUpdated = await _movieRepository.UpdateAsync(movie, cancellationToken);
        if (!isUpdated)
        {
            return NotFound(new
            {
                id
            });
        }
        return Ok(movie.MapToMovieResponse());
    }

    [HttpDelete(ApiEndpoint.Movies.Delete)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var isDeleted = await _movieRepository.DeleteByIdAsync(id, cancellationToken);
        if (!isDeleted)
        {
            return NotFound(new
            {
                id
            });
        }

        return Ok(new
        {
            result = isDeleted
        }
        );
    }
}