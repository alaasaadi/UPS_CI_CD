using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UPS.Core.Utilities.Export
{
    public class ExcelExporter : IExporter
    {
        public async Task<Stream> ExportAsync<T>(IEnumerable<T> models)
        {
            throw new NotImplementedException();
        }
    }
}
