using System.Windows;
using BITMasterTask.Models;

namespace BITMasterTask;

public partial class CarEditWindow : Window
{
    public CarEditWindow(Car car)
    {
        InitializeComponent();
        DataContext = car;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}

