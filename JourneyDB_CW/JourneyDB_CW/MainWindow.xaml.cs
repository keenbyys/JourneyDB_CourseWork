using MySql.Data.MySqlClient;
using System.Data;
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
    private string connectionString = "Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;";
    private readonly int currentUserId;

    public MainWindow(int userId)
    {
        InitializeComponent();
        LoadDataBase();

        currentUserId = userId;
        LoadUserData();
        LoadUserBookings();
        LoadUserReviews();
    }



    public void LoadDataBase()
    {
        try
        {
            DataBaseHelper connectionDataBase = new DataBaseHelper();
            DataGrid.ItemsSource = null;
            var table = connectionDataBase.SelectQuery("SELECT " +
                "trips.id_trip AS \"Trip ID\", " +
                "trips.name_trip AS \"Trip Name\", " +
                "trips.topic AS \"Topic\", " +
                "DATE_FORMAT(trips.date, '%d.%m.%Y') AS \"Trip Date\", " +
                "CONCAT(FORMAT(trips.price, 2), ' €') AS \"Trip Price\", " +
                "destination.city AS \"Destination City\", " +
                "destination.country AS \"Destination Country\", " +
                "transport.type AS \"Transport Type\", " +
                "CONCAT(FORMAT(transport.transport_price, 2), ' €') AS \"Transport Price\", " +
                "accommodation.hotel_name AS \"Hotel Name\", " +
                "accommodation.address AS \"Hotel Address\", " +
                "accommodation.rooms_available AS \"Rooms Available\", " +
                "CONCAT(FORMAT(accommodation.room_price, 2), ' €') AS \"Room Price\", " +
                "CONCAT(FORMAT(trips.price + transport.transport_price + accommodation.room_price, 2), ' €') AS \"Total Cost\" " +
                "FROM trips " +
                "JOIN destination ON trips.id_destination = destination.id_destination " +
                "JOIN transport ON trips.id_transport = transport.id_transport " +
                "JOIN accommodation ON trips.id_accommodation = accommodation.id_accommodation " +
                "ORDER BY id_trip;");
            DataGrid.ItemsSource = table.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private void LoadUserData()
    {
        string query = "SELECT first_name_user, last_name_user, email_user, birth_date FROM user WHERE id_user = @UserId";
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
        string query = @"SELECT 
            id_bookings AS ""ID Book"", 
            status AS ""Status"", 
            trips.id_trip AS ""Trip ID"", 
            booking_date AS ""Book Date"", 
            name_trip AS ""Trip Name"", 
            topic AS ""Topic"", 
            DATE_FORMAT(date, '%d.%m.%Y') AS ""Trip Date"", 
            CONCAT(FORMAT(price, 2), ' €') AS ""Trip Price"", 
            city AS ""Destination City"", 
            country AS ""Destination Country"", 
            type AS ""Transport Type"", 
            CONCAT(FORMAT(transport_price, 2), ' €') AS ""Transport Price"", 
            hotel_name AS ""Hotel Name"", 
            address AS ""Hotel Address"", 
            rooms_available AS ""Rooms Available"", 
            CONCAT(FORMAT(room_price, 2), ' €') AS ""Room Price"", 
            CONCAT(FORMAT(price + transport_price + room_price, 2), ' €') AS ""Total Cost""
            FROM bookings
                JOIN trips ON bookings.id_trip = trips.id_trip
                JOIN destination ON trips.id_destination = destination.id_destination
                JOIN transport ON trips.id_transport = transport.id_transport
                JOIN accommodation ON trips.id_accommodation = accommodation.id_accommodation
            WHERE id_user = @UserId
            ORDER BY booking_date;";
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                {
                    DataTable bookingsTable = new DataTable();
                    adapter.Fill(bookingsTable);
                    DataGrid_Booking.ItemsSource = bookingsTable.DefaultView;

                    DataGrid_Booking.AutoGeneratingColumn += (s, e) =>
                    {
                        if (e.PropertyName == "Trip ID")
                        {
                            e.Column.Visibility = Visibility.Collapsed;
                        }
                    };
                }
            }
        }
    }

    private void LoadUserReviews()
    {
        string query = "SELECT " +
            "id_reviews as \"ID Reviews\", " +
            "DATE_FORMAT(review_date, '%d.%m.%Y') as \"Review Date\", " +
            "rating AS \"Rating\", " +
            "name_trip AS \"Trip Name\", " +
            "topic AS \"Topic\", " +
            "city AS \"Destination City\", " +
            "country AS \"Destination Country\", " +
            "type AS \"Transport Type\", " +
            "hotel_name AS \"Hotel Name\", " +
            "CONCAT(FORMAT(price + transport_price + room_price, 2), ' €') AS \"Total Cost\" " +
            "FROM reviews " +
            "JOIN trips ON reviews.id_trip = trips.id_trip " +
            "JOIN destination ON trips.id_destination = destination.id_destination " +
            "JOIN transport ON trips.id_transport = transport.id_transport " +
            "JOIN accommodation ON trips.id_accommodation = accommodation.id_accommodation " +
            "WHERE id_user = @UserId;";
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                {
                    DataTable reviewsTable = new DataTable();
                    adapter.Fill(reviewsTable);
                    DataGrid_Review.ItemsSource = reviewsTable.DefaultView;
                }
            }
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
        if (DataGrid.SelectedItem is DataRowView selectedRow)
        {
            int tripId;
            try
            {
                tripId = Convert.ToInt32(selectedRow["Trip ID"]); // убедись, что колонка называется именно так
            }
            catch
            {
                MessageBox.Show("Не вдалося отримати ID подорожі.");
                return;
            }

            string checkQuery = @"SELECT COUNT(*) FROM bookings 
                              WHERE id_user = @id_user AND id_trip = @id_trip 
                              AND status IN ('CONFIRMED', 'WAIT')";

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
                        MessageBox.Show("Ви вже забронювали цю подорож (CONFIRMED або WAIT).");
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
                        MessageBox.Show($"Бронювання створено зі статусом: {status}");
                        LoadUserBookings();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка: " + ex.Message);
                }
            }
        }
        else
        {
            MessageBox.Show("Будь ласка, виберіть подорож із таблиці.");
        }

    }

    private void AddReviewButton_Click(object sender, RoutedEventArgs e)
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
                    MessageBox.Show("Будь ласка, виберіть рейтинг.");
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
                    MessageBox.Show("Review створено!");
                    LoadUserReviews();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка: " + ex.Message);
                }
            }
        }
        else
        {
            MessageBox.Show("Будь ласка, виберіть подорож із таблиці.");
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
        if (DataGrid_Review.SelectedItem is DataRowView selectedRow)
        {
            int reviewId = Convert.ToInt32(selectedRow["ID Reviews"]);
            string deleteQuery = "DELETE FROM reviews WHERE id_reviews = @id_reviews";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("@id_reviews", reviewId);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    LoadUserReviews();
                    MessageBox.Show("Review видалено!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка: " + ex.Message);
                }
            }
        }
        else
        {
            MessageBox.Show("Будь ласка, виберіть відгук із таблиці.");
        }
    }

    private void CancelBookButton_Click(object sender, RoutedEventArgs e)
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

        string query = "UPDATE bookings SET status = 'CANCELED' WHERE id_bookings = @bookingId";

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

    private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataGrid_Booking.SelectedItem == null)
        {
            MessageBox.Show("Please select a booking to cancel.");
            return;
        }

        DataRowView selectedRow = (DataRowView)DataGrid_Booking.SelectedItem;

        int bookingId = Convert.ToInt32(selectedRow["ID Book"]);
        string query = "DELETE FROM bookings WHERE id_bookings = @bookingId";
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

    private void SearchTextBox1_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox1.Text.Trim();

        string query = @"
SELECT 
    id_bookings AS 'ID Book', 
    status AS 'Status', 
    trips.id_trip AS 'Trip ID', 
    booking_date AS 'Book Date', 
    name_trip AS 'Trip Name', 
    topic AS 'Topic', 
    DATE_FORMAT(date, '%d.%m.%Y') AS 'Trip Date', 
    CONCAT(FORMAT(price, 2), ' €') AS 'Trip Price', 
    city AS 'Destination City', 
    country AS 'Destination Country', 
    type AS 'Transport Type', 
    CONCAT(FORMAT(transport_price, 2), ' €') AS 'Transport Price', 
    hotel_name AS 'Hotel Name', 
    address AS 'Hotel Address', 
    rooms_available AS 'Rooms Available', 
    CONCAT(FORMAT(room_price, 2), ' €') AS 'Room Price', 
    CONCAT(FORMAT(price + transport_price + room_price, 2), ' €') AS 'Total Cost'
FROM bookings
JOIN trips ON bookings.id_trip = trips.id_trip
JOIN destination ON trips.id_destination = destination.id_destination
JOIN transport ON trips.id_transport = transport.id_transport
JOIN accommodation ON trips.id_accommodation = accommodation.id_accommodation
WHERE id_user = @UserId AND name_trip LIKE @SearchText
ORDER BY booking_date DESC;";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", currentUserId); // твоя переменная ID текущего пользователя
                cmd.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                DataGrid_Booking.ItemsSource = dt.DefaultView;
            }
        }
    }
}