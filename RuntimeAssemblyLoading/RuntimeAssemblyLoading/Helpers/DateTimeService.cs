using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeAssemblyLoading.Helpers;
public interface IDateTimeService
{
    DateTime UtcNow { get; }
}

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
