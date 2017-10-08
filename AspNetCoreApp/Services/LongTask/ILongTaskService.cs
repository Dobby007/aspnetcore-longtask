using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreApp.Services.LongTask
{
    /// <summary>
    /// Сервис для запуска долгих заданий и сохранения результата их выполнения. Используется в основном для обращения к сторонним API.
    /// </summary>
    public interface ILongTaskService
    {
        Guid StartTask<T>(Func<Task<T>> taskFunc);

        object CheckResult(Guid taskGuid);
        
    }
}
