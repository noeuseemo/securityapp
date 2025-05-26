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
    public partial class PhysicalClientEditForm : Form
    {
        private TextBox txtLastName;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private TextBox txtAddress;
        private TextBox txtPassportData;
        private TextBox txtContractNumber;
        private DateTimePicker dtpContractDate;
        private DateTimePicker dtpEndDate;
        private Button btnSave;
        private Button btnCancel;

        private int? clientId;

        public PhysicalClientEditForm(int? id)
        {
            clientId = id;
            InitializeComponents();
            this.Text = clientId.HasValue ? "Редактирование клиента" : "Новый клиент (физ. лицо)";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;

            if (clientId.HasValue)
            {
                LoadClientData();
            }
        }

        private void InitializeComponents()
        {
            // ФИО
            var lblLastName = new Label { Text = "Фамилия:", Location = new Point(20, 20), Size = new Size(100, 20) };
            txtLastName = new TextBox { Location = new Point(130, 20), Size = new Size(300, 20) };

            var lblFirstName = new Label { Text = "Имя:", Location = new Point(20, 50), Size = new Size(100, 20) };
            txtFirstName = new TextBox { Location = new Point(130, 50), Size = new Size(300, 20) };

            var lblMiddleName = new Label { Text = "Отчество:", Location = new Point(20, 80), Size = new Size(100, 20) };
            txtMiddleName = new TextBox { Location = new Point(130, 80), Size = new Size(300, 20) };

            // Адрес и паспортные данные
            var lblAddress = new Label { Text = "Адрес:", Location = new Point(20, 110), Size = new Size(100, 20) };
            txtAddress = new TextBox { Location = new Point(130, 110), Size = new Size(300, 20) };

            var lblPassportData = new Label { Text = "Паспортные данные:", Location = new Point(20, 140), Size = new Size(100, 40) };
            txtPassportData = new TextBox { Location = new Point(130, 140), Size = new Size(300, 20) };

            // Данные договора
            var lblContract = new Label { Text = "Данные договора:", Location = new Point(20, 170), Size = new Size(150, 20), Font = new Font(Font, FontStyle.Bold) };

            var lblContractNumber = new Label { Text = "Номер договора:", Location = new Point(40, 200), Size = new Size(130, 20) };
            txtContractNumber = new TextBox { Location = new Point(180, 200), Size = new Size(150, 20) };

            var lblContractDate = new Label { Text = "Дата договора:", Location = new Point(40, 230), Size = new Size(130, 20) };
            dtpContractDate = new DateTimePicker { Location = new Point(180, 230), Size = new Size(150, 20), Format = DateTimePickerFormat.Short };

            var lblEndDate = new Label { Text = "Дата окончания:", Location = new Point(40, 260), Size = new Size(130, 20) };
            dtpEndDate = new DateTimePicker { Location = new Point(180, 260), Size = new Size(150, 20), Format = DateTimePickerFormat.Short };

            // Кнопки
            btnSave = new Button { Text = "Сохранить", Location = new Point(180, 290), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(300, 290), Size = new Size(100, 30) };
            btnCancel.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

            // Добавление элементов на форму
            this.Controls.AddRange(new Control[] {
            lblLastName, txtLastName,
            lblFirstName, txtFirstName,
            lblMiddleName, txtMiddleName,
            lblAddress, txtAddress,
            lblPassportData, txtPassportData,
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
                      FROM PhysicalClients c
                      LEFT JOIN Contracts co ON c.Id = co.ClientId AND co.ClientType = 'Physical'
                      WHERE c.Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", clientId.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtLastName.Text = reader["LastName"].ToString();
                            txtFirstName.Text = reader["FirstName"].ToString();
                            txtMiddleName.Text = reader["MiddleName"].ToString();
                            txtAddress.Text = reader["Address"].ToString();
                            txtPassportData.Text = reader["PassportData"].ToString();

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
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text) ||
                string.IsNullOrWhiteSpace(txtPassportData.Text))
            {
                MessageBox.Show("Фамилия, имя, адрес и паспортные данные обязательны для заполнения", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    SqlCommand command;
                    int newId; // Переменная перемещается сюда!

                    if (clientId.HasValue)
                    {
                        // Обновление существующего клиента
                        command = new SqlCommand(
                            @"UPDATE PhysicalClients SET 
                    LastName = @LastName, 
                    FirstName = @FirstName, 
                    MiddleName = @MiddleName,
                    Address = @Address,
                    PassportData = @PassportData
                  WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", clientId.Value);
                        newId = clientId.Value; // Присваиваем новое значение
                    }
                    else
                    {
                        // Добавление нового клиента
                        command = new SqlCommand(
                            @"INSERT INTO PhysicalClients (LastName, FirstName, MiddleName, Address, PassportData)
                  VALUES (@LastName, @FirstName, @MiddleName, @Address, @PassportData);
                  SELECT SCOPE_IDENTITY();", connection);

                        // Параметры команды сохраняются одинаково
                        command.Parameters.AddWithValue("@LastName", txtLastName.Text);
                        command.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                        command.Parameters.AddWithValue("@MiddleName", txtMiddleName.Text ?? string.Empty);
                        command.Parameters.AddWithValue("@Address", txtAddress.Text);
                        command.Parameters.AddWithValue("@PassportData", txtPassportData.Text);

                        object result = command.ExecuteScalar();
                        newId = Convert.ToInt32(result); // Получаем идентификатор новой записи
                    }

                    // Сохранение данных договора
                    if (!string.IsNullOrWhiteSpace(txtContractNumber.Text))
                    {
                        command = new SqlCommand(
                            @"IF EXISTS (SELECT 1 FROM Contracts WHERE ClientId = @ClientId AND ClientType = 'Physical')
                      UPDATE Contracts SET
                          ContractNumber = @ContractNumber,
                          ContractDate = @ContractDate,
                          EndDate = @EndDate
                      WHERE ClientId = @ClientId AND ClientType = 'Physical'
                    ELSE
                      INSERT INTO Contracts (ClientId, ClientType, ContractNumber, ContractDate, EndDate)
                      VALUES (@ClientId, 'Physical', @ContractNumber, @ContractDate, @EndDate)", connection);

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