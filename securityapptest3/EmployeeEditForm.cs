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
    public partial class EmployeeEditForm : Form
    {
        private TextBox txtLastName;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        // Другие поля...
        private Button btnSave;
        private Button btnCancel;

        private int? employeeId;

        public EmployeeEditForm(int? id)
        {
            employeeId = id;
            InitializeComponents();
            this.Text = employeeId.HasValue ? "Редактирование сотрудника" : "Добавление сотрудника";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            if (employeeId.HasValue)
            {
                LoadEmployeeData();
            }
        }

        private void InitializeComponents()
        {
            // Создание и настройка элементов управления
            var lblLastName = new Label { Text = "Фамилия:", Location = new Point(20, 20), Size = new Size(100, 20) };
            txtLastName = new TextBox { Location = new Point(130, 20), Size = new Size(300, 20) };

            var lblFirstName = new Label { Text = "Имя:", Location = new Point(20, 50), Size = new Size(100, 20) };
            txtFirstName = new TextBox { Location = new Point(130, 50), Size = new Size(300, 20) };

            var lblMiddleName = new Label { Text = "Отчество:", Location = new Point(20, 80), Size = new Size(100, 20) };
            txtMiddleName = new TextBox { Location = new Point(130, 80), Size = new Size(300, 20) };

            // Добавление других полей (адрес, должность, оклад и т.д.)

            btnSave = new Button { Text = "Сохранить", Location = new Point(130, 500), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(250, 500), Size = new Size(100, 30) };
            btnCancel.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

            // Добавление элементов на форму
            this.Controls.Add(lblLastName);
            this.Controls.Add(txtLastName);
            this.Controls.Add(lblFirstName);
            this.Controls.Add(txtFirstName);
            this.Controls.Add(lblMiddleName);
            this.Controls.Add(txtMiddleName);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void LoadEmployeeData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT * FROM Employees WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", employeeId.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtLastName.Text = reader["LastName"].ToString();
                            txtFirstName.Text = reader["FirstName"].ToString();
                            txtMiddleName.Text = reader["MiddleName"].ToString();
                            // Заполнение других полей
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных сотрудника: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Фамилия и имя обязательны для заполнения", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    SqlCommand command;

                    if (employeeId.HasValue)
                    {
                        // Обновление существующего сотрудника
                        command = new SqlCommand(
                            @"UPDATE Employees SET 
                            LastName = @LastName, 
                            FirstName = @FirstName, 
                            MiddleName = @MiddleName
                            /* другие поля */
                        WHERE Id = @Id", connection);

                        command.Parameters.AddWithValue("@Id", employeeId.Value);
                    }
                    else
                    {
                        // Добавление нового сотрудника
                        command = new SqlCommand(
                            @"INSERT INTO Employees 
                            (LastName, FirstName, MiddleName /* другие поля */) 
                        VALUES 
                            (@LastName, @FirstName, @MiddleName /* другие параметры */)", connection);
                    }

                    command.Parameters.AddWithValue("@LastName", txtLastName.Text);
                    command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                    command.Parameters.AddWithValue("@MiddleName", txtMiddleName.Text);
                    // Добавление параметров для других полей

                    command.ExecuteNonQuery();

                    MessageBox.Show("Данные сотрудника успешно сохранены", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
