using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace securityapptest3
{
    public partial class MainForm : Form
    {
        private Button btnEmployees;
        private Button btnLegalClients;
        private Button btnPhysicalClients;
        private Button btnSchedule;
        private Button btnReports;

        public MainForm()
        {
            InitializeComponents();
            this.Text = "Security Agency - Главное меню";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponents()
        {
            // Создаем кнопки
            btnEmployees = new Button
            {
                Text = "Сотрудники",
                Location = new Point(50, 50),
                Size = new Size(200, 50)
            };
            btnEmployees.Click += (sender, e) =>
            {
                var form = new EmployeesForm();
                form.ShowDialog();
            };

            btnLegalClients = new Button
            {
                Text = "Клиенты (юр. лица)",
                Location = new Point(50, 120),
                Size = new Size(200, 50)
            };
            btnLegalClients.Click += (sender, e) =>
            {
                var form = new LegalClientsForm();
                form.ShowDialog();
            };

            btnPhysicalClients = new Button
            {
                Text = "Клиенты (физ. лица)",
                Location = new Point(50, 190),
                Size = new Size(200, 50)
            };
            btnPhysicalClients.Click += (sender, e) =>
            {
                var form = new PhysicalClientsForm();
                form.ShowDialog();
            };

            btnSchedule = new Button
            {
                Text = "График дежурств",
                Location = new Point(300, 50),
                Size = new Size(200, 50)
            };
            btnSchedule.Click += (sender, e) =>
            {
                var form = new ScheduleForm();
                form.ShowDialog();
            };

            btnReports = new Button
            {
                Text = "Финансовые отчеты",
                Location = new Point(300, 120),
                Size = new Size(200, 50)
            };
            btnReports.Click += (sender, e) =>
            {
                var form = new ReportsForm();
                form.ShowDialog();
            };

            // Добавляем элементы на форму
            this.Controls.Add(btnEmployees);
            this.Controls.Add(btnLegalClients);
            this.Controls.Add(btnPhysicalClients);
            this.Controls.Add(btnSchedule);
            this.Controls.Add(btnReports);
        }
    }
}