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
    public partial class ReportsForm : Form
    {
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnGenerate;
        private DataGridView dataGridView;
        private Button btnExport;

        public ReportsForm()
        {
            InitializeComponents();
            this.Text = "Финансовые отчеты";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponents()
        {
            var lblFrom = new Label { Text = "С:", Location = new Point(20, 20), Size = new Size(30, 20) };
            dtpFrom = new DateTimePicker { Location = new Point(60, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };

            var lblTo = new Label { Text = "По:", Location = new Point(200, 20), Size = new Size(30, 20) };
            dtpTo = new DateTimePicker { Location = new Point(240, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };

            btnGenerate = new Button { Text = "Сформировать отчет", Location = new Point(380, 20), Size = new Size(150, 20) };
            btnGenerate.Click += BtnGenerate_Click;

            dataGridView = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(840, 450),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true
            };

            btnExport = new Button { Text = "Экспорт в Excel", Location = new Point(550, 20), Size = new Size(120, 20) };
            btnExport.Click += BtnExport_Click;

            this.Controls.Add(lblFrom);
            this.Controls.Add(dtpFrom);
            this.Controls.Add(lblTo);
            this.Controls.Add(dtpTo);
            this.Controls.Add(btnGenerate);
            this.Controls.Add(dataGridView);
            this.Controls.Add(btnExport);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (dtpFrom.Value > dtpTo.Value)
            {
                MessageBox.Show("Дата 'С' не может быть больше даты 'По'", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand(
                        @"SELECT 
                        p.PaymentDate AS 'Дата',
                        CASE 
                            WHEN c.CompanyName IS NOT NULL THEN c.CompanyName
                            ELSE pc.LastName + ' ' + pc.FirstName + ' ' + pc.MiddleName
                        END AS 'Клиент',
                        p.Amount AS 'Сумма',
                        p.DocumentNumber AS 'Номер документа',
                        p.Comments AS 'Назначение платежа'
                      FROM Payments p
                      LEFT JOIN Contracts co ON p.ContractId = co.Id
                      LEFT JOIN LegalClients c ON co.ClientId = c.Id AND co.ClientType = 'Legal'
                      LEFT JOIN PhysicalClients pc ON co.ClientId = pc.Id AND co.ClientType = 'Physical'
                      WHERE p.PaymentDate BETWEEN @FromDate AND @ToDate
                      ORDER BY p.PaymentDate",
                        connection);

                    command.Parameters.AddWithValue("@FromDate", dtpFrom.Value.Date);
                    command.Parameters.AddWithValue("@ToDate", dtpTo.Value.Date);

                    var adapter = new SqlDataAdapter(command);
                    var table = new DataTable();
                    adapter.Fill(table);

                    dataGridView.DataSource = table;

                    // Добавляем итоговую строку
                    var total = table.AsEnumerable().Sum(row => row.Field<decimal>("Сумма"));
                    dataGridView.Rows.Add();
                    dataGridView.Rows[dataGridView.Rows.Count - 1].Cells["Сумма"].Value = total;
                    dataGridView.Rows[dataGridView.Rows.Count - 1].Cells["Клиент"].Value = "ИТОГО:";
                    dataGridView.Rows[dataGridView.Rows.Count - 1].DefaultCellStyle.Font = new Font(dataGridView.Font, FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dataGridView.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Сохранить отчет",
                    FileName = $"Финансовый отчет {DateTime.Now:yyyy-MM-dd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Использование библиотеки EPPlus или аналогичной для экспорта в Excel
                    MessageBox.Show("Экспорт в Excel будет реализован в следующей версии", "Информация",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}