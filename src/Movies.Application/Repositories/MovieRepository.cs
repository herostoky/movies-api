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

        var movieResult = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  INSERT INTO movies (id, slug, title, year_of_release)
                                  VALUES (@Id, @Slug, @Title, @YearOfRelease);
                                  """
                , movie, cancellationToken: cancellationToken));

        if (movieResult.Equals(1))
        {
            foreach (var movieGenre in movie.Genres)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition("""
                                           INSERT INTO genres (fk_movie_id, name)
                                           VALUES (@MovieId, @Name);
                                           """
                        , new { MovieId = movie.Id, Name = movieGenre }
                        , cancellationToken: cancellationToken)
                    );
            }
        }

        transaction.Commit();

        return movieResult.Equals(1);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                   SELECT m.id AS Id, m.slug AS Slug, m.title AS Title, m.year_of_release AS YearOfRelease,
                                   ROUND(AVG(r.rating), 1) AS Rating,
                                   ROUND(my_r.rating, 1) AS UserRating
                                   FROM movies m
                                   LEFT JOIN ratings r
                                        ON r.fk_movie_id = m.id
                                   LEFT JOIN ratings my_r
                                        ON my_r.fk_movie_id = m.id 
                                        AND my_r.fictive_user_id = @UserId 
                                   WHERE m.id = @Id
                                   GROUP BY m.id, my_r.rating;
                                   """
                , new { Id = id, UserId = userId }
                , cancellationToken: cancellationToken)
            );

        if (movie is null)
        {
            return movie;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                   SELECT name
                                   FROM genres
                                   WHERE fk_movie_id = @MovieId;
                                   """
                , new { MovieId = movie.Id }
                , cancellationToken: cancellationToken)
            );
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                   SELECT m.id AS Id, m.slug AS Slug, m.title AS Title, m.year_of_release AS YearOfRelease,
                                   ROUND(AVG(r.rating), 1) AS Rating,
                                   ROUND(my_r.rating, 1) AS UserRating
                                   FROM movies m
                                   LEFT JOIN ratings r
                                        ON r.fk_movie_id = m.id
                                   LEFT JOIN ratings my_r
                                        ON my_r.fk_movie_id = m.id 
                                        AND my_r.fictive_user_id = @UserId 
                                   WHERE m.slug = @Slug
                                   GROUP BY m.id, my_r.rating;
                                   """
                , new { Slug = slug, UserId = userId }
                , cancellationToken: cancellationToken)
            );

        if (movie is null)
        {
            return movie;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                   SELECT name
                                   FROM genres
                                   WHERE fk_movie_id = @MovieId;
                                   """
                , new { MovieId = movie.Id }
                , cancellationToken: cancellationToken)
            );
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movies = await connection.QueryAsync(
            new CommandDefinition("""
                                  SELECT m.id AS Id, m.slug AS Slug, m.title AS Title, m.year_of_release AS YearOfRelease,
                                           string_agg(DISTINCT g.name, ', ') As Genres
                                  FROM movies m
                                  LEFT JOIN genres g
                                       ON g.fk_movie_id = m.id
                                  GROUP BY m.id
                                  """, cancellationToken: cancellationToken));

        return movies.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(", "))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, Guid? userId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        // Delete genres
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE
                FROM genres
                WHERE fk_movie_id = @Id
                """,
                new { movie.Id },
                cancellationToken: cancellationToken
            )
        );

        // Insert genres
        foreach (var movieGenre in movie.Genres)
        {
            await connection.ExecuteAsync(
                new CommandDefinition("""
                                       INSERT INTO genres (fk_movie_id, name)
                                       VALUES (@MovieId, @Name);
                                       """
                    , new { MovieId = movie.Id, Name = movieGenre }
                    , cancellationToken: cancellationToken)
                );
        }

        // Update movie
        var result = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE movies
                SET slug = @Slug, title = @Title, year_of_release = @YearOfRelease
                WHERE id = @Id
                """,
                movie,
                cancellationToken: cancellationToken
            )
        );

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        // Delete genres
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE
                FROM genres
                WHERE fk_movie_id = @id
                """,
                new { id },
                cancellationToken: cancellationToken
            )
        );

        // Delete movie
        var result = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                DELETE
                FROM movies
                WHERE id = @id
                """,
                new { id },
                cancellationToken: cancellationToken
            )
        );

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT COUNT(1)
                FROM movies
                WHERE id = @id
                """,
                new { id },
                cancellationToken: cancellationToken
            )
        );
    }
}