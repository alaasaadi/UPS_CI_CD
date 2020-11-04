using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    return new FrmList<Employee, FrmEmployee>(new EmployeeHandler(), "Employees");
                default:
                    throw new NotImplementedException();        
            }
        }
    }
}
