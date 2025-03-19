using Dapper;

namespace Movies.Application.Databases;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS movies (
                                          id UUID PRIMARY KEY,
                                          slug TEXT NOT NULL,
                                          title TEXT NOT NULL,
                                          year_of_release integer NOT NULL
                                      );
                                      """);

        await connection.ExecuteAsync("""
                                      CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS idx_movies_slug 
                                      ON movies 
                                      USING btree(slug);
                                      """);

        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS genres (
                                          fk_movie_id UUID REFERENCES movies(id),
                                          name TEXT NOT NULL
                                      );
                                      """);
    }
}

