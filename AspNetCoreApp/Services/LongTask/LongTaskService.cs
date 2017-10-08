using AspNetCoreApp.Services.HttpContext;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreApp.Services.LongTask
{
    public class LongTaskService : BasicLongTaskService
    {
        private readonly IHttpContextPreserver _httpContextPreserver;

        public LongTaskService(IHttpContextPreserver httpContextPreserver)
        {
            _httpContextPreserver = httpContextPreserver;
        }

        public override Guid StartTask<T>(Func<Task<T>> taskFunc)
        {
            _httpContextPreserver.CloneCurrentContext();

            var resultGuid = Guid.NewGuid();

            var resultTask = Task.Run(async () => (object)await taskFunc());

            TasksDic.AddOrUpdate(resultGuid, new LongTaskModel(resultTask), (key, oldValue) => oldValue);

            return resultGuid;
        }
        
        
    }
    
}