using Movies.Application.Models;

namespace Movies.Application.Repositories;

internal interface IMovieRepository
{
    Task<bool> CreateMovieAsync(Movie movie, CancellationToken cancellationToken);
    Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> UpdateMovieAsync(Movie movie, CancellationToken cancellationToken);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
}