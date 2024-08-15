using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OfficeOpenXml;

namespace ExcelApp
{
    public partial class MainForm : Form
    {
        private string inputFilePath = @"C:\Users\ASUS\Desktop\NewFiles\input.xlsx";
        private string outputFilePath = @"C:\Users\ASUS\Desktop\NewFiles\output.xlsx";

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        private const uint FR_PRIVATE = 0x10;

        public MainForm()
        {
            InitializeComponent();
            LoadCustomFont();
            // اضافه کردن ستون‌ها به DataGridView با نام‌های جدید
            dataGridView.Columns.Add("ItemCode", "کد کالا");
            dataGridView.Columns.Add("ItemName", "نام کالا");
            dataGridView.Columns.Add("PurchasePrice", "قیمت خرید");
            dataGridView.Columns.Add("LastPurchasePrice", "آخرین قیمت خرید");
            dataGridView.Columns.Add("DefaultSalePrice", "قیمت فروش پیشفرض");
            dataGridView.Columns.Add("Stock", "موجودی کالا");
        }

        private void LoadCustomFont()
        {
            string fontPath = Path.Combine(Application.StartupPath, "Fonts", "IRANYekanX-Bold.ttf");
            AddFontResourceEx(fontPath, FR_PRIVATE, IntPtr.Zero);

            Font customFont = new Font("IRANYekanX-Bold.ttf", 12);
            this.Font = customFont; // اعمال فونت به فرم
            // اعمال فونت به کنترل‌های دیگر
            dataGridView.Font = customFont;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string itemCode = txtItemCode.Text;

            using (var package = new ExcelPackage(new FileInfo(inputFilePath)))
            {
                var worksheet = package.Workbook.Worksheets["همگام سیستم"];
                var cell = worksheet.Cells["A:A"].FirstOrDefault(c => c.Text == itemCode);
                if (cell == null)
                {
                    MessageBox.Show("کد آیتم یافت نشد.");
                    return;
                }
                int row = cell.Start.Row;

                if (worksheet.Cells[row, 6].GetValue<int>() < 0)
                {
                    MessageBox.Show($"موجودی برای آیتم {itemCode} کمتر از صفر است.");
                    return;
                }

                var rowData = new object[6];
                for (int col = 1; col <= 6; col++)
                {
                    rowData[col - 1] = worksheet.Cells[row, col].Value;
                }

                dataGridView.Rows.Add(rowData);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var outputPackage = new ExcelPackage(new FileInfo(outputFilePath)))
            {
                var outputWorksheet = outputPackage.Workbook.Worksheets.FirstOrDefault() ?? outputPackage.Workbook.Worksheets.Add("Output");

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.IsNewRow) continue;

                    string itemCode = row.Cells[0].Value.ToString();
                    var existingCell = outputWorksheet.Cells["A:A"].FirstOrDefault(c => c.Text == itemCode);
                    if (existingCell != null)
                    {
                        int existingRow = existingCell.Start.Row;
                        for (int col = 1; col <= 6; col++)
                        {
                            outputWorksheet.Cells[existingRow, col].Value = row.Cells[col - 1].Value;
                        }
                    }
                    else
                    {
                        int newRow = (outputWorksheet.Dimension?.End.Row ?? 0) + 1;
                        for (int col = 1; col <= 6; col++)
                        {
                            outputWorksheet.Cells[newRow, col].Value = row.Cells[col - 1].Value;
                        }
                    }
                }

                outputPackage.SaveAs(new FileInfo(outputFilePath));
            }

            MessageBox.Show("اطلاعات با موفقیت ذخیره شد.");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
