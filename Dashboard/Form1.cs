namespace Dashboard
{
    public partial class DashboardForm : Form
    {

        // Fields 
        private Models.Dashboard model;

        // constructor 
        public DashboardForm()
        {
            InitializeComponent();
            model = new Models.Dashboard();

            this.DisableDatePicker();
            dtpStartDate.Value = DateTime.Now.AddDays(-200);
            dtpEndDate.Value = DateTime.Now;
            btnLast7Days.Select();
            this.LoadData();
        }

        private void LoadData()
        {
           bool refreshData = model.LoadData(this.dtpStartDate.Value, this.dtpEndDate.Value);
            if (refreshData)
            {
                lblNumOrders.Text = model.NumOrders.ToString();
                lblTotalRevenue.Text = "$" + model.TotalRevenue.ToString();
                lblTotalProfit.Text = "$" + model.TotalProfit.ToString();

                lblNumCustomers.Text = model.NumCustomers.ToString();
                lblNumProducts.Text = model.NumProducts.ToString();
                lblNumSuppliers.Text = model.NumSuppliers.ToString();

                dgvUnderstock.DataSource = model.UnderstockList;
                dgvUnderstock.Columns[0].HeaderText = "Items";
                dgvUnderstock.Columns[1].HeaderText = "Units";
            } 
            else
            {

            }
           
        }

        private void DisableDatePicker ()
        {
            dtpStartDate.Enabled = false;
            dtpEndDate.Enabled = false;
            btnOkCustomDate.Visible = false;
        }

        private void EnableDatePicker ()
        {
            dtpStartDate.Enabled = true;
            dtpEndDate.Enabled = true;
            btnOkCustomDate.Visible = true;
        }

        private void btnThisMonth_Click(object sender, EventArgs e)
        {
            this.DisableDatePicker();
            dtpStartDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpEndDate.Value = DateTime.Now;
            this.LoadData();

        }

        private void btnToday_Click(object sender, EventArgs e)
        {
            this.DisableDatePicker();
            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Now;
            this.LoadData();
            
        }

        private void btnLast30Days_Click(object sender, EventArgs e)
        {
            this.DisableDatePicker();
            dtpStartDate.Value = DateTime.Now.AddDays(-30);
            dtpEndDate.Value = DateTime.Now;
            this.LoadData();
        }

        private void btnLast7Days_Click(object sender, EventArgs e)
        {
            this.DisableDatePicker();
            dtpStartDate.Value = DateTime.Now.AddDays(-7);
            dtpEndDate.Value = DateTime.Now;
            btnLast7Days.Select();
            this.LoadData();
        }

        private void btnCustomDate_Click(object sender, EventArgs e)
        {
            this.EnableDatePicker();
            btnCustomDate.Select();
        }

        private void btnOkCustomDate_Click(object sender, EventArgs e)
        {
            this.LoadData();
            btnCustomDate.Select();
        }
    }
}