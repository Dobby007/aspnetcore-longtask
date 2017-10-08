using System;
using System.Threading.Tasks;

namespace AspNetCoreApp.Services.LongTask
{
    public class LongTaskModel
    {
        public Task<object> LongTask { get; set; }
        public DateTime AddedTime { get; }

        public LongTaskModel(Task<object> longTask)
        {
            LongTask = longTask;
            AddedTime = DateTime.Now;
        }
    }
}
