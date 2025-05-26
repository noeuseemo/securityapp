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
    public partial class LegalClientsForm : Form
    {
        private DataGridView dataGridView;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnContracts;

        public LegalClientsForm()
        {
            InitializeComponents();
            this.Text = "Клиенты (юридические лица)";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            LoadClients();
        }

        private void InitializeComponents()
        {
            dataGridView = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(840, 400),
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
            btnRefresh.Click += (sender, e) => LoadClients();

            btnContracts = new Button
            {
                Text = "Договоры",
                Location = new Point(500, 440),
                Size = new Size(100, 30)
            };
            btnContracts.Click += BtnContracts_Click;

            this.Controls.Add(dataGridView);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(btnContracts);
        }

        private void LoadClients()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand(
                        @"SELECT c.Id, c.CompanyName, c.Address, 
                             co.ContractNumber, co.ContractDate, co.EndDate
                      FROM LegalClients c
                      LEFT JOIN Contracts co ON c.Id = co.ClientId AND co.ClientType = 'Legal'",
                        connection);

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
            var form = new LegalClientEditForm(null);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadClients();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) return;

            var id = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;
            var form = new LegalClientEditForm(id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadClients();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) return;

            var id = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;

            if (MessageBox.Show("Удалить выбранного клиента?", "Подтверждение",
                              MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand(
                            "DELETE FROM LegalClients WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Клиент удален", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadClients();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnContracts_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) return;

            var clientId = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;
            var companyName = dataGridView.SelectedRows[0].Cells["CompanyName"].ToString();
            var form = new ContractsForm(clientId, "Legal", companyName);
            form.ShowDialog();
        }
    }
}