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
    public partial class LegalClientEditForm : Form
    {
        private TextBox txtCompanyName;
        private TextBox txtAddress;
        private TextBox txtContractNumber;
        private DateTimePicker dtpContractDate;
        private DateTimePicker dtpEndDate;
        private Button btnSave;
        private Button btnCancel;

        private int? clientId;

        public LegalClientEditForm(int? id)
        {
            clientId = id;
            InitializeComponents();
            this.Text = clientId.HasValue ? "Редактирование клиента" : "Новый клиент";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            if (clientId.HasValue)
            {
                LoadClientData();
            }
        }

        private void InitializeComponents()
        {
            var lblCompanyName = new Label { Text = "Название компании:", Location = new Point(20, 20), Size = new Size(150, 20) };
            txtCompanyName = new TextBox { Location = new Point(180, 20), Size = new Size(280, 20) };

            var lblAddress = new Label { Text = "Адрес:", Location = new Point(20, 50), Size = new Size(150, 20) };
            txtAddress = new TextBox { Location = new Point(180, 50), Size = new Size(280, 20) };

            var lblContract = new Label { Text = "Данные договора:", Location = new Point(20, 80), Size = new Size(150, 20), Font = new Font(Font, FontStyle.Bold) };

            var lblContractNumber = new Label { Text = "Номер договора:", Location = new Point(40, 110), Size = new Size(130, 20) };
            txtContractNumber = new TextBox { Location = new Point(180, 110), Size = new Size(150, 20) };

            var lblContractDate = new Label { Text = "Дата договора:", Location = new Point(40, 140), Size = new Size(130, 20) };
            dtpContractDate = new DateTimePicker { Location = new Point(180, 140), Size = new Size(150, 20), Format = DateTimePickerFormat.Short };

            var lblEndDate = new Label { Text = "Дата окончания:", Location = new Point(40, 170), Size = new Size(130, 20) };
            dtpEndDate = new DateTimePicker { Location = new Point(180, 170), Size = new Size(150, 20), Format = DateTimePickerFormat.Short };

            btnSave = new Button { Text = "Сохранить", Location = new Point(180, 220), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(300, 220), Size = new Size(100, 30) };
            btnCancel.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
            lblCompanyName, txtCompanyName,
            lblAddress, txtAddress,
            lblContract,
            lblContractNumber, txtContractNumber,
            lblContractDate, dtpContractDate,
            lblEndDate, dtpEndDate,
            btnSave, btnCancel
        });
        }

        private void LoadClientData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand(
                        @"SELECT c.*, co.ContractNumber, co.ContractDate, co.EndDate
                      FROM LegalClients c
                      LEFT JOIN Contracts co ON c.Id = co.ClientId AND co.ClientType = 'Legal'
                      WHERE c.Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", clientId.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtCompanyName.Text = reader["CompanyName"].ToString();
                            txtAddress.Text = reader["Address"].ToString();

                            if (reader["ContractNumber"] != DBNull.Value)
                            {
                                txtContractNumber.Text = reader["ContractNumber"].ToString();
                                dtpContractDate.Value = (DateTime)reader["ContractDate"];
                                dtpEndDate.Value = (DateTime)reader["EndDate"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("Название компании обязательно для заполнения", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    SqlCommand command;

                    if (clientId.HasValue)
                    {
                        command = new SqlCommand(
                            @"UPDATE LegalClients SET 
                            CompanyName = @CompanyName, 
                            Address = @Address
                          WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", clientId.Value);
                    }
                    else
                    {
                        command = new SqlCommand(
                            @"INSERT INTO LegalClients (CompanyName, Address)
                          VALUES (@CompanyName, @Address);
                          SELECT SCOPE_IDENTITY();", connection);
                    }

                    command.Parameters.AddWithValue("@CompanyName", txtCompanyName.Text);
                    command.Parameters.AddWithValue("@Address", txtAddress.Text);

                    var newId = clientId.HasValue ?
                        command.ExecuteNonQuery() :
                        Convert.ToInt32(command.ExecuteScalar());

                    // Сохранение данных договора
                    if (!string.IsNullOrWhiteSpace(txtContractNumber.Text))
                    {
                        command = new SqlCommand(
                            @"IF EXISTS (SELECT 1 FROM Contracts WHERE ClientId = @ClientId AND ClientType = 'Legal')
                            UPDATE Contracts SET
                                ContractNumber = @ContractNumber,
                                ContractDate = @ContractDate,
                                EndDate = @EndDate
                            WHERE ClientId = @ClientId AND ClientType = 'Legal'
                          ELSE
                            INSERT INTO Contracts (ClientId, ClientType, ContractNumber, ContractDate, EndDate)
                            VALUES (@ClientId, 'Legal', @ContractNumber, @ContractDate, @EndDate)", connection);

                        command.Parameters.AddWithValue("@ClientId", clientId ?? newId);
                        command.Parameters.AddWithValue("@ContractNumber", txtContractNumber.Text);
                        command.Parameters.AddWithValue("@ContractDate", dtpContractDate.Value);
                        command.Parameters.AddWithValue("@EndDate", dtpEndDate.Value);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Данные сохранены", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}