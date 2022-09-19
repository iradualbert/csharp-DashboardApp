using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Dashboard.DB;

namespace Dashboard.Models
{
    public struct RevenueByDate
    {
        public string Date { get; set; }
        public decimal TotalAmount { get; set; }

    }
    public class Dashboard : DbConnection
    {
        private DateTime startDate;
        private DateTime endDate;
        private int numberDays;

        public int NumCustomers { get; private set; }
        public int NumSuppliers { get; private set; }
        public int NumProducts { get; private set; }
        public List<KeyValuePair<string, int>> TopProductsList { get; set; }
        public List<KeyValuePair<string, int>> UnderstockList { get; set; }
        public List<RevenueByDate> GrossRevenueList { get; set; }
        public int NumOrders { get; private set; }
        public decimal TotalRevenue { get; private set; }
        public decimal TotalProfit { get; private set; }


        public Dashboard() 
        {

        }


        private SqlCommand GetCommand()
        {
            var command = new SqlCommand();
            command.Connection = GetConnection();
            return command;
        }
        private void GetNumberItems()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    // Get Total Number of Customers
                    command.CommandText = "SELECT count(id) FROM Customer";
                    NumCustomers = Convert.ToInt32(command.ExecuteScalar());

                    // Get Total Number of Products
                    command.CommandText = "SELECT count(id) FROM Product";
                    NumProducts = Convert.ToInt32(command.ExecuteScalar());

                    // Get Total Number of Suppliers
                    command.CommandText = "SELECT count(id) FROM Supplier";
                    NumSuppliers = Convert.ToInt32(command.ExecuteScalar());

                    // Get total number of orders
                    command.CommandText = 
                        @"SELECT COUNT(id) FROM [Order] WHERE OrderDate BETWEEN @fromDate AND @toDate;";
                    command.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    command.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    NumOrders = (int)command.ExecuteScalar();


                }

                connection.Close();
            }
        }
        private void GetOrderAnalysis()
        {
            GrossRevenueList = new List<RevenueByDate>();
            TotalProfit = 0;
            TotalRevenue = 0;

            using( var command = GetCommand())
            {
                command.Connection.Open();


                command.CommandText =
                    "SELECT OrderDate, sum(TotalAmount) FROM [Order] WHERE OrderDate BETWEEN @fromDate AND @toDate GROUP BY OrderDate";

                command.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                command.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;

                var reader = command.ExecuteReader();

                var resultTable = new List<KeyValuePair<DateTime, decimal>>();

                while (reader.Read())
                {
                    resultTable.Add(
                         new KeyValuePair<DateTime, decimal>((DateTime)reader[0], (decimal)reader[1])
                        );

                    TotalRevenue += (decimal)reader[1];
                }

                TotalProfit = TotalRevenue * 0.2m;
                reader.Close();
                command.Connection.Close();

                //Group by Hours
                if (numberDays <= 1)
                {
                    GrossRevenueList = (from orderList in resultTable
                                        group orderList by orderList.Key.ToString("hh tt")
                                       into order
                                        select new RevenueByDate
                                        {
                                            Date = order.Key,
                                            TotalAmount = order.Sum(amount => amount.Value)
                                        }).ToList();
                }

                // Group by days 

                if (numberDays <= 30)
                {
                    foreach (var item in resultTable)
                    {
                        GrossRevenueList.Add(new RevenueByDate() 
                        { 
                            Date = item.Key.ToString("dd MM"),
                            TotalAmount = item.Value
                        });
                    }
                }

                //Group by Weeks
                else if (numberDays <= 92)
                {
                    GrossRevenueList = (from orderList in resultTable
                                        group orderList by CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                            orderList.Key, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                                       into order
                                        select new RevenueByDate
                                        {
                                            Date = "Week " + order.Key.ToString(),
                                            TotalAmount = order.Sum(amount => amount.Value)
                                        }).ToList();
                }
                //Group by Months
                else if (numberDays <= (365 * 2))
                {
                    bool isYear = numberDays <= 365 ? true : false;
                    GrossRevenueList = (from orderList in resultTable
                                        group orderList by orderList.Key.ToString("MMM yyyy")
                                       into order
                                        select new RevenueByDate
                                        {
                                            Date = isYear ? order.Key.Substring(0, order.Key.IndexOf(" ")) : order.Key,
                                            TotalAmount = order.Sum(amount => amount.Value)
                                        }).ToList();
                }
                //Group by Years
                else
                {
                    GrossRevenueList = (from orderList in resultTable
                                        group orderList by orderList.Key.ToString("yyyy")
                                       into order
                                        select new RevenueByDate
                                        {
                                            Date = order.Key,
                                            TotalAmount = order.Sum(amount => amount.Value)
                                        }).ToList();
                }


                command.Connection.Close();
            }
            
        }
        private void GetProductAnalysis()
        {
            TopProductsList = new List<KeyValuePair<string, int>>();
            UnderstockList = new List<KeyValuePair<string, int>>();
            
            using ( var connection = GetConnection())
            {
                connection.Open();

                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText =
                        @"select top 5 P.ProductName, sum(OrderItem.Quantity) as Q
                        from OrderItem
                        inner join Product P on P.Id = OrderItem.ProductId
                        inner
                        join [Order] O on O.Id = OrderItem.OrderId
                        where OrderDate between @fromDate and @toDate
                        group by P.ProductName
                         order by Q desc ";
                    command.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    command.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        TopProductsList.Add(
                            new KeyValuePair<string, int>(reader[0].ToString(), (int)reader[1]));
                    }
                    reader.Close();
                    //Get Understock
                    command.CommandText = @"select ProductName, Stock
                                            from Product
                                            where Stock <= 6 and IsDiscontinued = 0";
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        UnderstockList.Add(
                            new KeyValuePair<string, int>(reader[0].ToString(), (int)reader[1]));
                    }
                    reader.Close();
                }



                connection.Close();
            }


        }

        //Public methods
        public bool LoadData(DateTime startDate, DateTime endDate)
        {
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day,
                endDate.Hour, endDate.Minute, 59);
            if (startDate != this.startDate || endDate != this.endDate)
            {
                this.startDate = startDate;
                this.endDate = endDate;
                this.numberDays = (endDate - startDate).Days;
                GetNumberItems();
                GetProductAnalysis();
                GetOrderAnalysis();
                Console.WriteLine("Refreshed data: {0} - {1}", startDate.ToString(), endDate.ToString());
                return true;
            }
            else
            {
                Console.WriteLine("Data not refreshed, same query: {0} - {1}", startDate.ToString(), endDate.ToString());
                return false;
            }
        }


    }
}
