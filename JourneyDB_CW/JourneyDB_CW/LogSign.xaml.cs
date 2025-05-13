using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JourneyDB_CW
{
    /// <summary>
    /// Interaction logic for LogSign.xaml
    /// </summary>
    public partial class LogSign : Window
    {
        public int UserId { get; private set; }
        private string connectionString = "Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;";

        public LogSign()
        {
            InitializeComponent();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            DateTime birthDate = BirthDatePicker.SelectedDate.Value;

            RegisterUser(firstName, lastName, email, password, birthDate);
        }

        private bool ValidateInputs(out string errorMessage)
        {
            List<string> emptyFields = new List<string>();

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                emptyFields.Add("Имя");

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                emptyFields.Add("Фамилия");

            if (EmailTextBox.Text == "@gmail.com")
                emptyFields.Add("Email");

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                emptyFields.Add("Пароль");

            if (BirthDatePicker.SelectedDate == null)
                emptyFields.Add("Дата рождения");

            if (emptyFields.Count > 0)
            {
                errorMessage = "Заполните следующие поля:\n- " + string.Join("\n- ", emptyFields);
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool IsEmailUnique(string email)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM user WHERE email_user = @Email";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        long count = (long)cmd.ExecuteScalar();
                        return count == 0; // Если count == 0, значит email уникален
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private void RegisterUser(string firstName, string lastName, string email, string password, DateTime birthDate)
        {
            if (!IsEmailUnique(email))
            {
                MessageBox.Show("Пользователь с таким email уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO user 
                             (first_name_user, last_name_user, email_user, password_user, birth_date) 
                             VALUES (@firstName, @lastName, @email, @password, @birthDate)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@birthDate", birthDate.ToString("yyyy-MM-dd"));

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    MessageBox.Show("Пользователь с таким email уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Ошибка базы данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool isUpdating = false;

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdating) return;
            isUpdating = true;

            string suffix = "@gmail.com";
            TextBox textBox = (TextBox)sender;
            string input = textBox.Text;

            // Если пользователь удалил @gmail.com — вернём обратно
            if (!input.EndsWith(suffix))
            {
                // Удалим всё, что после "@" (если пользователь случайно ввёл свою почту)
                int atIndex = input.IndexOf("@");
                if (atIndex != -1)
                    input = input.Substring(0, atIndex);

                textBox.Text = input + suffix;
                textBox.CaretIndex = input.Length;
            }

            isUpdating = false;
        }

        // LOG IN ====================================================================================================================================

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            string email = LogInEmailTextBox.Text;
            string password = LogInPasswordBox.Password;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id_user FROM user WHERE email_user = @Email AND password_user = @Password";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        int userId = Convert.ToInt32(result);

                        // создаём главное окно
                        MainWindow mainWindow = new MainWindow(userId);

                        // делаем его главным окном приложения
                        Application.Current.MainWindow = mainWindow;
                        mainWindow.Show();

                        // закрываем окно логина
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный email или пароль", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}
