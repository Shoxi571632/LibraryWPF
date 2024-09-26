using Ado.Net.Server;
using Dapper;
using Npgsql;
using System.Windows;
using System.Windows.Controls;

namespace AdoNetWpf
{
    public partial class MainWindow : Window
    {
        private NpgsqlConnection _connection;

        public MainWindow()
        {
            InitializeComponent();
            UlanishOrnatish(); // Ma'lumotlar bazasiga ulanish
        }

        private void UlanishOrnatish()
        {
            try
            {
                string connectionString = "Host=localhost;Port=2208;Database=4-modul;Username=postgres;Password=2208";
                _connection = new NpgsqlConnection(connectionString);
                _connection.Open();
                JadvalYuklash(); // Jadvallarni yuklash
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ma'lumotlar bazasiga ulanishda xatolik: {ex.Message}");
            }
        }

        private void JadvalYuklash()
        {
            try
            {
                var tables = DataViewer.ViewTable(_connection);
                TablesComboBox.ItemsSource = tables;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Jadvallarni yuklashda xatolik: {ex.Message}");
            }
        }

        private void TablesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TablesComboBox.SelectedItem != null)
            {
                string selectedTable = TablesComboBox.SelectedItem.ToString();
                JadvalMalumotlariniYuklash(selectedTable);
            }
        }

        private void JadvalMalumotlariniYuklash(string tableName)
        {
            try
            {
                var data = DataViewer.ViewTableData(_connection, tableName);
                DataGridTableData.ItemsSource = data.Select(row => new { Data = row }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ma'lumotlarni yuklashda xatolik: {ex.Message}");
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedTable = TablesComboBox.SelectedItem.ToString();
                string[] values = { Column1TextBox.Text, Column2TextBox.Text }; // Qo'shmoqchi bo'lgan ustunlar
                DataViewer.InsertData(_connection, selectedTable, values);
                JadvalMalumotlariniYuklash(selectedTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ma'lumot qo'shishda xatolik: {ex.Message}");
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            int oldId;
            int newId;
            string columnName = UpdateColumnTextBox.Text;
            string newValue = UpdateValueTextBox.Text;

            // Foydalanuvchi tomonidan kiritilgan eski ID va yangi ID ni olish
            if (int.TryParse(UpdateIdTextBox.Text, out oldId) && int.TryParse(NewIdTextBox.Text, out newId))
            {
                try
                {
                    // SQL so'rovini tayyorlash
                    string query = $"UPDATE {TablesComboBox.SelectedItem} SET {columnName} = @newValue, id = @newId WHERE id = @oldId";

                    // Ma'lumotlarni yangilash
                    _connection.Execute(query, new { newId, newValue, oldId });

                    // Ma'lumotlar yangilangandan keyin jadvalni yangilash
                    JadvalMalumotlariniYuklash(TablesComboBox.SelectedItem.ToString());
        
            MessageBox.Show("Ma'lumot muvaffaqiyatli yangilandi!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Yangilashda xatolik: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Iltimos, ID larni to'g'ri kiritganingizga ishonch hosil qiling.");
            }
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedTable = TablesComboBox.SelectedItem.ToString();
                int id = int.Parse(DeleteIdTextBox.Text);
                DataViewer.DeleteData(_connection, selectedTable, id);
                JadvalMalumotlariniYuklash(selectedTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ma'lumotni o'chirishda xatolik: {ex.Message}");
            }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Name) // Tekshiramiz, agar joylanadigan matn hali ko'rsatilgan bo'lsa
            {
                textBox.Text = string.Empty; // Foydalanuvchi matn kiritganida, joylanadigan matnni tozalaymiz
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text)) // Agar foydalanuvchi hech qanday matn kiritmasa
            {
                textBox.Text = textBox.Name; // Qayta joylanadigan matnni qo'shamiz
            }
        }

    }
}
