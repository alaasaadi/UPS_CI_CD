using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using UPS.Core;
using UPS.Core.Models;

namespace UPS.App
{
    public partial class FrmEmployeeDetails : Form, IFormDetails<Employee>
    {
        public Employee MyModel
        {
            get
            {
                Employee employee = new Employee();

                int.TryParse(txtId.Text, out int id);
                employee.Id = id;
                if (!string.IsNullOrWhiteSpace(txtName.Text)) employee.Name = txtName.Text;
                if (!string.IsNullOrWhiteSpace(txtEmail.Text)) employee.Email = txtEmail.Text;
                if (!string.IsNullOrWhiteSpace(cmbGender.Text)) employee.Gender = (Gender)cmbGender.SelectedItem;
                if (!string.IsNullOrWhiteSpace(cmbStatus.Text)) employee.Status = (Status)cmbStatus.SelectedItem;

                return employee;
            }

            set
            {
                txtId.Text = value.Id.ToString();
                txtName.Text = value.Name;
                txtEmail.Text = value.Email;
                cmbGender.SelectedItem = value.Gender;
                cmbStatus.SelectedItem = value.Status;
            }
        }

        public FrmEmployeeDetails()
        {
            InitializeComponent();
            cmbGender.Items.AddRange(Enum.GetValues(typeof(Gender)).Cast<object>().ToArray());
            cmbStatus.Items.AddRange(Enum.GetValues(typeof(Status)).Cast<object>().ToArray());
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
