using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AppointmentBooking.Models;

namespace AppointmentBooking.Controllers;

public class AdminController : Controller
{
    private readonly string _conn;

    public AdminController(IConfiguration config)
    {
        _conn = config.GetConnectionString("DefaultConnection")!;
    }

    private bool IsAdmin() =>
        HttpContext.Session.GetString("IsAdmin") == "true";

    public IActionResult Index(string? status, string? service, string? search)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var list = new List<Appointment>();

        using var con = new SqlConnection(_conn);
        con.Open();

        var sql = @"SELECT * FROM Appointments WHERE 1=1";
        if (!string.IsNullOrEmpty(status)) sql += " AND Status = @status";
        if (!string.IsNullOrEmpty(service)) sql += " AND ServiceType = @service";
        if (!string.IsNullOrEmpty(search)) sql += " AND (ClientName LIKE @search OR Email LIKE @search)";
        sql += " ORDER BY AppointmentDate, AppointmentTime";

        using var cmd = new SqlCommand(sql, con);
        if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
        if (!string.IsNullOrEmpty(service)) cmd.Parameters.AddWithValue("@service", service);
        if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Appointment
            {
                Id = (int)reader["Id"],
                ClientName = reader["ClientName"].ToString()!,
                Email = reader["Email"].ToString()!,
                ServiceType = reader["ServiceType"].ToString()!,
                AppointmentDate = DateOnly.FromDateTime((DateTime)reader["AppointmentDate"]),
                AppointmentTime = TimeOnly.FromTimeSpan((TimeSpan)reader["AppointmentTime"]),
                Status = reader["Status"].ToString()!
            });
        }
        reader.Close();

        // Analytics
        using var statsCmd = new SqlCommand(@"
            SELECT
                COUNT(*) AS Total,
                SUM(CASE WHEN Status='Pending'   THEN 1 ELSE 0 END) AS Pending,
                SUM(CASE WHEN Status='Confirmed' THEN 1 ELSE 0 END) AS Confirmed,
                SUM(CASE WHEN Status='Cancelled' THEN 1 ELSE 0 END) AS Cancelled,
                SUM(CASE WHEN CAST(AppointmentDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS Today
            FROM Appointments", con);

        using var statsReader = statsCmd.ExecuteReader();
        if (statsReader.Read())
        {
            ViewBag.Total = statsReader["Total"];
            ViewBag.Pending = statsReader["Pending"];
            ViewBag.Confirmed = statsReader["Confirmed"];
            ViewBag.Cancelled = statsReader["Cancelled"];
            ViewBag.Today = statsReader["Today"];
        }
        statsReader.Close();

        // Bookings per service
        using var svcCmd = new SqlCommand(@"
            SELECT ServiceType, COUNT(*) AS Cnt
            FROM Appointments GROUP BY ServiceType", con);
        var svcData = new Dictionary<string, int>();
        using var svcReader = svcCmd.ExecuteReader();
        while (svcReader.Read())
            svcData[svcReader["ServiceType"].ToString()!] = (int)svcReader["Cnt"];

        ViewBag.ServiceData = svcData;
        ViewBag.StatusFilter = status;
        ViewBag.ServiceFilter = service;
        ViewBag.Search = search;

        return View(list);
    }

    [HttpPost]
    public IActionResult UpdateStatus(int id, string status)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        using var con = new SqlConnection(_conn);
        con.Open();
        using var cmd = new SqlCommand(
            "UPDATE Appointments SET Status=@s WHERE Id=@id", con);
        cmd.Parameters.AddWithValue("@s", status);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        return RedirectToAction("Index");
    }
}