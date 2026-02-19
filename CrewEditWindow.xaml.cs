using System.Collections.Generic;
using System.Windows;
using BITMasterTask.Models;

namespace BITMasterTask;

public partial class CrewEditWindow : Window
{
    public CrewEditViewModel ViewModel { get; }

    public CrewEditWindow(Crew crew, IList<Car> cars, IList<Driver> drivers)
    {
        InitializeComponent();
        ViewModel = new CrewEditViewModel(crew, cars, drivers);
        DataContext = ViewModel;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}

public class CrewEditViewModel
{
    public Crew Crew { get; }
    public IList<Car> Cars { get; }
    public IList<Driver> Drivers { get; }

    public CrewEditViewModel(Crew crew, IList<Car> cars, IList<Driver> drivers)
    {
        Crew = crew;
        Cars = cars;
        Drivers = drivers;
    }
}

