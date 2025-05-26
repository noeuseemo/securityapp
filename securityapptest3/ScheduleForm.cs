using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace securityapptest3
{
    public partial class ScheduleForm : Form
    {
        private DataGridView dataGridView;
        private MonthCalendar monthCalendar;
        private Button btnGenerate;
        private Button btnSave;
        private Button btnPrint;

        public ScheduleForm()
        {
            InitializeComponents();
            this.Text = "График дежурств";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            LoadGuards();
        }

        private void InitializeComponents()
        {
            monthCalendar = new MonthCalendar
            {
                Location = new Point(20, 20),
                MaxSelectionCount = 1
            };
            monthCalendar.DateChanged += (sender, e) => LoadSchedule(e.Start);

            dataGridView = new DataGridView
            {
                Location = new Point(300, 20),
                Size = new Size(650, 450),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = false,
                AllowUserToAddRows = false
            };

            btnGenerate = new Button
            {
                Text = "Сгенерировать график",
                Location = new Point(20, 200),
                Size = new Size(250, 30)
            };
            btnGenerate.Click += BtnGenerate_Click;

            btnSave = new Button
            {
                Text = "Сохранить изменения",
                Location = new Point(20, 240),
                Size = new Size(250, 30)
            };
            btnSave.Click += BtnSave_Click;

            btnPrint = new Button
            {
                Text = "Печать графика",
                Location = new Point(20, 280),
                Size = new Size(250, 30)
            };
            btnPrint.Click += BtnPrint_Click;

            this.Controls.Add(monthCalendar);
            this.Controls.Add(dataGridView);
            this.Controls.Add(btnGenerate);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnPrint);
        }

        private void LoadGuards()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "SELECT Id, LastName + ' ' + FirstName + ' ' + MiddleName AS FullName FROM Employees WHERE Position = 'Охранник'",
                        connection);

                    var adapter = new SqlDataAdapter(command);
                    var table = new DataTable();
                    adapter.Fill(table);

                    // Настройка DataGridView
                    dataGridView.Columns.Clear();
                    dataGridView.DataSource = table;

                    // Добавляем колонку для выбора дежурства
                    var dutyColumn = new DataGridViewCheckBoxColumn
                    {
                        Name = "OnDuty",
                        HeaderText = "Дежурит"
                    };
                    dataGridView.Columns.Add(dutyColumn);

                    // Настраиваем отображение
                    dataGridView.Columns["Id"].Visible = false;
                    dataGridView.Columns["FullName"].ReadOnly = true;
                    dataGridView.Columns["FullName"].HeaderText = "Охранник";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки охранников: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSchedule(DateTime date)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "SELECT EmployeeId FROM Schedule WHERE Date = @Date",
                        connection);
                    command.Parameters.AddWithValue("@Date", date.Date);

                    var onDutyIds = new List<int>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            onDutyIds.Add(reader.GetInt32(0));
                        }
                    }

                    // Отмечаем дежурных в DataGridView
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        var employeeId = (int)row.Cells["Id"].Value;
                        row.Cells["OnDuty"].Value = onDutyIds.Contains(employeeId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки графика: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Сгенерировать график дежурств на месяц? Существующие данные будут перезаписаны.",
                              "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    var startDate = new DateTime(monthCalendar.SelectionStart.Year, monthCalendar.SelectionStart.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);

                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();

                        // Получаем список охранников
                        var command = new SqlCommand(
                            "SELECT Id FROM Employees WHERE Position = 'Охранник'",
                            connection);

                        var guardIds = new List<int>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                guardIds.Add(reader.GetInt32(0));
                            }
                        }

                        if (guardIds.Count == 0)
                        {
                            MessageBox.Show("Нет охранников в базе данных", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Удаляем старый график за этот период
                        command = new SqlCommand(
                            "DELETE FROM Schedule WHERE Date BETWEEN @StartDate AND @EndDate",
                            connection);
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);
                        command.ExecuteNonQuery();

                        // Генерируем новый график (пример алгоритма - каждый охранник дежурит через 2 дня)
                        int guardIndex = 0;
                        for (var date = startDate; date <= endDate; date = date.AddDays(1))
                        {
                            command = new SqlCommand(
                                "INSERT INTO Schedule (EmployeeId, Date) VALUES (@EmployeeId, @Date)",
                                connection);
                            command.Parameters.AddWithValue("@EmployeeId", guardIds[guardIndex]);
                            command.Parameters.AddWithValue("@Date", date);
                            command.ExecuteNonQuery();

                            guardIndex = (guardIndex + 1) % guardIds.Count;
                        }

                        MessageBox.Show("График дежурств успешно сгенерирован", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSchedule(monthCalendar.SelectionStart);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка генерации графика: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var date = monthCalendar.SelectionStart.Date;

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Удаляем старые записи на эту дату
                    var command = new SqlCommand(
                        "DELETE FROM Schedule WHERE Date = @Date",
                        connection);
                    command.Parameters.AddWithValue("@Date", date);
                    command.ExecuteNonQuery();

                    // Добавляем новые записи
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (Convert.ToBoolean(row.Cells["OnDuty"].Value))
                        {
                            command = new SqlCommand(
                                "INSERT INTO Schedule (EmployeeId, Date) VALUES (@EmployeeId, @Date)",
                                connection);
                            command.Parameters.AddWithValue("@EmployeeId", row.Cells["Id"].Value);
                            command.Parameters.AddWithValue("@Date", date);
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Изменения сохранены", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            // Создание PDF или печать графика
            MessageBox.Show("Функция печати будет реализована в следующей версии", "Информация",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}