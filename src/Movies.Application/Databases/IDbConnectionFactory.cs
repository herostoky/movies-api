using System.Data;
using Npgsql;

namespace Movies.Application.Databases;
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken);
}

public class NpgSqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
