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
    public partial class EmployeesForm : Form
    {
        private DataGridView dataGridView;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;

        public EmployeesForm()
        {
            InitializeComponents();
            this.Text = "Управление сотрудниками";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            LoadEmployees();
        }

        private void InitializeComponents()
        {
            dataGridView = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(740, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            btnAdd = new Button
            {
                Text = "Добавить",
                Location = new Point(20, 440),
                Size = new Size(100, 30)
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Редактировать",
                Location = new Point(140, 440),
                Size = new Size(100, 30)
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Удалить",
                Location = new Point(260, 440),
                Size = new Size(100, 30)
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(380, 440),
                Size = new Size(100, 30)
            };
            btnRefresh.Click += (sender, e) => LoadEmployees();

            this.Controls.Add(dataGridView);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRefresh);
        }

        private void LoadEmployees()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT * FROM Employees", connection);
                    var adapter = new SqlDataAdapter(command);
                    var table = new DataTable();
                    adapter.Fill(table);

                    dataGridView.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new EmployeeEditForm(null);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadEmployees();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника для редактирования", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var id = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;
            var form = new EmployeeEditForm(id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadEmployees();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника для удаления", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранного сотрудника?", "Подтверждение",
                              MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var id = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;

                try
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("DELETE FROM Employees WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Сотрудник успешно удален", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadEmployees();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления сотрудника: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}