using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = new();

    public Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        _movies.Add(movie);
        return Task.FromResult(true);
    }

    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = _movies.SingleOrDefault(m => m.Id.Equals(id));
        return Task.FromResult(movie);
    }

    public Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var movie = _movies.SingleOrDefault(m => m.Slug.Equals(slug));
        return Task.FromResult(movie);
    }

    public Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_movies.AsEnumerable());
    }

    public Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken)
    {
        var movieIndex = _movies.FindIndex(m => m.Id.Equals(movie.Id));
        if (movieIndex == -1)
        {
            return Task.FromResult(false);
        }

        _movies[movieIndex] = movie;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var removeCount = _movies.RemoveAll(m => m.Id.Equals(id));
        return Task.FromResult(removeCount > 0);
    }
}