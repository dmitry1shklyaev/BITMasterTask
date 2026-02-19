using BITMasterTask.Models;
using System.Data;

namespace BITMasterTask.Data;

public class SqlDriverRepository : IDriverRepository
{
    private readonly IDbConnection _connection;

    public SqlDriverRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<Driver>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name, Surname, Patronymic FROM Driver";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        var drivers = new List<Driver>();

        using var reader = await Task.Run(() => command.ExecuteReader());
        while (reader.Read())
        {
            drivers.Add(new Driver
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Name = reader.GetString(1),
                Surname = reader.GetString(2),
                Patronymic = reader.GetString(3)
            });
        }

        return drivers;
    }

    public async Task<Driver?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Id, Name, Surname, Patronymic FROM Driver WHERE Id = @Id";

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

        return new Driver
        {
            Id = Convert.ToInt32(reader.GetValue(0)),
            Name = reader.GetString(1),
            Surname = reader.GetString(2),
            Patronymic = reader.GetString(3)
        };
    }

    public async Task<int> AddAsync(Driver driver)
    {
        const string sql = @"
    INSERT INTO Driver (Name, Surname, Patronymic)
    VALUES (@Name, @Surname, @Patronymic);
    SELECT last_insert_rowid();
";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var nameParam = command.CreateParameter();
        nameParam.ParameterName = "@Name";
        nameParam.Value = driver.Name;
        command.Parameters.Add(nameParam);

        var surnameParam = command.CreateParameter();
        surnameParam.ParameterName = "@Surname";
        surnameParam.Value = driver.Surname;
        command.Parameters.Add(surnameParam);

        var patronymicParam = command.CreateParameter();
        patronymicParam.ParameterName = "@Patronymic";
        patronymicParam.Value = driver.Patronymic;
        command.Parameters.Add(patronymicParam);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        var id = Convert.ToInt32(Convert.ToInt64(await Task.Run(() => command.ExecuteScalar())));
        driver.Id = id;
        return id;
    }

    public async Task UpdateAsync(Driver driver)
    {
        const string sql = "UPDATE Driver SET Name = @Name, Surname = @Surname, Patronymic = @Patronymic WHERE Id = @Id";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var idParam = command.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = driver.Id;
        command.Parameters.Add(idParam);

        var nameParam = command.CreateParameter();
        nameParam.ParameterName = "@Name";
        nameParam.Value = driver.Name;
        command.Parameters.Add(nameParam);

        var surnameParam = command.CreateParameter();
        surnameParam.ParameterName = "@Surname";
        surnameParam.Value = driver.Surname;
        command.Parameters.Add(surnameParam);

        var patronymicParam = command.CreateParameter();
        patronymicParam.ParameterName = "@Patronymic";
        patronymicParam.Value = driver.Patronymic;
        command.Parameters.Add(patronymicParam);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        await Task.Run(() => command.ExecuteNonQuery());
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Driver WHERE Id = @Id";

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

