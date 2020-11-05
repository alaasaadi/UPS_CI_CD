using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UPS.Core.Utilities.Export
{
    public interface IExporter
    {
        Task<Stream> ExportAsync<T>(IEnumerable<T> models);
    }
}
