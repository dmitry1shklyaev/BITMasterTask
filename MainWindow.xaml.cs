using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BITMasterTask.Data;
using BITMasterTask.Models;

namespace BITMasterTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ICarRepository _carRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly ICrewRepository _crewRepository;

        private readonly DispatcherTimer _countdownTimer;
        private bool _cleanupRunning;

        // Для каждого неактивного экипажа храним момент, когда он стал неактивным
        private readonly Dictionary<int, DateTimeOffset> _crewDeactivatedAt = new();

        public ObservableCollection<Car> Cars { get; } = new();
        public ObservableCollection<Driver> Drivers { get; } = new();
        public ObservableCollection<CrewListItem> Crews { get; } = new();

        public MainWindow(
            ICarRepository carRepository,
            IDriverRepository driverRepository,
            ICrewRepository crewRepository)
        {
            _carRepository = carRepository;
            _driverRepository = driverRepository;
            _crewRepository = crewRepository;

            InitializeComponent();
            DataContext = this;

            Loaded += MainWindow_Loaded;

            // Таймер обновления обратного отсчёта и фонового удаления (1 раз в секунду)
            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += CountdownTimer_Tick;
            _countdownTimer.Start();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAllAsync();
        }

        private async Task ReloadAllAsync()
        {
            await ReloadCarsAsync();
            await ReloadDriversAsync();
            await ReloadCrewsAsync();
        }

        private async Task ReloadCarsAsync()
        {
            Cars.Clear();
            var cars = await _carRepository.GetAllAsync();
            foreach (var car in cars)
            {
                Cars.Add(car);
            }
        }

        private async Task ReloadDriversAsync()
        {
            Drivers.Clear();
            var drivers = await _driverRepository.GetAllAsync();
            foreach (var driver in drivers)
            {
                Drivers.Add(driver);
            }
        }

        private async Task ReloadCrewsAsync()
        {
            Crews.Clear();

            var crews = await _crewRepository.GetAllAsync();
            var cars = Cars.ToDictionary(c => c.Id, c => c.Name);
            var drivers = Drivers.ToDictionary(d => d.Id, d => d.FullName);

            foreach (var crew in crews)
            {
                cars.TryGetValue(crew.CarId, out var carName);
                drivers.TryGetValue(crew.DriverId, out var driverName);

                SyncDeactivationState(crew);

                Crews.Add(new CrewListItem
                {
                    Id = crew.Id,
                    Name = crew.Name,
                    CarId = crew.CarId,
                    DriverId = crew.DriverId,
                    CarName = carName ?? $"Id={crew.CarId}",
                    DriverName = driverName ?? $"Id={crew.DriverId}",
                    Active = crew.Active
                });
            }

            UpdateCrewCountdowns();
        }

        private void SyncDeactivationState(Crew crew)
        {
            if (crew.Active)
            {
                // Если снова активен — сбрасываем таймер
                _crewDeactivatedAt.Remove(crew.Id);
                return;
            }

            // Если неактивен и отметки ещё нет — значит "стал неактивным" прямо сейчас (для UI-отсчёта)
            if (!_crewDeactivatedAt.ContainsKey(crew.Id))
            {
                _crewDeactivatedAt[crew.Id] = DateTimeOffset.Now;
            }
        }

        private async void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            UpdateCrewCountdowns();
            await CleanupDueCrewsAsync();
        }

        private async Task CleanupDueCrewsAsync()
        {
            if (_cleanupRunning)
            {
                return;
            }

            var now = DateTimeOffset.Now;
            var dueIds = _crewDeactivatedAt
                .Where(kvp => (now - kvp.Value).TotalSeconds >= 30)
                .Select(kvp => kvp.Key)
                .ToList();

            if (dueIds.Count == 0)
            {
                return;
            }

            _cleanupRunning = true;
            try
            {
                foreach (var id in dueIds)
                {
                    await _crewRepository.DeleteAsync(id);
                    _crewDeactivatedAt.Remove(id);
                }

                await ReloadCrewsAsync();
            }
            catch
            {
                // Логирование опускаем в рамках тестового задания
            }
            finally
            {
                _cleanupRunning = false;
            }
        }

        private void UpdateCrewCountdowns()
        {
            var now = DateTimeOffset.Now;

            foreach (var crew in Crews)
            {
                if (crew.Active)
                {
                    crew.AutoDeleteIn = "—";
                    continue;
                }

                if (!_crewDeactivatedAt.TryGetValue(crew.Id, out var deactivatedAt))
                {
                    deactivatedAt = now;
                    _crewDeactivatedAt[crew.Id] = deactivatedAt;
                }

                var secondsLeft = 30 - (int)Math.Floor((now - deactivatedAt).TotalSeconds);
                if (secondsLeft < 0)
                {
                    secondsLeft = 0;
                }

                crew.AutoDeleteIn = secondsLeft.ToString();
            }
        }

        #region Машины

        private async void AddCar_Click(object sender, RoutedEventArgs e)
        {
            var car = new Car();
            var window = new CarEditWindow(car) { Owner = this };
            if (window.ShowDialog() == true)
            {
                await _carRepository.AddAsync(car);
                Cars.Add(car);
                await ReloadCrewsAsync();
            }
        }

        private async void EditCar_Click(object sender, RoutedEventArgs e)
        {
            if (CarsGrid.SelectedItem is not Car selected)
            {
                MessageBox.Show("Выберите машину для редактирования.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var carCopy = new Car
            {
                Id = selected.Id,
                Name = selected.Name
            };

            var window = new CarEditWindow(carCopy) { Owner = this };
            if (window.ShowDialog() == true)
            {
                selected.Name = carCopy.Name;
                await _carRepository.UpdateAsync(selected);
                await ReloadCrewsAsync();
            }
        }

        private async void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (CarsGrid.SelectedItem is not Car selected)
            {
                MessageBox.Show("Выберите машину для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Удалить выбранную машину?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            await _carRepository.DeleteAsync(selected.Id);
            Cars.Remove(selected);
            await ReloadCrewsAsync();
        }

        #endregion

        #region Водители

        private async void AddDriver_Click(object sender, RoutedEventArgs e)
        {
            var driver = new Driver();
            var window = new DriverEditWindow(driver) { Owner = this };
            if (window.ShowDialog() == true)
            {
                await _driverRepository.AddAsync(driver);
                Drivers.Add(driver);
                await ReloadCrewsAsync();
            }
        }

        private async void EditDriver_Click(object sender, RoutedEventArgs e)
        {
            if (DriversGrid.SelectedItem is not Driver selected)
            {
                MessageBox.Show("Выберите водителя для редактирования.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var driverCopy = new Driver
            {
                Id = selected.Id,
                Surname = selected.Surname,
                Name = selected.Name,
                Patronymic = selected.Patronymic
            };

            var window = new DriverEditWindow(driverCopy) { Owner = this };
            if (window.ShowDialog() == true)
            {
                selected.Surname = driverCopy.Surname;
                selected.Name = driverCopy.Name;
                selected.Patronymic = driverCopy.Patronymic;
                await _driverRepository.UpdateAsync(selected);
                await ReloadCrewsAsync();
            }
        }

        private async void DeleteDriver_Click(object sender, RoutedEventArgs e)
        {
            if (DriversGrid.SelectedItem is not Driver selected)
            {
                MessageBox.Show("Выберите водителя для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Удалить выбранного водителя?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            await _driverRepository.DeleteAsync(selected.Id);
            Drivers.Remove(selected);
            await ReloadCrewsAsync();
        }

        #endregion

        #region Экипажи

        private async void AddCrew_Click(object sender, RoutedEventArgs e)
        {
            var crew = new Crew
            {
                Active = true
            };

            var window = new CrewEditWindow(crew, Cars.ToList(), Drivers.ToList())
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                await _crewRepository.AddAsync(crew);
                await ReloadCrewsAsync();
            }
        }

        private async void EditCrew_Click(object sender, RoutedEventArgs e)
        {
            if (CrewsGrid.SelectedItem is not CrewListItem selected)
            {
                MessageBox.Show("Выберите экипаж для редактирования.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var crew = new Crew
            {
                Id = selected.Id,
                Name = selected.Name,
                CarId = selected.CarId,
                DriverId = selected.DriverId,
                Active = selected.Active
            };

            var window = new CrewEditWindow(crew, Cars.ToList(), Drivers.ToList())
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                await _crewRepository.UpdateAsync(crew);
                await ReloadCrewsAsync();
            }
        }

        private async void DeleteCrew_Click(object sender, RoutedEventArgs e)
        {
            if (CrewsGrid.SelectedItem is not CrewListItem selected)
            {
                MessageBox.Show("Выберите экипаж для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Удалить выбранный экипаж?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            await _crewRepository.DeleteAsync(selected.Id);
            await ReloadCrewsAsync();
        }

        #endregion
    }

    public class CrewListItem
        : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CarId { get; set; }
        public int DriverId { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;

        private bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value) return;
                _active = value;
                OnPropertyChanged();
            }
        }

        private string _autoDeleteIn = "—";
        public string AutoDeleteIn
        {
            get => _autoDeleteIn;
            set
            {
                if (_autoDeleteIn == value) return;
                _autoDeleteIn = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}