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
        private string connectionString = "Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;";

        public LogSign()
        {
            InitializeComponent();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string email = EmailTextBox.Text;
                string password = PasswordBox.Password;
                DateTime? selectedBirthDate = BirthDatePicker.SelectedDate;
                DateTime birthDate = selectedBirthDate.Value;

                List<string> emptyFields = new List<string>();

                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                    emptyFields.Add("Имя");

                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                    emptyFields.Add("Фамилия");

                if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                    emptyFields.Add("Email");

                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                    emptyFields.Add("Пароль");

                if (BirthDatePicker.SelectedDate == null)
                    emptyFields.Add("Дата рождения");

                if (emptyFields.Count > 0)
                {
                    string message = "Заполните следующие поля:\n- " + string.Join("\n- ", emptyFields);
                    MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    RegisterUser(firstName, lastName, email, password, birthDate);
                }
            }
            catch
            (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            


        }

        private bool RegisterUser(string firstName, string lastName, string email, string password, DateTime birthDate)
        {

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Проверим, нет ли уже пользователя с таким email
                string checkQuery = "SELECT COUNT(*) FROM user WHERE email_user = @Email";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Пользователь с таким email уже существует.");
                        return false;
                    }
                }

                // Добавим нового пользователя
                string insertQuery = "INSERT INTO user (first_name_user, last_name_user, email_user, password_user, birth_date) VALUES (@FirstName, @LastName, @Email, @Password, @BirthDate)";
                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password); // или хеш
                    cmd.Parameters.AddWithValue("@birthDate", birthDate.ToString("yyyy-MM-dd"));

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

    }
}
