using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UPS.Core;
using UPS.Core.Handlers;
using UPS.Core.Models;

namespace UPS.App
{
    public partial class FrmEmployee : Form
    {
        private readonly FormActionType _actionType;
        public Employee MyEmployee { get; private set; }

        FrmEmployee()
        {
            InitializeComponent();
            cmbGender.Items.AddRange(Enum.GetValues(typeof(Gender)).Cast<object>().ToArray());
            cmbStatus.Items.AddRange(Enum.GetValues(typeof(Status)).Cast<object>().ToArray());
        }
        public FrmEmployee(FormActionType ActionType, Employee employee):this()
        {
            _actionType = ActionType;

            if (ActionType == FormActionType.Edit)
                if (employee != null)
                    SetEmployee(employee);
                else
                    throw new ArgumentNullException("Employee", "Can not be null");

            if (ActionType == FormActionType.Search) 
                txtId.ReadOnly = false;
        }

        private async void BtnOk_Click(object sender, EventArgs e)
        {
            if (await DoOk())
                this.DialogResult = DialogResult.OK;
        }
        
        async Task<bool> DoOk()
        {
            try
            {
                switch (_actionType)
                {
                    case FormActionType.Add:
                        await DoAdd();
                        break;
                    case FormActionType.Edit:
                        await DoEdit();
                        break;
                    case FormActionType.Search:
                        this.MyEmployee = GetEmployee();
                        break;
                    default:
                        throw new NotImplementedException();
                }
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
        async Task DoAdd()
        {
            var emp = await new EmployeeHandler().CreateAsync(GetEmployee());
            if (emp != null)
            {
                txtId.Text = emp.Id.ToString();
                MessageBox.Show($"{emp.Name}, Has been successfully added with id ({emp.Id})", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        async Task DoEdit()
        {
            if (await new EmployeeHandler().UpdateAsync(ModifiedEntity: GetEmployee(), OrginalEntity: MyEmployee))
                MessageBox.Show($"{MyEmployee.Name}, Has been successfully updated", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void SetEmployee(Employee employee)
        {
            MyEmployee = employee;

            txtId.Text = employee.Id.ToString();
            txtName.Text = employee.Name;
            txtEmail.Text = employee.Email;
            cmbGender.SelectedItem = employee.Gender;
            cmbStatus.SelectedItem = employee.Status;
        }
        Employee GetEmployee()
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
 
    }
}
