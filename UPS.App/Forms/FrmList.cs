using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UPS.Core.Events;
using UPS.Core.Handlers;
using UPS.Core.Models;
using UPS.Core.Utilities.Export;

namespace UPS.App
{
    public partial class FrmList<M, F> : Form where M : IModel 
                                              where F : Form, IFormDetails<M>, new()
    {
        readonly IHandler<M> _handler;
        public FrmList(IHandler<M> handler, string title)
        {
            InitializeComponent();
            _handler = handler;
            this.Text = title;
            _handler.ProccessingDataStarted += OnProccessingDataStarted;
            _handler.ProccessingDataSuccess += OnProccessingDataSuccess;
            _handler.ProccessingDataError += OnProccessingDataError;
            _handler.ProccessingDataEnded += OnProccessingDataEnded;
        }

        private async void FrmList_Load(object sender, EventArgs e)
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

        #region CRUD
        async Task<bool> DoCreateAsync(M model)
        {
            try
            {
                var emp = await _handler.CreateAsync(model);
                if (emp != null)
                {
                    MessageBox.Show($"Successfully added", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            catch (AggregateException aggEx)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var ex in aggEx.Flatten().InnerExceptions)
                    sb.AppendLine(ex.Message);

                MessageBox.Show(sb.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
        async Task<bool> DoUpdateAsync(M model, M orginalModel)
        {
            try
            {
                if (await _handler.UpdateAsync(ModifiedEntity: model, OrginalEntity: orginalModel))
                    MessageBox.Show($"Successfully updated", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (AggregateException aggEx)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var ex in aggEx.Flatten().InnerExceptions)
                    sb.AppendLine(ex.Message);

                MessageBox.Show(sb.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
        async Task<bool> DoDeleteAsync(M model)
        {
            try
            {
                await _handler.DeleteAsync(model);
                MessageBox.Show($"Successfully deleted", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
        M GetSelectedModel()
        {
            if (gvList.SelectedRows.Count > 0)
            {
                return (M)gvList.SelectedRows[0].DataBoundItem;
            }
            else
            {
                return default;
            }
        }
        async Task RefreshData(int? PageNumber = 1)
        {
            try
            {
                int selectedRow = gvList.SelectedRows.Count > 0 ? gvList.SelectedRows[0].Index : 1;
                gvList.DataSource = await _handler.ReadPageAsync(PageNumber);

                if (gvList.Rows.Count > selectedRow)
                    gvList.Rows[selectedRow].Selected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Actions
        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await RefreshData(_handler.Pager.CurrentPage);
        }
        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var frm = new F())
            {
                do
                {
                    frm.ShowDialog();
                } while (frm.DialogResult == DialogResult.OK && !await DoCreateAsync(frm.MyModel));

                if (frm.DialogResult == DialogResult.OK)
                {
                    await RefreshData(_handler.Pager.TotalPages);
                    gvList.Rows[gvList.Rows.Count - 1].Selected = true;
                }
            }
        }
        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            M emp = GetSelectedModel();
            if (emp != null)
            {
                var dialogResult = MessageBox.Show($"Are you sure to delete", "Data loss warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (dialogResult == DialogResult.Yes)
                {
                    await DoDeleteAsync(emp);
                    await RefreshData(_handler.Pager.CurrentPage);
                }
            }
            else
            {
                MessageBox.Show("Select employee to delete", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private async void BtnEdit_Click(object sender, EventArgs e)
        {

            M emp = GetSelectedModel();
            if (emp != null)
            {
                var frm = new F()
                {
                    MyModel = emp
                };
                do
                {
                    frm.ShowDialog();
                } while (frm.DialogResult == DialogResult.OK && !await DoUpdateAsync(frm.MyModel, emp));

                if (frm.DialogResult == DialogResult.OK) await RefreshData(_handler.Pager.CurrentPage);
                frm.Dispose();
            }
            else
            {
                MessageBox.Show("Select employee to modify", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private async void BtnSearch_Click(object sender, EventArgs e)
        {

            using (var frm = new F())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    _handler.SetFilter(frm.MyModel);
                    await RefreshData();
                }
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
                saveFileDialog.FileName = $"{this.Text}_{DateTime.Now:ddMMyyy_HHmmss}.csv";
                saveFileDialog.ShowDialog();

                if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                {
                    using (var stream = await _handler.ExportAsync(new CsvExporter()))
                    {
                        using (var fileStream = new FileStream($@"{saveFileDialog.FileName}", FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(fileStream);
                            var msg = MessageBox.Show("Export file has been successfuly completed, do yo want to open the file?", "Export Complteted", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (msg == DialogResult.Yes)
                                Process.Start(saveFileDialog.FileName);
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
            if (_handler.Pager.CurrentPage > 1)
                await RefreshData();
        }
        private async void BtnLast_Click(object sender, EventArgs e)
        {
            if (_handler.Pager.CurrentPage != _handler.Pager.TotalPages)
                await RefreshData(_handler.Pager.TotalPages);
        }
        private async void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (_handler.Pager.CurrentPage > 1)
                await RefreshData(_handler.Pager.CurrentPage - 1);
        }
        private async void BtnNext_Click(object sender, EventArgs e)
        {
            if (_handler.Pager.CurrentPage < _handler.Pager.TotalPages)
                await RefreshData(_handler.Pager.CurrentPage + 1);
        }
        private async void TxtCurrentPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                int.TryParse(txtCurrentPage.Text, out int page);
                if (page > 0 && _handler.Pager.TotalPages >= page)
                    await RefreshData(page);
            }
        }
        #endregion 
    }
}
