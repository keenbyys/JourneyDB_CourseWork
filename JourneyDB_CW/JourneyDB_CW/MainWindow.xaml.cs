using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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

namespace JourneyDB_CW;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string connectionString = "Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;";
    DataBaseProcedures _dbService = new DataBaseProcedures("Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;");
    private readonly int currentUserId;

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

        if (result == MessageBoxResult.Yes)
        {
            if (DataGrid.SelectedItem is DataRowView selectedRow)
            {
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
                            MessageBox.Show($"Booking created with status: {status}");
                            LoadUserBookings();
                        }
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
            }

            if (TableComboBox.Text == "trips")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO trips (name_trip, topic, id_destination, date, price, id_transport, id_accommodation) 
                    VALUES ('name_trip', 'topic', 000, 'date', 000, 000, 000);");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
            }
            if (TableComboBox.Text == "transport")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO transport (type, transport_price) 
                    VALUES ('type', 000);");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
            }
            if (TableComboBox.Text == "reviews")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO reviews (id_user, id_trip, rating, review_date) 
                    VALUES (000, 000, 'rating', 'review_date');");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
            }
            if (TableComboBox.Text == "destination")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO destination (city, country) 
                    VALUES ('city', 'country');");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
            }
            if (TableComboBox.Text == "bookings")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO bookings (status, id_user, id_trip, booking_date) 
                    VALUES ('status', 000, 000, 'booking_date');");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
            }
            if (TableComboBox.Text == "accommodation")
            {
                var table = helperDataBase.SelectQuery(@"INSERT INTO accommodation (hotel_name, address, rooms_available, room_price) 
                    VALUES ('hotel_name', 'address', 000, 000);");
                LoadTableButton_Click(sender, e);
                TotalLoadData();
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
                    }
                    else if (TableComboBox.Text == "trips")
                    {
                        int id_trip = Convert.ToInt32(selectedRow["id_trip"]);
                        string query = @$"DELETE FROM trips WHERE id_trip = {id_trip}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                    }
                    else if (TableComboBox.Text == "transport")
                    {
                        int id_transport = Convert.ToInt32(selectedRow["id_transport"]);
                        string query = @$"DELETE FROM transport WHERE id_transport = {id_transport}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                    }
                    else if (TableComboBox.Text == "reviews")
                    {
                        int id_reviews = Convert.ToInt32(selectedRow["id_reviews"]);
                        string query = @$"DELETE FROM reviews WHERE id_reviews = {id_reviews}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                    }
                    else if (TableComboBox.Text == "destination")
                    {
                        int id_destination = Convert.ToInt32(selectedRow["id_destination"]);
                        string query = @$"DELETE FROM destination WHERE id_destination = {id_destination}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                    }
                    else if (TableComboBox.Text == "bookings")
                    {
                        int id_bookings = Convert.ToInt32(selectedRow["id_bookings"]);
                        string query = @$"DELETE FROM bookings WHERE id_bookings = {id_bookings}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
                    }
                    else if (TableComboBox.Text == "accommodation")
                    {
                        int id_accommodation = Convert.ToInt32(selectedRow["id_accommodation"]);
                        string query = @$"DELETE FROM accommodation WHERE id_accommodation = {id_accommodation}";
                        delete.NoneQuery(query);
                        LoadTableButton_Click(sender, e);
                        TotalLoadData();
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
                    cmd.CommandType = CommandType.StoredProcedure; // Specify that this is a stored procedure
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        AdminDataGrid.ItemsSource = dt.DefaultView; // Bind data to AdminDataGrid
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