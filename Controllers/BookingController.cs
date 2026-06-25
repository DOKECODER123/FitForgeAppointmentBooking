using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AppointmentBooking.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace AppointmentBooking.Controllers;

public class BookingController : Controller
{
    private readonly string _conn;
    private readonly IConfiguration _config;

    public BookingController(IConfiguration config)
    {
        _config = config; //declared new variable "_config" that will receive the same value as "config" parameter
        _conn = config.GetConnectionString("DefaultConnection")!; //calling the default connection from the appsettings.json as "_conn" variable.
    }

    private int GetMaxSlots()
    {
        using var con = new SqlConnection(_conn);
        con.Open();
        using var cmd = new SqlCommand("SELECT TOP 1 MaxSlots FROM SlotConfig", con);
        var result = cmd.ExecuteScalar();
        return result != null ? (int)result : 10;
    }

    private int GetBookedSlots(string date)
    {
        using var con = new SqlConnection(_conn);
        con.Open();
        using var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM Appointments WHERE CAST(AppointmentDate AS DATE) = @date AND Status != 'Cancelled'", con);
        cmd.Parameters.AddWithValue("@date", date);
        return (int)cmd.ExecuteScalar();
    }

    public IActionResult Index()
    {
        var maxSlots = GetMaxSlots();
        ViewBag.MaxSlots = maxSlots;
        return View();
    }

    [HttpGet]
    public IActionResult GetSlots(string date)
    {
        var maxSlots = GetMaxSlots();
        var booked = GetBookedSlots(date);
        var remaining = maxSlots - booked;
        return Json(new { remaining, maxSlots, booked, full = remaining <= 0 });
    }

    [HttpPost]
    public IActionResult Index(Appointment model)
    {
        var maxSlots = GetMaxSlots();
        var booked = GetBookedSlots(model.AppointmentDate.ToString("yyyy-MM-dd"));

        if (booked >= maxSlots)
        {
            ViewBag.Error = "Sorry, that date is fully booked. Please choose another date.";
            ViewBag.MaxSlots = maxSlots;
            return View(model);
        }

        using var con = new SqlConnection(_conn);
        con.Open();

        var sql = @"INSERT INTO Appointments
                    (ClientName, Email, Phone, ServiceType, AppointmentDate, AppointmentTime)
                    VALUES (@name, @email, @phone, @service, @date, @time)";

        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@name", model.ClientName);
        cmd.Parameters.AddWithValue("@email", model.Email);
        cmd.Parameters.AddWithValue("@phone", model.Phone ?? "");
        cmd.Parameters.AddWithValue("@service", model.ServiceType);
        cmd.Parameters.Add("@date", System.Data.SqlDbType.Date).Value =
            model.AppointmentDate.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@time", System.Data.SqlDbType.Time).Value =
            new TimeSpan(model.AppointmentTime.Hour, model.AppointmentTime.Minute, 0);
        cmd.ExecuteNonQuery();

        TrySendEmail(model);

        return RedirectToAction("Confirmation");
    }

    private void TrySendEmail(Appointment model)
    {
        try
        {
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderPass = _config["EmailSettings:SenderPassword"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("FitForge Gym", senderEmail));
            message.To.Add(new MailboxAddress(model.ClientName, model.Email));
            message.Subject = "Your FitForge appointment request";

            message.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family:sans-serif; max-width:500px; margin:auto; background:#1c1c1e; color:#e0e0e0; border-radius:12px; overflow:hidden;'>
                    <div style='background:#ff8c42; padding:28px 32px;'>
                        <h2 style='color:#fff; margin:0; font-size:22px;'>💪 FitForge</h2>
                        <p style='color:rgba(255,255,255,0.85); margin:6px 0 0; font-size:14px;'>Appointment confirmed</p>
                    </div>
                    <div style='padding:28px 32px;'>
                        <p>Hi <strong style='color:#ff8c42;'>{model.ClientName}</strong>,</p>
                        <p>We've received your session request. Here are your details:</p>
                        <table style='width:100%; border-collapse:collapse; margin:16px 0;'>
                            <tr style='border-bottom:1px solid #2a2a2e;'>
                                <td style='padding:10px 0; color:#888; font-size:13px;'>Service</td>
                                <td style='padding:10px 0; color:#fff; font-weight:600;'>{model.ServiceType}</td>
                            </tr>
                            <tr style='border-bottom:1px solid #2a2a2e;'>
                                <td style='padding:10px 0; color:#888; font-size:13px;'>Date</td>
                                <td style='padding:10px 0; color:#fff; font-weight:600;'>{model.AppointmentDate:MMMM dd, yyyy}</td>
                            </tr>
                            <tr>
                                <td style='padding:10px 0; color:#888; font-size:13px;'>Time</td>
                                <td style='padding:10px 0; color:#fff; font-weight:600;'>{model.AppointmentTime:hh:mm tt}</td>
                            </tr>
                        </table>
                        <p style='color:#888; font-size:13px;'>Our team will confirm your slot shortly. See you at the gym!</p>
                        <p style='color:#ff8c42; font-weight:700; margin-top:24px;'>— The FitForge Team 💪</p>
                    </div>
                </div>"
            };

            using var client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(senderEmail, senderPass);
            client.Send(message);
            client.Disconnect(true);
        }
        catch { }
    }

    public IActionResult Confirmation() => View();
}