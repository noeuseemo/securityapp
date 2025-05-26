using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace securityapptest3
{
    public partial class ContractsForm : Form
    {
        private DataGridView dataGridView;
        private Button btnAddPayment;
        private Button btnPrintContract;
        private Button btnClose;

        private int clientId;
        private string clientType;
        private string clientName;

        public ContractsForm(int clientId, string clientType, string clientName)
        {
            this.clientId = clientId;
            this.clientType = clientType;
            this.clientName = clientName;

            InitializeComponents();
            this.Text = $"Договоры клиента: {clientName}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            LoadContracts();
        }

        private void InitializeComponents()
        {
            dataGridView = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(740, 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            btnAddPayment = new Button
            {
                Text = "Добавить платеж",
                Location = new Point(20, 390),
                Size = new Size(150, 30)
            };
            btnAddPayment.Click += BtnAddPayment_Click;

            btnPrintContract = new Button
            {
                Text = "Печать договора",
                Location = new Point(190, 390),
                Size = new Size(150, 30)
            };
            btnPrintContract.Click += BtnPrintContract_Click;

            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(360, 390),
                Size = new Size(150, 30)
            };
            btnClose.Click += (sender, e) => this.Close();

            this.Controls.Add(dataGridView);
            this.Controls.Add(btnAddPayment);
            this.Controls.Add(btnPrintContract);
            this.Controls.Add(btnClose);
        }

        private void LoadContracts()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand(
                        @"SELECT 
                        c.Id, 
                        c.ContractNumber AS 'Номер договора',
                        c.ContractDate AS 'Дата договора',
                        c.EndDate AS 'Дата окончания',
                        SUM(p.Amount) AS 'Оплачено',
                        c.TotalAmount AS 'Сумма договора',
                        CASE 
                            WHEN c.TotalAmount IS NULL THEN 'Бессрочный'
                            WHEN SUM(p.Amount) >= c.TotalAmount THEN 'Оплачен'
                            ELSE 'Не оплачен'
                        END AS 'Статус'
                      FROM Contracts c
                      LEFT JOIN Payments p ON c.Id = p.ContractId
                      WHERE c.ClientId = @ClientId AND c.ClientType = @ClientType
                      GROUP BY c.Id, c.ContractNumber, c.ContractDate, c.EndDate, c.TotalAmount",
                        connection);

                    command.Parameters.AddWithValue("@ClientId", clientId);
                    command.Parameters.AddWithValue("@ClientType", clientType);

                    var adapter = new SqlDataAdapter(command);
                    var table = new DataTable();
                    adapter.Fill(table);

                    dataGridView.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки договоров: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddPayment_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите договор для добавления платежа", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var contractId = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;
            var form = new PaymentForm(contractId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadContracts();
            }
        }

        private void BtnPrintContract_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите договор для печати", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var contractId = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Получаем данные договора
                    var command = new SqlCommand(
                        @"SELECT 
                    c.ContractNumber,
                    c.ContractDate,
                    c.EndDate,
                    c.TotalAmount,
                    c.Comments,
                    CASE 
                        WHEN c.ClientType = 'Legal' THEN l.CompanyName
                        ELSE p.LastName + ' ' + p.FirstName + ' ' + p.MiddleName
                    END AS ClientName,
                    CASE 
                        WHEN c.ClientType = 'Legal' THEN l.Address
                        ELSE p.Address
                    END AS ClientAddress
                  FROM Contracts c
                  LEFT JOIN LegalClients l ON c.ClientId = l.Id AND c.ClientType = 'Legal'
                  LEFT JOIN PhysicalClients p ON c.ClientId = p.Id AND c.ClientType = 'Physical'
                  WHERE c.Id = @ContractId",
                        connection);

                    command.Parameters.AddWithValue("@ContractId", contractId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Генерация документа договора
                            var contractNumber = reader["ContractNumber"].ToString();
                            var contractDate = ((DateTime)reader["ContractDate"]).ToString("dd.MM.yyyy");
                            var clientName = reader["ClientName"].ToString();
                            var clientAddress = reader["ClientAddress"].ToString();
                            var endDate = reader["EndDate"] != DBNull.Value ?
                                ((DateTime)reader["EndDate"]).ToString("dd.MM.yyyy") : "не определена";
                            var totalAmount = reader["TotalAmount"] != DBNull.Value ?
                                ((decimal)reader["TotalAmount"]).ToString("N2") + " руб." : "не определена";
                            var comments = reader["Comments"] != DBNull.Value ?
                                reader["Comments"].ToString() : string.Empty;

                            // Создаем HTML-документ для печати
                            var htmlContent = $@"
                    <html>
                    <head>
                        <title>Договор {contractNumber}</title>
                        <style>
                            body {{ font-family: Arial; margin: 20px; }}
                            h1 {{ text-align: center; font-size: 18pt; }}
                            .header {{ text-align: right; margin-bottom: 30px; }}
                            .parties {{ margin-bottom: 30px; }}
                            .party {{ margin-bottom: 15px; }}
                            .content {{ margin-bottom: 30px; }}
                            .signatures {{ display: flex; justify-content: space-between; margin-top: 50px; }}
                            .signature {{ width: 45%; }}
                            table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                            th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                            th {{ background-color: #f2f2f2; }}
                        </style>
                    </head>
                    <body>
                        <div class='header'>
                            <p>г. Москва</p>
                            <p>Дата: {contractDate}</p>
                        </div>
                        
                        <h1>ДОГОВОР № {contractNumber}</h1>
                        
                        <div class='parties'>
                            <div class='party'>
                                <strong>Охранное агентство «Security»</strong><br>
                                Адрес: г. Москва, ул. Охранная, д. 1<br>
                                ИНН 1234567890, ОГРН 1234567890123
                            </div>
                            
                            <div class='party'>
                                <strong>Клиент: {clientName}</strong><br>
                                Адрес: {clientAddress}
                            </div>
                        </div>
                        
                        <div class='content'>
                            <h2>1. Предмет договора</h2>
                            <p>Охранное агентство «Security» обязуется предоставлять охранные услуги, а Клиент обязуется оплачивать эти услуги в соответствии с условиями настоящего договора.</p>
                            
                            <h2>2. Условия оказания услуг</h2>
                            <p>Дата начала действия договора: {contractDate}</p>
                            <p>Дата окончания действия договора: {endDate}</p>
                            <p>Общая сумма договора: {totalAmount}</p>
                            
                            <h2>3. Особые условия</h2>
                            <p>{comments}</p>
                            
                            <h2>4. Платежи по договору</h2>
                            <table>
                                <tr>
                                    <th>Дата платежа</th>
                                    <th>Сумма</th>
                                    <th>Документ</th>
                                </tr>";

                            // Добавляем информацию о платежах
                            command = new SqlCommand(
                                @"SELECT PaymentDate, Amount, DocumentNumber 
                          FROM Payments 
                          WHERE ContractId = @ContractId
                          ORDER BY PaymentDate",
                                connection);
                            command.Parameters.AddWithValue("@ContractId", contractId);

                            using (var paymentsReader = command.ExecuteReader())
                            {
                                while (paymentsReader.Read())
                                {
                                    htmlContent += $@"
                                <tr>
                                    <td>{((DateTime)paymentsReader["PaymentDate"]).ToString("dd.MM.yyyy")}</td>
                                    <td>{((decimal)paymentsReader["Amount"]).ToString("N2")} руб.</td>
                                    <td>{paymentsReader["DocumentNumber"].ToString()}</td>
                                </tr>";
                                }
                            }

                            htmlContent += $@"
                            </table>
                        </div>
                        
                        <div class='signatures'>
                            <div class='signature'>
                                <p>___________________________</p>
                                <p>Директор ОА «Security»</p>
                                <p>Иванов И.И.</p>
                            </div>
                            
                            <div class='signature'>
                                <p>___________________________</p>
                                <p>{clientName}</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                            // Сохраняем HTML во временный файл
                            var tempFile = Path.GetTempFileName() + ".html";
                            File.WriteAllText(tempFile, htmlContent);

                            // Открываем в браузере для печати
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = tempFile,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подготовке договора к печати: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}