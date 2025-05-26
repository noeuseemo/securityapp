using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using MongoDB.Driver.Core.Configuration;

namespace securityapptest3
{
    class DatabaseHelper
    {
        public static string connectionString = "Server=217.71.129.139,5203;Encrypt=False;Database=Kapustin;User ID=ip22;Password=ip22_1";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
        public static void InitializeDatabase()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    var command = new SqlCommand(
                        @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
                      BEGIN
                          CREATE TABLE Employees (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              LastName NVARCHAR(50) NOT NULL,
                              FirstName NVARCHAR(50) NOT NULL,
                              MiddleName NVARCHAR(50),
                              Address NVARCHAR(200),
                              Position NVARCHAR(50) NOT NULL,
                              Salary DECIMAL(18,2) NOT NULL,
                              WeaponBrand NVARCHAR(50),
                              WeaponNumber NVARCHAR(50),
                              SpecialEquipment NVARCHAR(200),
                              CertificateNumber NVARCHAR(50),
                              LicenseNumber NVARCHAR(50),
                              INN NVARCHAR(12),
                              PFR NVARCHAR(14),
                              HireDate DATETIME DEFAULT GETDATE(),
                              DismissalDate DATETIME NULL
                          );
                      END
                      
                      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LegalClients')
                      BEGIN
                          CREATE TABLE LegalClients (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              CompanyName NVARCHAR(100) NOT NULL,
                              Address NVARCHAR(200) NOT NULL
                          );
                      END
                      
                      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PhysicalClients')
                      BEGIN
                          CREATE TABLE PhysicalClients (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              LastName NVARCHAR(50) NOT NULL,
                              FirstName NVARCHAR(50) NOT NULL,
                              MiddleName NVARCHAR(50),
                              Address NVARCHAR(200) NOT NULL,
                              PassportData NVARCHAR(100) NOT NULL
                          );
                      END
                      
                      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contracts')
                      BEGIN
                          CREATE TABLE Contracts (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              ClientId INT NOT NULL,
                              ClientType NVARCHAR(10) NOT NULL CHECK (ClientType IN ('Legal', 'Physical')),
                              ContractNumber NVARCHAR(20) NOT NULL,
                              ContractDate DATETIME NOT NULL,
                              EndDate DATETIME,
                              TotalAmount DECIMAL(18,2),
                              Comments NVARCHAR(500)
                          );
                      END
                      
                      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
                      BEGIN
                          CREATE TABLE Payments (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              ContractId INT NOT NULL,
                              PaymentDate DATETIME NOT NULL,
                              Amount DECIMAL(18,2) NOT NULL,
                              DocumentNumber NVARCHAR(50) NOT NULL,
                              Comments NVARCHAR(200),
                              FOREIGN KEY (ContractId) REFERENCES Contracts(Id)
                          );
                      END
                      
                      IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Schedule')
                      BEGIN
                          CREATE TABLE Schedule (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              EmployeeId INT NOT NULL,
                              Date DATETIME NOT NULL,
                              ReplacementReason NVARCHAR(200),
                              FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
                          );
                      END",
                        connection);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Остальные методы класса DatabaseHelper остаются без изменений
        // ...
    }
}
