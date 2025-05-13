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
        string query = "SELECT " +
            "id_bookings as \"ID Book\", " +
            "status as \"Status\", " +
            "DATE_FORMAT(booking_date, '%d.%m.%Y') as \"Book Date\", " +
            "name_trip AS \"Trip Name\", " +
            "topic AS \"Topic\", " +
            "DATE_FORMAT(date, '%d.%m.%Y') AS \"Trip Date\", " +
            "CONCAT(FORMAT(price, 2), ' €') AS \"Trip Price\", " +
            "city AS \"Destination City\", " +
            "country AS \"Destination Country\", " +
            "type AS \"Transport Type\", " +
            "CONCAT(FORMAT(transport_price, 2), ' €') AS \"Transport Price\", " +
            "hotel_name AS \"Hotel Name\", " +
            "address AS \"Hotel Address\", " +
            "rooms_available AS \"Rooms Available\", " +
            "CONCAT(FORMAT(room_price, 2), ' €') AS \"Room Price\", " +
            "CONCAT(FORMAT(price + transport_price + room_price, 2), ' €') AS \"Total Cost\" " +
            "FROM bookings " +
            "JOIN trips ON bookings.id_trip = trips.id_trip " +
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
                    DataTable bookingsTable = new DataTable();
                    adapter.Fill(bookingsTable);
                    DataGrid_Booking.ItemsSource = bookingsTable.DefaultView;
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
}