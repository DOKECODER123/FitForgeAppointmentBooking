# 💪 FitForge — Gym Appointment Booking System

A full-stack web application for managing gym appointments — built from scratch with ASP.NET Core MVC and SQL Server. Clients can book sessions in real time, and gym staff can manage bookings through a secure admin dashboard with live analytics.

> 🔗 **Live demo:** _add your deployed link here( deploying soon!)_
> 📂 **Tech stack:** ASP.NET Core MVC · SQL Server · MailKit · Bootstrap

---

## ✨ Features

**For clients**
- 📅 Book a session (Inquiry, Workout session, or Assessment) through a clean, responsive form
- 🟢 Live slot availability — see remaining slots for a chosen date before booking
- 📧 Automatic email confirmation sent on booking
- ✅ Instant confirmation page after submitting

**For gym staff (Admin)**
- 🔐 Secure, session-based admin login
- 📊 Analytics dashboard — total, pending, confirmed, and cancelled bookings at a glance
- 📈 Bookings-by-session-type breakdown
- 🔎 Search and filter bookings by client, status, or session type
- 🔄 Confirm or cancel any booking with one click
- 🔥 Today's appointments highlighted automatically

---

## 🛠️ Built with

| Layer        | Technology |
|--------------|------------|
| Backend      | ASP.NET Core MVC (C#) |
| Database     | SQL Server |
| Email        | MailKit / MimeKit (SMTP) |
| Frontend     | Razor Views, Bootstrap, custom CSS |
| Auth         | Session-based admin authentication |

---

## 📸 Screenshots

<img width="1918" height="907" alt="image" src="https://github.com/user-attachments/assets/00024022-4bca-4171-8d75-edcdc38530e1" />
<img width="1918" height="907" alt="image" src="https://github.com/user-attachments/assets/739bf7ef-fc54-4ae0-b2af-c2241363a354" />
<img width="1918" height="910" alt="image" src="https://github.com/user-attachments/assets/df5ea14f-dea9-4ccd-8c98-fd5fe0f8327d" />
<img width="1915" height="902" alt="image" src="https://github.com/user-attachments/assets/a6379b9e-55de-4495-94c2-25099db9a466" />



---

## 🚀 Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or SQL Server LocalDB)
- A Gmail account with an [App Password](https://myaccount.google.com/apppasswords) for email confirmations

### Setup

1. Clone the repo
   ```bash
   git clone https://github.com/YOURUSERNAME/FitForge.git
   cd FitForge
   ```

2. Create the database — run this in SQL Server Management Studio:
   ```sql
   CREATE DATABASE AppointmentDB;
   GO
   USE AppointmentDB;
   GO

   CREATE TABLE Appointments (
       Id              INT IDENTITY(1,1) PRIMARY KEY,
       ClientName      NVARCHAR(100) NOT NULL,
       Email           NVARCHAR(150) NOT NULL,
       Phone           NVARCHAR(20),
       ServiceType     NVARCHAR(100) NOT NULL,
       AppointmentDate DATE NOT NULL,
       AppointmentTime TIME NOT NULL,
       Status          NVARCHAR(20) DEFAULT 'Pending',
       CreatedAt       DATETIME DEFAULT GETDATE()
   );

   CREATE TABLE SlotConfig (
       Id       INT IDENTITY(1,1) PRIMARY KEY,
       MaxSlots INT DEFAULT 10
   );
   INSERT INTO SlotConfig (MaxSlots) VALUES (10);
   ```

3. Update `appsettings.json` with your own connection string, admin credentials, and email settings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AppointmentDB;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "AdminCredentials": {
       "Username": "admin",
       "Password": "your_password_here"
     },
     "EmailSettings": {
       "SenderEmail": "your_gmail@gmail.com",
       "SenderPassword": "your_16_char_app_password",
       "SenderName": "FitForge"
     }
   }
   ```

4. Run the app
   ```bash
   dotnet run
   ```

5. Open `https://localhost:7143` in your browser.

---

## 🗺️ Roadmap

- [ ] Deploy to a live hosting provider
- [ ] Add admin email notifications for new bookings
- [ ] Add recurring appointment support
- [ ] Add SMS reminders

---

## 📄 License

This project is open source and available for learning purposes.

---

### 👤 About this project

Built as a hands-on learning project to practice full-stack development with ASP.NET Core — covering MVC architecture, SQL Server integration, session-based authentication, third-party email integration, and responsive UI design.
