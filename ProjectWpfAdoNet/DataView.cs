using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;

namespace Ado.Net.Server
{
    public static class DataViewer
    {
        public static List<string> ViewTable(NpgsqlConnection connection)
        {
            // Jadval nomlarini ko'rsatish
            var tables = connection.Query<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'").ToList();
            return tables;
        }

        public static List<string> ViewTableData(NpgsqlConnection connection, string tableName)
        {
            // Ustun nomlarini olish
            var columns = connection.Query<string>($"SELECT column_name FROM information_schema.columns WHERE table_name = '{tableName}'");
            var columnNames = string.Join(", ", columns);

            // Tanlangan jadvaldan ma'lumotlarni olish
            string query = $"SELECT {columnNames} FROM {tableName}";
            var data = connection.Query(query).ToList();

            List<string> result = new List<string>();
            foreach (var row in data)
            {
                result.Add(string.Join(", ", ((IDictionary<string, object>)row).Values));
            }

            return result;
        }

        public static void InsertData(NpgsqlConnection connection, string tableName, string[] columnValues)
        {
            // Tanlangan jadvalga ma'lumot qo'shish
            var columns = connection.Query<string>($"SELECT column_name FROM information_schema.columns WHERE table_name = '{tableName}'").ToList();
            var columnNames = string.Join(", ", columns.Skip(1));  // ID ni inobatga olmaslik uchun Skip(1)

            var values = string.Join(", ", columnValues.Select(v => $"'{v}'"));

            string insertQuery = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values})";
            connection.Execute(insertQuery);
        }

        public static void UpdateData(NpgsqlConnection connection, string tableName, int id, string columnName, string newValue)
        {
            // Ma'lumotni yangilash
            string updateQuery = $"UPDATE {tableName} SET {columnName} = '{newValue}' WHERE id = {id}";
            connection.Execute(updateQuery);
        }

        public static void DeleteData(NpgsqlConnection connection, string tableName, int id)
        {
            // Ma'lumotni o'chirish
            string deleteQuery = $"DELETE FROM {tableName} WHERE id = {id}";
            connection.Execute(deleteQuery);
        }
    }
}
