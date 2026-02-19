using System.Windows;
using BITMasterTask.Models;

namespace BITMasterTask;

public partial class DriverEditWindow : Window
{
    public DriverEditWindow(Driver driver)
    {
        InitializeComponent();
        DataContext = driver;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}

