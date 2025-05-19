using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace JourneyDB_CW;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string connectionString = "Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;";
    DataBaseProcedures _dbService = new DataBaseProcedures("Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;");
    private readonly int currentUserId;
    private DispatcherTimer checkUserTimer;


    public MainWindow(int userId)
    {
        InitializeComponent();
        LoadDataBase();

        currentUserId = userId;
        LoadUserData();
        LoadUserBookings();
        LoadUserReviews();

        LoadUniqueValues("SELECT DISTINCT topic FROM trips ORDER BY topic ASC", TopicComboBox, "topic");
        LoadUniqueValues("SELECT DISTINCT country FROM destination ORDER BY country ASC", CountryComboBox, "country");
        LoadUniqueValues("SELECT DISTINCT price FROM trips ORDER BY price ASC", PriceComboBox, "price");
        LoadUniqueValues("SELECT DISTINCT type FROM transport ORDER BY type ASC", TypeTransportComboBox, "type");
        
        FilterData();
        StartUserCheckTimer();
    }

    public void TotalLoadData()
    {
        LoadDataBase();
        LoadUserData();
        LoadUserBookings();
        LoadUserReviews();
        LoadUniqueValues("SELECT DISTINCT topic FROM trips ORDER BY topic ASC", TopicComboBox, "topic");
        LoadUniqueValues("SELECT DISTINCT country FROM destination ORDER BY country ASC", CountryComboBox, "country");
        LoadUniqueValues("SELECT DISTINCT price FROM trips ORDER BY price ASC", PriceComboBox, "price");
        LoadUniqueValues("SELECT DISTINCT type FROM transport ORDER BY type ASC", TypeTransportComboBox, "type");
        FilterData();
    }

    private void StartUserCheckTimer()
    {
        checkUserTimer = new DispatcherTimer();
        checkUserTimer.Interval = TimeSpan.FromSeconds(5);
        checkUserTimer.Tick += CheckIfUserStillExists;
        checkUserTimer.Start();
    }

    private void CheckIfUserStillExists(object sender, EventArgs e)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM user WHERE id_user = @id_user";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id_user", currentUserId);

                long count = (long)cmd.ExecuteScalar();

                if (count == 0)
                {
                    checkUserTimer?.Stop();

                    MessageBox.Show("Your account has been deleted. The program is shutting down.",
                        "Deleting a user", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("User validation error: " + ex.Message);
            }
        }
    }

    public void LoadDataBase()
    {
        try
        {
            DataTable dataTable = _dbService.ExecuteStoredProcedure("GetTripDetails");
            DataGrid.ItemsSource = dataTable.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadUserData()
    {
        string query = @"SELECT first_name_user, last_name_user, email_user, birth_date FROM user WHERE id_user = @UserId";
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        FirstName.Text = reader.GetString("first_name_user");
                        LastName.Text = reader.GetString("last_name_user");
                        Email.Text = reader.GetString("email_user");
                        DateBirth.Text = reader.GetDateTime("birth_date").ToShortDateString();
                    }
                }
            }
        }
    }

    private void LoadUserBookings()
    {
        try
        {
            DataTable bookingsTable = _dbService.ExecuteStoredProcedure(
                "GetUserBookings",
                new MySqlParameter("userId", currentUserId)
            );
            DataGrid_Booking.ItemsSource = bookingsTable.DefaultView;

            DataGrid_Booking.AutoGeneratingColumn += (s, e) =>
            {
                if (e.PropertyName == "Trip ID")
                {
                    e.Column.Visibility = Visibility.Collapsed;
                }
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadUserReviews()
    {
        try
        {
            DataTable reviewTable = _dbService.ExecuteStoredProcedure(
                "GetUserReviews",
                new MySqlParameter("userId", currentUserId)
            );
            DataGrid_Review.ItemsSource = reviewTable.DefaultView;
        }
        catch (MySqlException ex)
        {
            MessageBox.Show($"Database connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string GetRandomStatus()
    {
        Random rnd = new Random();
        int randomNum = rnd.Next(1, 3);

        return randomNum switch
        {
            1 => "CONFIRMED",
            2 => "CANCELED",
            _ => "WAIT"
        };
    }

    private void BookingButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to book?",
            "Confirmation of action",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        if (DataGrid.SelectedItem is not DataRowView selectedRow)
        {
            MessageBox.Show("Please select a trip from the table.");
            return;
        }

        int tripId;
        try
        {
            tripId = Convert.ToInt32(selectedRow["Trip ID"]);
        }
        catch
        {
            MessageBox.Show("Failed to retrieve trip ID.");
            return;
        }

        string checkQuery = @"SELECT COUNT(*) FROM bookings 
                          WHERE id_user = @id_user AND id_trip = @id_trip 
                          AND status IN ('CONFIRMED')";

        string availabilityQuery = "SELECT total_count FROM trips WHERE id_trip = @id_trip";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
        {
            checkCmd.Parameters.AddWithValue("@id_user", currentUserId);
            checkCmd.Parameters.AddWithValue("@id_trip", tripId);

            try
            {
                conn.Open();

                
                int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (existingCount > 0)
                {
                    MessageBox.Show("You have already booked this trip (CONFIRMED).");
                    return;
                }

                
                string status = GetRandomStatus();

                
                if (status == "CONFIRMED")
                {
                    using (MySqlCommand availCmd = new MySqlCommand(availabilityQuery, conn))
                    {
                        availCmd.Parameters.AddWithValue("@id_trip", tripId);
                        object totalObj = availCmd.ExecuteScalar();
                        if (totalObj == null)
                        {
                            MessageBox.Show("Trip not found.");
                            return;
                        }

                        int totalCount = Convert.ToInt32(totalObj);
                        if (totalCount <= 0)
                        {
                            MessageBox.Show("No seats available for this trip.");
                            return;
                        }
                    }
                }

                
                DateTime bookingDate = DateTime.Now;

                string insertQuery = @"INSERT INTO bookings (id_user, id_trip, status, booking_date)
                                   VALUES (@id_user, @id_trip, @status, @booking_date)";

                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@id_user", currentUserId);
                    insertCmd.Parameters.AddWithValue("@id_trip", tripId);
                    insertCmd.Parameters.AddWithValue("@status", status);
                    insertCmd.Parameters.AddWithValue("@booking_date", bookingDate);

                    insertCmd.ExecuteNonQuery();
                }

               
                if (status == "CONFIRMED")
                {
                    string updateCountQuery = @"UPDATE trips SET total_count = total_count - 1 
                                            WHERE id_trip = @id_trip AND total_count > 0";
                    using (MySqlCommand updateCmd = new MySqlCommand(updateCountQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@id_trip", tripId);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Booking created with status: {status}");
                LoadUserBookings();
                FilterData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }

    private void AddReviewButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
        "Are you sure you want to add review?",
        "Confirmation of action",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            if (DataGrid_Booking.SelectedItem is DataRowView selectedRow)
            {
                int tripId = Convert.ToInt32(selectedRow["Trip ID"]);
                DateTime reviewDate = DateTime.Now;

                string insertQuery = @"INSERT INTO reviews (id_user, id_trip, review_date, rating)
                               VALUES (@id_user, @id_trip, @review_date, @rating)";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                {
                    string rating = Convert.ToString(GetSelectedRating());

                    if (rating == "0")
                    {
                        MessageBox.Show("Please select a rating.");
                        return;
                    }

                    cmd.Parameters.AddWithValue("@id_user", currentUserId);
                    cmd.Parameters.AddWithValue("@id_trip", tripId);
                    cmd.Parameters.AddWithValue("@review_date", reviewDate);
                    cmd.Parameters.AddWithValue("@rating", rating);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Review created!");
                        LoadUserReviews();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a trip from the table.");
            }
        }
        else
        {
            return;
        }
    }

    private int GetSelectedRating()
    {
        if (RadioButton1.IsChecked == true)
        {
            return 1;
        }
        else if (RadioButton2.IsChecked == true)
        {
            return 2;
        }
        else if (RadioButton3.IsChecked == true)
        {
            return 3;
        }
        else if (RadioButton4.IsChecked == true)
        {
            return 4;
        }
        else if (RadioButton5.IsChecked == true)
        {
            return 5;
        }
        else
        {
            return 0;
        }
    }

    private void DeleteReviewButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
        "Are you sure you want to delete review?",
        "Confirmation of action",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            if (DataGrid_Review.SelectedItem is DataRowView selectedRow)
            {
                int reviewId = Convert.ToInt32(selectedRow["ID Reviews"]);
                string deleteQuery = @"DELETE FROM reviews WHERE id_reviews = @id_reviews";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id_reviews", reviewId);
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        LoadUserReviews();
                        MessageBox.Show("Review deleted!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a review from the table.");
            }
        }
        else
        {
            return;
        }
    }

    private void CancelBookButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
        "Are you sure you want to cancel book?",
        "Confirmation of action",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            if (DataGrid_Booking.SelectedItem == null)
            {
                MessageBox.Show("Please select a booking to cancel.");
                return;
            }

            DataRowView selectedRow = (DataRowView)DataGrid_Booking.SelectedItem;
            string currentStatus = selectedRow["Status"].ToString();
            int bookingId = Convert.ToInt32(selectedRow["ID Book"]);

            if (currentStatus == "CANCELED")
            {
                MessageBox.Show("This booking has already been canceled.");
                return;
            }

            string query = @"UPDATE bookings SET status = 'CANCELED' WHERE id_bookings = @bookingId";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@bookingId", bookingId);
                    cmd.ExecuteNonQuery();
                    LoadUserBookings();
                    MessageBox.Show("Booking successfully canceled.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error canceling booking: " + ex.Message);
                }
            }
        }
        else
        {
            return;
        }
    }

    private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
        "Are you sure you want to delete book?",
        "Confirmation of action",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            if (DataGrid_Booking.SelectedItem == null)
            {
                MessageBox.Show("Please select a booking to cancel.");
                return;
            }

            DataRowView selectedRow = (DataRowView)DataGrid_Booking.SelectedItem;

            int bookingId = Convert.ToInt32(selectedRow["ID Book"]);
            string query = @"DELETE FROM bookings WHERE id_bookings = @bookingId";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@bookingId", bookingId);
                    cmd.ExecuteNonQuery();
                    LoadUserBookings();
                    MessageBox.Show("Booking successfully deleted.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting booking: " + ex.Message);
                }
            }
        }
        else
        {
            return;
        }
    }

    private void SearchTextBox1_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox1.Text.Trim();

        try
        {
            DataTable dt = _dbService.ExecuteStoredProcedure(
                "SearchUserBookings",
                new MySqlParameter("p_id_user", currentUserId),
                new MySqlParameter("p_search_text", "%" + searchText + "%")
            );
            DataGrid_Booking.ItemsSource = dt.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadUniqueValues(string sql, ComboBox comboBox, string displayMember)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            var adapter = new MySqlDataAdapter(sql, connection);
            var dt = new DataTable();
            adapter.Fill(dt);
            comboBox.ItemsSource = dt.DefaultView;
            comboBox.DisplayMemberPath = displayMember;
            comboBox.SelectedValuePath = displayMember;
        }
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        ToggleComboBoxes();
        FilterData();
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        ToggleComboBoxes();
        FilterData();
    }

    private void ToggleComboBoxes()
    {
        TopicComboBox.IsEnabled = TopicCheckBox.IsChecked == true;
        CountryComboBox.IsEnabled = CountryCheckBox.IsChecked == true;
        PriceComboBox.IsEnabled = PriceCheckBox.IsChecked == true;
        TypeTransportComboBox.IsEnabled = TypeTransportCheckBox.IsChecked == true;
    }

    private void FilterData()
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("FilterTrips", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("p_topic",
                        TopicCheckBox.IsChecked == true && TopicComboBox.SelectedValue != null
                        ? TopicComboBox.SelectedValue
                        : null);
                    command.Parameters.AddWithValue("p_country",
                        CountryCheckBox.IsChecked == true && CountryComboBox.SelectedValue != null
                        ? CountryComboBox.SelectedValue
                        : null);
                    command.Parameters.AddWithValue("p_price",
                        PriceCheckBox.IsChecked == true && PriceComboBox.SelectedValue != null
                        ? PriceComboBox.SelectedValue
                        : null);
                    command.Parameters.AddWithValue("p_type",
                        TypeTransportCheckBox.IsChecked == true && TypeTransportComboBox.SelectedValue != null
                        ? TypeTransportComboBox.SelectedValue
                        : null);
                    command.Parameters.AddWithValue("p_search_text",
                        !string.IsNullOrWhiteSpace(SearchTextBox.Text)
                        ? "%" + SearchTextBox.Text + "%"
                        : null);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        DataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            MessageBox.Show($"Database connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FilterData();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterData();
    }

    // ADMIN PANEL =======================================================================================================================================

    private void AdminCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        AdminTabItem.Visibility = Visibility.Visible;
        TableComboBox.IsEnabled = true;
        LoadTableButton.IsEnabled = true;
        AddButton.IsEnabled = true;
        DeleteButton.IsEnabled = true;
        AdminComboBox.IsEnabled = true;
        ViewButton.IsEnabled = true;
    }

    private void AdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        AdminTabItem.Visibility = Visibility.Collapsed;
        TableComboBox.IsEnabled = false;
        LoadTableButton.IsEnabled = false;
        AddButton.IsEnabled = false;
        DeleteButton.IsEnabled = false;
        AdminComboBox.IsEnabled = false;
        ViewButton.IsEnabled = false;
    }

    private void LoadTableButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DatabaseHelper helperDataBase = new DatabaseHelper();
            if (TableComboBox.Text == "user")
            {
                AdminDataGrid.ItemsSource = null;
                var table = helperDataBase.SelectQuery(@"SELECT * FROM db_joint_journey.user;");
                AdminDataGrid.ItemsSource = table.DefaultView;
            }
            if (TableComboBox.Text == "trips")
            {
                AdminDataGrid.ItemsSource = null;
                var table = helperDataBase.SelectQuery(@"SELECT * FROM db_joint_journey.trips;");
                AdminDataGrid.ItemsSource = table.DefaultView;
            }
            if (TableComboBox.Text == "transport")
            {
                AdminDataGrid.ItemsSource = null;
                var table = helperDataBase.SelectQuery(@"SELECT * FROM db_joint_journey.transport;");
                AdminDataGrid.ItemsSource = table.DefaultView;
            }
            if (TableComboBox.Text == "reviews")
            {
                AdminDataGrid.ItemsSource = null;
                var table = helperDataBase.SelectQuery(@"SELECT * FROM db_joint_journey.reviews;");
                AdminDataGrid.ItemsSource = table.DefaultView;
            }
            if (TableComboBox.Text == "destination")
            {
                AdminDataGrid.ItemsSource = null;
                var table = helperDataBase.SelectQuery(@"SELECT * FROM db_joint_journey.destination;");
                AdminDataGrid.ItemsSource = table.DefaultView;
            }
            if (TableComboBox.Text == "bookings")
            {
                AdminDataGrid.ItemsSource = null;
                var table = helperDataBase.SelectQuery(@"SELECT * FROM db_joint_journey.bookings;");
                AdminDataGrid.ItemsSource = table.DefaultView;
            }
            if (TableComboBox.Text == "accommodation")
            {
                AdminDataGrid.ItemsSource = null;
                var table = helperDataBase.SelectQuery(@"SELECT * FROM db_joint_journey.accommodation;");
                AdminDataGrid.ItemsSource = table.DefaultView;
            }
            else if (TableComboBox.Text == "")
            {
                MessageBox.Show("Please select a table.");
                return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
        "Are you sure you want to add?",
        "Confirmation of action",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            DatabaseHelper helperDataBase = new DatabaseHelper();
            if (TableComboBox.Text == "user")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO user (first_name_user, last_name_user, email_user, password_user, birth_date) 
                    VALUES ('first_name', 'last_name', '@gmail.com', 'password', '2025-01-01');");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
                FilterData();
            }

            if (TableComboBox.Text == "trips")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO trips (name_trip, topic, total_count, id_destination, date, price, id_transport, id_accommodation) 
                    VALUES ('name_trip', 'topic', 1, 1, '2025-01-01', 000, 1, 1);");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
                FilterData();
            }
            if (TableComboBox.Text == "transport")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO transport (type, transport_price) 
                    VALUES ('type', 000);");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
                FilterData();
            }
            if (TableComboBox.Text == "reviews")
            {
                var table = helperDataBase.SelectQuery(@$"INSERT INTO reviews (id_user, id_trip, rating, review_date) 
                    VALUES ({currentUserId}, 1, '1', '2025-01-01');");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
                FilterData();
            }
            if (TableComboBox.Text == "destination")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO destination (city, country) 
                    VALUES ('city', 'country');");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
                FilterData();
            }
            if (TableComboBox.Text == "bookings")
            {
                var table = helperDataBase.SelectQuery(@$"INSERT INTO bookings (status, id_user, id_trip, booking_date) 
                    VALUES ('WAIT', {currentUserId}, 1, '2025-01-01');");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
                FilterData();
            }
            if (TableComboBox.Text == "accommodation")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO accommodation (hotel_name, address, rooms_available, room_price) 
                    VALUES ('hotel_name', 'address', 000, 000);");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
                FilterData();
            }
            else if (TableComboBox.Text == "")
            {
                MessageBox.Show("Please select a table.");
                return;
            }
        }
        else
        {
            return;
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
        "Are you sure you want to delete?",
        "Confirmation of action",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                DatabaseHelper delete = new DatabaseHelper();

                if (AdminDataGrid.SelectedItem is DataRowView selectedRow)
                {
                    if (TableComboBox.Text == "user")
                    {
                        int id_user = Convert.ToInt32(selectedRow["id_user"]);
                        string query = @$"DELETE FROM user WHERE id_user = {id_user}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                        FilterData();
                    }
                    else if (TableComboBox.Text == "trips")
                    {
                        int id_trip = Convert.ToInt32(selectedRow["id_trip"]);
                        string query = @$"DELETE FROM trips WHERE id_trip = {id_trip}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                        FilterData();
                    }
                    else if (TableComboBox.Text == "transport")
                    {
                        int id_transport = Convert.ToInt32(selectedRow["id_transport"]);
                        string query = @$"DELETE FROM transport WHERE id_transport = {id_transport}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                        FilterData();
                    }
                    else if (TableComboBox.Text == "reviews")
                    {
                        int id_reviews = Convert.ToInt32(selectedRow["id_reviews"]);
                        string query = @$"DELETE FROM reviews WHERE id_reviews = {id_reviews}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                        FilterData();
                    }
                    else if (TableComboBox.Text == "destination")
                    {
                        int id_destination = Convert.ToInt32(selectedRow["id_destination"]);
                        string query = @$"DELETE FROM destination WHERE id_destination = {id_destination}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                        FilterData();
                    }
                    else if (TableComboBox.Text == "bookings")
                    {
                        int id_bookings = Convert.ToInt32(selectedRow["id_bookings"]);
                        string query = @$"DELETE FROM bookings WHERE id_bookings = {id_bookings}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                        FilterData();
                    }
                    else if (TableComboBox.Text == "accommodation")
                    {
                        int id_accommodation = Convert.ToInt32(selectedRow["id_accommodation"]);
                        string query = @$"DELETE FROM accommodation WHERE id_accommodation = {id_accommodation}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                        FilterData();
                    }
                }
                else
                {
                    MessageBox.Show("Error.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        else
        {
            return;
        }
    }

    private void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to view?",
            "Confirmation of action",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        int selectIndex = AdminComboBox.SelectedIndex;
        string procedureName = selectIndex switch
        {
            0 => "GetConfirmedBookingsByTopic",
            1 => "GetConfirmedBookingsByTripName",
            2 => "GetUserBookingCounts",
            _ => null
        };

        if (procedureName == null)
        {
            MessageBox.Show("Invalid selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        AdminDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            MessageBox.Show($"Database connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AdminDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit)
            return;

        Dispatcher.InvokeAsync(() =>
        {
            var row = (DataRowView)e.Row.Item;
            string columnName = e.Column.Header.ToString();
            object newValue = ((TextBox)e.EditingElement).Text;

            int selectIndex = TableComboBox.SelectedIndex;

            switch (selectIndex)
            {
                case 0:
                    int id = Convert.ToInt32(row["id_user"]);
                    string table = "user";
                    string id_table = "id_user";
                    UpdateDatabase(id, columnName, newValue, table, id_table);
                    FilterData();
                    break;
                case 1:
                    id = Convert.ToInt32(row["id_trip"]);
                    table = "trips";
                    id_table = "id_trip";
                    UpdateDatabase(id, columnName, newValue, table, id_table);
                    FilterData();
                    break;
                case 2:
                    id = Convert.ToInt32(row["id_transport"]);
                    table = "transport";
                     id_table = "id_transport";
                    UpdateDatabase(id, columnName, newValue, table, id_table);
                    FilterData();
                    break;
                case 3:
                    id = Convert.ToInt32(row["id_reviews"]);
                    table = "reviews";
                    id_table = "id_reviews";
                    UpdateDatabase(id, columnName, newValue, table, id_table);
                    FilterData();
                    break;
                case 4:
                    id = Convert.ToInt32(row["id_destination"]);
                    table = "destination";
                    id_table = "id_destination";
                    UpdateDatabase(id, columnName, newValue, table, id_table);
                    FilterData();
                    break;
                case 5:
                    id = Convert.ToInt32(row["id_bookings"]);
                    table = "bookings";
                    id_table = "id_bookings";
                    UpdateDatabase(id, columnName, newValue, table, id_table);
                    FilterData();
                    break;
                case 6:
                    id = Convert.ToInt32(row["id_accommodation"]);
                    table = "accommodation";
                    id_table = "id_accommodation";
                    UpdateDatabase(id, columnName, newValue, table, id_table);
                    FilterData();
                    break;
            }
        });
    }

    private void UpdateDatabase(int id, string columnName, object value, string table, string id_table)
    {
        string query = @$"UPDATE {table} SET `{columnName}` = @value WHERE {id_table} = @id";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating: " + ex.Message);
            }
        }
    }

    private void ExitApp()
    {
        Application.Current.Shutdown();
    }

    private void ExitMainButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
        "Are you sure you want to exit?",
        "Confirmation of action",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            ExitApp();
        }
        else
        {
            return;
        }
    }
}