using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken);
    Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken);
    Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken);
    Task<IEnumerable<Movie>> GetAllAsync(Guid? userId, CancellationToken cancellationToken);
    Task<Movie?> UpdateAsync(Movie movie, Guid? userId, CancellationToken cancellationToken);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
}