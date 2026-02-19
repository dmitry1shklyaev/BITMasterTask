using BITMasterTask.Models;
using System.Data;

namespace BITMasterTask.Data;

public class SqlCrewRepository : ICrewRepository
{
    private readonly IDbConnection _connection;

    public SqlCrewRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<Crew>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name, CarId, DriverId, Active FROM Crew";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        var crews = new List<Crew>();

        using var reader = await Task.Run(() => command.ExecuteReader());
        while (reader.Read())
        {
            crews.Add(new Crew
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Name = reader.GetString(1),
                CarId = Convert.ToInt32(reader.GetValue(2)),
                DriverId = Convert.ToInt32(reader.GetValue(3)),
                Active = Convert.ToInt32(reader.GetValue(4)) == 1
            });
        }

        return crews;
    }

    public async Task<int> AddAsync(Crew crew)
    {
        const string sql = @"
    INSERT INTO Crew (Name, CarId, DriverId, Active)
    VALUES (@Name, @CarId, @DriverId, @Active);
    SELECT last_insert_rowid();
";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var nameParam = command.CreateParameter();
        nameParam.ParameterName = "@Name";
        nameParam.Value = crew.Name;
        command.Parameters.Add(nameParam);

        var carParam = command.CreateParameter();
        carParam.ParameterName = "@CarId";
        carParam.Value = crew.CarId;
        command.Parameters.Add(carParam);

        var driverParam = command.CreateParameter();
        driverParam.ParameterName = "@DriverId";
        driverParam.Value = crew.DriverId;
        command.Parameters.Add(driverParam);

        var activeParam = command.CreateParameter();
        activeParam.ParameterName = "@Active";
        activeParam.Value = crew.Active;
        command.Parameters.Add(activeParam);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        var id = Convert.ToInt32(Convert.ToInt64(await Task.Run(() => command.ExecuteScalar())));
        crew.Id = id;
        return id;
    }

    public async Task UpdateAsync(Crew crew)
    {
        const string sql = @"UPDATE Crew
                             SET Name = @Name,
                                 CarId = @CarId,
                                 DriverId = @DriverId,
                                 Active = @Active
                             WHERE Id = @Id";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var idParam = command.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = crew.Id;
        command.Parameters.Add(idParam);

        var nameParam = command.CreateParameter();
        nameParam.ParameterName = "@Name";
        nameParam.Value = crew.Name;
        command.Parameters.Add(nameParam);

        var carParam = command.CreateParameter();
        carParam.ParameterName = "@CarId";
        carParam.Value = crew.CarId;
        command.Parameters.Add(carParam);

        var driverParam = command.CreateParameter();
        driverParam.ParameterName = "@DriverId";
        driverParam.Value = crew.DriverId;
        command.Parameters.Add(driverParam);

        var activeParam = command.CreateParameter();
        activeParam.ParameterName = "@Active";
        activeParam.Value = crew.Active;
        command.Parameters.Add(activeParam);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        await Task.Run(() => command.ExecuteNonQuery());
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Crew WHERE Id = @Id";

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

    public async Task DeleteInactiveAsync()
    {
        const string sql = "DELETE FROM Crew WHERE Active = 0";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        await Task.Run(() => command.ExecuteNonQuery());
    }
}

