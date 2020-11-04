using UPS.Core.Models;

namespace UPS.App
{
    public interface IFormDetails<T>
    {
        T MyModel { get; set; }
    }
}
