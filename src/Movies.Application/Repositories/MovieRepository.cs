using Dapper;
using Movies.Application.Databases;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var movieResult = await connection.ExecuteAsync("""
                                                        INSERT INTO movies (id, slug, title, year_of_release)
                                                        VALUES (@Id, @Slug, @Title, @YearOfRelease);
                                                        """, movie);

        if (movieResult.Equals(1))
        {
            foreach (var movieGenre in movie.Genres)
            {
                await connection.ExecuteAsync("""
                                              INSERT INTO genres (fk_movie_id, name)
                                              VALUES (@MovieId, @Name);
                                              """, new { MovieId = movie.Id, Name = movieGenre });
            }
        }

        transaction.Commit();

        return movieResult.Equals(1);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>("""
                                                                     SELECT id AS Id, slug AS Slug, title AS Title, year_of_release AS YearOfRelease
                                                                     FROM movies
                                                                     WHERE id = @Id;
                                                                     """, new { Id = id });

        if (movie is null)
        {
            return movie;
        }

        var genres = await connection.QueryAsync<string>("""
                                                         SELECT name
                                                         FROM genres
                                                         WHERE fk_movie_id = @MovieId;
                                                         """, new { MovieId = movie.Id });
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>("""
                                                                      SELECT id AS Id, slug AS Slug, title AS Title, year_of_release AS YearOfRelease
                                                                      FROM movies
                                                                      WHERE slug = @Slug;
                                                                      """, new { Slug = slug });

        if (movie is null)
        {
            return movie;
        }

        var genres = await connection.QueryAsync<string>("""
                                                         SELECT name
                                                         FROM genres
                                                         WHERE fk_movie_id = @MovieId;
                                                         """, new { MovieId = movie.Id });
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movies = await connection.QueryAsync("""
                                                 SELECT m.id AS Id, m.slug AS Slug, m.title AS Title, m.year_of_release AS YearOfRelease,
                                                          string_agg(g.name, ', ') As Genres
                                                 FROM movies m
                                                 LEFT JOIN genres g
                                                      ON g.fk_movie_id = m.id
                                                 GROUP BY m.id
                                                 """);

        return movies.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(", "))
        });
    }

    public Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        //var movieIndex = _movies.FindIndex(m => m.Id.Equals(movie.Id));
        //if (movieIndex == -1)
        //{
        //    return Task.FromResult(false);
        //}

        //_movies[movieIndex] = movie;
        //return Task.FromResult(true);
    }

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        //var removeCount = _movies.RemoveAll(m => m.Id.Equals(id));
        //return Task.FromResult(removeCount > 0);
    }

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}