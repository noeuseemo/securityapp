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
    public partial class PaymentForm : Form
    {
        private NumericUpDown numAmount;
        private DateTimePicker dtpPaymentDate;
        private TextBox txtDocumentNumber;
        private TextBox txtComments;
        private Button btnSave;
        private Button btnCancel;

        private int contractId;

        public PaymentForm(int contractId)
        {
            this.contractId = contractId;
            InitializeComponents();
            this.Text = "Добавление платежа";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponents()
        {
            var lblAmount = new Label { Text = "Сумма:", Location = new Point(20, 20), Size = new Size(100, 20) };
            numAmount = new NumericUpDown { Location = new Point(130, 20), Size = new Size(200, 20), Minimum = 1, Maximum = 10000000, DecimalPlaces = 2 };

            var lblPaymentDate = new Label { Text = "Дата платежа:", Location = new Point(20, 50), Size = new Size(100, 20) };
            dtpPaymentDate = new DateTimePicker { Location = new Point(130, 50), Size = new Size(200, 20), Format = DateTimePickerFormat.Short };

            var lblDocumentNumber = new Label { Text = "Номер документа:", Location = new Point(20, 80), Size = new Size(100, 20) };
            txtDocumentNumber = new TextBox { Location = new Point(130, 80), Size = new Size(200, 20) };

            var lblComments = new Label { Text = "Комментарий:", Location = new Point(20, 110), Size = new Size(100, 20) };
            txtComments = new TextBox { Location = new Point(130, 110), Size = new Size(200, 60), Multiline = true };

            btnSave = new Button { Text = "Сохранить", Location = new Point(130, 190), Size = new Size(100, 30) };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(240, 190), Size = new Size(100, 30) };
            btnCancel.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblAmount);
            this.Controls.Add(numAmount);
            this.Controls.Add(lblPaymentDate);
            this.Controls.Add(dtpPaymentDate);
            this.Controls.Add(lblDocumentNumber);
            this.Controls.Add(txtDocumentNumber);
            this.Controls.Add(lblComments);
            this.Controls.Add(txtComments);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDocumentNumber.Text))
            {
                MessageBox.Show("Номер документа обязателен для заполнения", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand(
                        @"INSERT INTO Payments (ContractId, PaymentDate, Amount, DocumentNumber, Comments)
                      VALUES (@ContractId, @PaymentDate, @Amount, @DocumentNumber, @Comments)",
                        connection);

                    command.Parameters.AddWithValue("@ContractId", contractId);
                    command.Parameters.AddWithValue("@PaymentDate", dtpPaymentDate.Value.Date);
                    command.Parameters.AddWithValue("@Amount", numAmount.Value);
                    command.Parameters.AddWithValue("@DocumentNumber", txtDocumentNumber.Text);
                    command.Parameters.AddWithValue("@Comments", txtComments.Text ?? string.Empty);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Платеж успешно добавлен", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения платежа: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
