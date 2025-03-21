using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        return _movieRepository.CreateAsync(movie, cancellationToken);
    }

    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _movieRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        return _movieRepository.GetBySlugAsync(slug, cancellationToken);
    }

    public Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _movieRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken)
    {
        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!movieExists)
        {
            return null;
        }

        await _movieRepository.UpdateAsync(movie, cancellationToken);
        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _movieRepository.DeleteByIdAsync(id, cancellationToken);
    }
}