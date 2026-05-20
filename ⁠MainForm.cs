// =================================================================
// اسم المشروع: نظام إدارة مطعم متكامل - المرحلة الثالثة (case 3)
// المطور: عبد الرحمن
// التقنيات: C# | Windows Forms (GUI) | Object-Oriented Programming (OOP)
// =================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic; // المكتبة المسؤولة عن تشغيل شاشات إدخال البيانات المباشرة لـ Login

namespace RestaurantManagementSystem
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }

        public MenuItem(int id, string name, double price, string category)
        {
            Id = id;
            Name = name;
            Price = price;
            Category = category;
        }

        public override string ToString() => $"{Name} - {Price} JD";
    }

    public class Table
    {
        public int TableNumber { get; set; }
        public bool IsOccupied { get; set; }

        public Table(int number)
        {
            TableNumber = number;
            IsOccupied = false;
        }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int TableNumber { get; set; }
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
        public DateTime OrderTime { get; set; }
        public string ProcessedBy { get; set; } // تتبع اسم الموظف المسؤول عن الطلب

        public double TotalAmount => Items.Sum(item => item.Price); // حساب الفاتورة تلقائياً بدقة

        public Order(int orderId, int tableNumber, string EmployeeName)
        {
            OrderId = orderId;
            TableNumber = tableNumber;
            OrderTime = DateTime.Now;
            ProcessedBy = EmployeeName;
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // رتبة المستخدم: Admin أو Employee

        public User(string username, string password, string role)
        {
            Username = username;
            Password = password;
            Role = role;
        }
    }

    public partial class MainForm : Form
    {
        private List<MenuItem> menuList = new List<MenuItem>();
        private List<Table> tableList = new List<Table>();
        private List<User> userDatabase = new List<User>(); // محاكاة جدول الحسابات
        private List<Order> orderHistory = new List<Order>(); // محاكاة سجل الفواتير في قاعدة البيانات
        
        private Order currentOrder;
        private User loggedInUser = null;
        private int orderCounter = 1;

        private ListBox lstMenu = new ListBox();
        private ListBox lstCurrentOrder = new ListBox();
        private ComboBox cmbTables = new ComboBox();
        private Label lblTotal = new Label();
        private Button btnAddItem = new Button();
        private Button btnRemoveItem = new Button();
        private Button btnCheckout = new Button();
        private Button btnViewReports = new Button(); // زر التقارير الخاص بالمدير فقط

        public MainForm()
        {
            LoadInitialData();
            ShowLoginDialog(); // فرض تسجيل الدخول أولاً لحماية النظام
        }

        private void LoadInitialData()
        {
            userDatabase.Add(new User("admin", "123", "Admin"));
            userDatabase.Add(new User("emp1", "123", "Employee"));

            menuList.Add(new MenuItem(1, "شاورما سوبر", 3.50, "وجبات"));
            menuList.Add(new MenuItem(2, "برغر لحم", 4.00, "وجبات"));
            menuList.Add(new MenuItem(3, "بيتزا خضار", 5.50, "وجبات"));
            menuList.Add(new MenuItem(4, "بيبسي", 0.50, "مشروبات"));
            menuList.Add(new MenuItem(5, "ماء", 0.25, "مشروبات"));

            foreach (var item in menuList)
            {
                lstMenu.Items.Add(item);
            }

            for (int i = 1; i <= 5; i++)
            {
                tableList.Add(new Table(i));
                cmbTables.Items.Add($"طاولة {i}");
            }
            cmbTables.SelectedIndex = 0;
        }

        private void ShowLoginDialog()
        {
            string inputUser = Interaction.InputBox("أدخل اسم المستخدم (admin أو emp1):", "تسجيل الدخول للنظام", "");
            string inputPass = Interaction.InputBox("أدخل كلمة المرور (123):", "كلمة المرور", "");

            var user = userDatabase.FirstOrDefault(u => u.Username == inputUser && u.Password == inputPass);

            if (user != null)
            {
                loggedInUser = user;
                MessageBox.Show($"تم تسجيل الدخول بنجاح بصلاحية: {user.Role}", "أهلاً بك", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplyPermissions();
                InitializeCurrentOrder();
            }
            else
            {
                MessageBox.Show("خطأ في اسم المستخدم أو كلمة المرور! سيتم إغلاق النظام وحمايته.", "فشل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void ApplyPermissions()
        {
            if (loggedInUser.Role == "Employee")
            {
                btnViewReports.Enabled = false; // حجب زر التقارير والأرباح عن الموظف
                this.Text = $"نظام إدارة المطعم - واجهة الموظف: {loggedInUser.Username}";
            }
            else if (loggedInUser.Role == "Admin")
            {
                btnViewReports.Enabled = true; // تمكين المدير من الوصول لكافة التقارير
                this.Text = $"نظام إدارة المطعم - واجهة المدير (كامل الصلاحيات)";
            }
        }

        private void InitializeCurrentOrder()
        {
            if (loggedInUser == null) return;
            int selectedTable = cmbTables.SelectedIndex + 1;
            currentOrder = new Order(orderCounter++, selectedTable, loggedInUser.Username);
            UpdateOrderUI();
        }

        private void UpdateOrderUI()
        {
            lstCurrentOrder.Items.Clear();
            foreach (var item in currentOrder.Items)
            {
                lstCurrentOrder.Items.Add(item);
            }
            lblTotal.Text = $"إجمالي الفاتورة: {currentOrder.TotalAmount:F2} JD";
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (lstMenu.SelectedItem is MenuItem selectedItem)
            {
                currentOrder.Items.Add(selectedItem);
                UpdateOrderUI();
            }
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (lstCurrentOrder.SelectedItem is MenuItem selectedItem)
            {
                currentOrder.Items.Remove(selectedItem);
                UpdateOrderUI();
            }
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            if (currentOrder.Items.Count == 0) return;

            int currentTableIdx = cmbTables.SelectedIndex;
            tableList[currentTableIdx].IsOccupied = true;

            // أرشفة وحفظ الفاتورة في السجل العام (محاكاة قاعدة البيانات)
            orderHistory.Add(currentOrder);

            string receipt = $"--- فاتورة مطعم رقم ({currentOrder.OrderId}) ---\n" +
                             $"التاريخ: {currentOrder.OrderTime}\n" +
                             $"الطاولة: {currentOrder.TableNumber}\n" +
                             $"بواسطة الموظف: {currentOrder.ProcessedBy}\n" +
                             $"-----------------------------------\n" +
                             $"المجموع النهائي: {currentOrder.TotalAmount:F2} JD\n\n" +
                             $"تم الحفظ في قاعدة البيانات وطباعة الفاتورة آلياً!";
            
            MessageBox.Show(receipt, "الفاتورة والطباعة الآلية", MessageBoxButtons.OK, MessageBoxIcon.Information);
            InitializeCurrentOrder();
        }

        private void btnViewReports_Click(object sender, EventArgs e)
        {
            double totalSales = orderHistory.Sum(o => o.TotalAmount); // حساب الإيرادات التراكمية بـ LINQ
            int totalOrders = orderHistory.Count;

            string report = $"=== تقرير المبيعات والإحصائيات اليومية ===\n" +
                            $"إجمالي عدد الطلبات المنفذة: {totalOrders}\n" +
                            $"إجمالي الإيرادات والأرباح: {totalSales:F2} JD\n" +
                            $"=====================================";

            MessageBox.Show(report, "تقارير الإدارة والتحليل", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}