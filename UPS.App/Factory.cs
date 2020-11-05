using System;
using System.Windows.Forms;
using UPS.Core.Handlers;
using UPS.Core.Models;

namespace UPS.App
{
    public static class Factory
    {
        public static Form GetInstance(FormType formType)
        {
            switch (formType)
            {
                case FormType.Employees:
                    return new FrmList<Employee, FrmEmployeeDetails>(new EmployeeHandler(), "Employees");
                default:
                    throw new NotImplementedException();
            }
        }

        public static Form CreateEmployeeListForm()
        {
            return new FrmList<Employee, FrmEmployeeDetails>(new EmployeeHandler(), "Employees");
        }
    }
}
