using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using UPS.Core.Events;
using UPS.Core.Handlers;
using UPS.Core.Models;
using UPS.Core.Utilities.Export;

namespace UPS.App
{
    public partial class FrmEmployees : Form
    {
        readonly EmployeeHandler _handler = new EmployeeHandler();
        public FrmEmployees()
        {
            InitializeComponent();

            _handler.ProccessingDataStarted += OnProccessingDataStarted;
            _handler.ProccessingDataSuccess += OnProccessingDataSuccess;
            _handler.ProccessingDataError += OnProccessingDataError;
            _handler.ProccessingDataEnded += OnProccessingDataEnded;
        }

        private async void FrmEmployees_Load(object sender, EventArgs e)
        {
            await RefreshData();
        }

        #region Handler Events
        private void OnProccessingDataStarted(object sender, HandlerEventArgs e)
        {
            toolStrip.Enabled = false;
            lblStatus.ForeColor = Color.Black;
            lblStatus.Text = $"Please wait while proccessing {e.ActionType} ...";
        }
        private void OnProccessingDataSuccess(object sender, HandlerEventArgs e)
        {
            lblStatus.ForeColor = Color.Green;
            lblStatus.Text = "Redy";
        }
        private void OnProccessingDataError(object sender, HandlerEventArgs e)
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = $"Error proccessing {e.ActionType} !"; ;
        }
        private void OnProccessingDataEnded(object sender, HandlerEventArgs e)
        {
            toolStrip.Enabled = true;
            txtCurrentPage.Text = _handler.Pager.CurrentPage.ToString();
            lblTotalPages.Text = _handler.Pager.TotalPages.ToString();
            lblFilterStatus.Visible = !string.IsNullOrEmpty(_handler.Filter);
            lblFilterText.Text = $" | Fiter: [{_handler.Filter?.Replace("&", ", ")}]";
            lblRecords.Text = $" | Records: {_handler.Pager.TotalRecords}";
        }
        #endregion

        async Task RefreshData(int? PageNumber = 1)
        {
            try
            {
                int selectedRow = gvList.CurrentRow?.Index ?? 0;
                gvList.DataSource = await _handler.ReadPageAsync(PageNumber);

                if (gvList.Rows.Count > selectedRow)
                    gvList.Rows[selectedRow].Selected = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        #region Actions
        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await RefreshData(_handler?.Pager?.CurrentPage);
        }
        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            FrmEmployee frm = new FrmEmployee(FormActionType.Add, null);
 
            if (frm.ShowDialog() == DialogResult.OK) 
                await RefreshData(_handler.Pager.TotalPages);
        }
        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (gvList.SelectedRows.Count > 0)
            {
                Employee emp = (Employee)gvList.SelectedRows[0].DataBoundItem;
                var dialogResult = MessageBox.Show($"Are you sure to delete '{emp.Name}'", "Data loss warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        await _handler.DeleteAsync(emp);
                        await RefreshData(_handler.Pager.CurrentPage);
                        MessageBox.Show($"'{emp.Name}', Has been successfully deleted", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Select employee to delete", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            
        }
        private async void BtnEdit_Click(object sender, EventArgs e)
        {
            if (gvList.SelectedRows.Count > 0)
            {
                Employee emp = (Employee)gvList.SelectedRows[0].DataBoundItem;
                FrmEmployee frm = new FrmEmployee(FormActionType.Edit, emp);
                
                if (frm.ShowDialog() == DialogResult.OK) 
                    await RefreshData(_handler.Pager.CurrentPage);
            }
            else
            {
                MessageBox.Show("Select employee to modify", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            FrmEmployee frm = new FrmEmployee(FormActionType.Search, null);

            if (frm.ShowDialog() == DialogResult.OK)
            { 
                _handler.SetFilter(frm.MyEmployee);
                await RefreshData();
            }
        }
        private async void BtnClearFilter_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_handler.Filter))
            {
                _handler.ClearFilter();
                await RefreshData();
            }
        }
        private async void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog.Filter = "comma-separated values|*.csv";
                saveFileDialog.Title = "Save CSV File";
                saveFileDialog.FileName = $"Employees_{DateTime.Now:ddMMyyy_HHmmss}.csv";
                saveFileDialog.ShowDialog();

                if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                {
                    using (var stream = await _handler.ExportAsync(new CsvExporter()))
                    {
                        using (var fileStream = new FileStream($@"{saveFileDialog.FileName}", FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Export error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        #endregion

        #region Navigation
        private async void BtnFirst_Click(object sender, EventArgs e)
        {
            if (_handler?.Pager?.CurrentPage > 1) 
                await RefreshData();
        }
        private async void BtnLast_Click(object sender, EventArgs e)
        {
            if (_handler?.Pager?.CurrentPage != _handler?.Pager?.TotalPages)
                await RefreshData(_handler?.Pager?.TotalPages);
        }
        private async void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (_handler?.Pager?.CurrentPage > 1)
                await RefreshData(_handler?.Pager?.CurrentPage - 1);
        }
        private async void BtnNext_Click(object sender, EventArgs e)
        {
            if (_handler?.Pager?.CurrentPage < _handler?.Pager?.TotalPages)
                await RefreshData(_handler?.Pager?.CurrentPage + 1);
        }
        private async void TxtCurrentPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                int.TryParse(txtCurrentPage.Text, out int page);
                if (page > 0 && _handler?.Pager?.TotalPages >= page)
                    await RefreshData(page);
            }
        }
        #endregion 
    }
}
