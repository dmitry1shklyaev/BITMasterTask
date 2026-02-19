using BITMasterTask.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLitePCL;
using System.Data;
using System.IO;
using System.Windows;

namespace BITMasterTask
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Batteries.Init();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            _host.Start();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var path = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "BITMasterTask",
    "database.db");

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);


            services.AddTransient<IDbConnection>(_ =>
            {
                var connection = new SqliteConnection($"Data Source={path}");
                connection.Open();

                using var pragma = connection.CreateCommand();
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQueryAsync();

                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Car (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Driver (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Surname TEXT NOT NULL,
            Name TEXT NOT NULL,
            Patronymic TEXT
        );

        CREATE TABLE IF NOT EXISTS Crew (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            CarId INTEGER NOT NULL,
            DriverId INTEGER NOT NULL,
            Active INTEGER NOT NULL DEFAULT 1 CHECK (Active IN (0,1)),
            FOREIGN KEY(CarId) REFERENCES Car(Id) ON DELETE CASCADE,
            FOREIGN KEY(DriverId) REFERENCES Driver(Id) ON DELETE CASCADE
        );
        ";

                cmd.ExecuteNonQuery();

                return connection;
            });

            services.AddTransient<ICarRepository, SqlCarRepository>();
            services.AddTransient<IDriverRepository, SqlDriverRepository>();
            services.AddTransient<ICrewRepository, SqlCrewRepository>();

            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
