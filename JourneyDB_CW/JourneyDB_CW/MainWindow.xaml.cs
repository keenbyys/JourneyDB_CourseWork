using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JourneyDB_CW;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadDataBase();
    }

    public void LoadDataBase()
    {
        try
        {
            DataBaseHelper connectionDataBase = new DataBaseHelper();
            DataGrid.ItemsSource = null;
            var table = connectionDataBase.SelectQuery("SELECT * FROM db_joint_journey.trips;");
            DataGrid.ItemsSource = table.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }
}