using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UPS.Core.Attributes;

namespace UPS.Core.Utilities.Export
{
    public class CsvExporter : IExporter
    {
        public async Task<Stream> ExportAsync<T>(IEnumerable<T> modelList)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var lines = new List<string>();

            var exportableProperties = typeof(T).GetProperties().Where(property => property.IsDefined(typeof(ExportAttribute)));
            string header = string.Join(",", exportableProperties.ToList().Select(x => x.Name));
            var valueLines = modelList.Select(model => string.Join(",", exportableProperties.ToList().Select(prop => typeof(T).GetProperty(prop.Name).GetValue(model))));

            lines.Add(header);
            lines.AddRange(valueLines);

            foreach (var line in lines)
            {
                await writer.WriteAsync(line);
                await writer.WriteLineAsync();
            }

            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
