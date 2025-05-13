using System.Configuration;
using System.Data;
using System.Windows;

namespace JourneyDB_CW;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Запуск окна авторизации
        var logSign = new LogSign();
        bool? result = logSign.ShowDialog();

        if (result == true)
        {
            // Если вход успешный — запускаем главное окно
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        else
        {
            // Иначе закрываем приложение
            Shutdown();
        }
    }
}

