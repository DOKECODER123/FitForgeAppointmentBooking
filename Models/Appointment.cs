namespace AppointmentBooking.Models;

public class Appointment
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string ServiceType { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
}