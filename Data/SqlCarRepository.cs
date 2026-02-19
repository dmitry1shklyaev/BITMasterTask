using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BITMasterTask.Models;

namespace BITMasterTask.Data;

public class SqlCarRepository : ICarRepository
{
    private readonly IDbConnection _connection;

    public SqlCarRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<Car>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name FROM Car";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        var cars = new List<Car>();

        using var reader = await Task.Run(() => command.ExecuteReader());
        while (reader.Read())
        {
            cars.Add(new Car
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Name = reader.GetString(1)
            });
        }

        return cars;
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Id, Name FROM Car WHERE Id = @Id";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        using var reader = await Task.Run(() => command.ExecuteReader());
        if (!reader.Read())
        {
            return null;
        }

        return new Car
        {
            Id = Convert.ToInt32(reader.GetValue(0)),
            Name = reader.GetString(1)
        };
    }

    public async Task<int> AddAsync(Car car)
    {
        const string sql = @"
        INSERT INTO Car (Name)
        VALUES (@Name);
        SELECT last_insert_rowid();
    ";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var nameParam = command.CreateParameter();
        nameParam.ParameterName = "@Name";
        nameParam.Value = car.Name;
        command.Parameters.Add(nameParam);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        var id = Convert.ToInt32(Convert.ToInt64(await Task.Run(() => command.ExecuteScalar())));
        car.Id = id;
        return id;
    }

    public async Task UpdateAsync(Car car)
    {
        const string sql = "UPDATE Car SET Name = @Name WHERE Id = @Id";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var idParam = command.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = car.Id;
        command.Parameters.Add(idParam);

        var nameParam = command.CreateParameter();
        nameParam.ParameterName = "@Name";
        nameParam.Value = car.Name;
        command.Parameters.Add(nameParam);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        await Task.Run(() => command.ExecuteNonQuery());
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Car WHERE Id = @Id";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var idParam = command.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = id;
        command.Parameters.Add(idParam);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        await Task.Run(() => command.ExecuteNonQuery());
    }
}

