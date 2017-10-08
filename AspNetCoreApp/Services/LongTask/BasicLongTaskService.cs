using AspNetCoreApp.Services.HttpContext;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreApp.Services.LongTask
{
    public class BasicLongTaskService : ILongTaskService
    {
        protected ConcurrentDictionary<Guid, LongTaskModel> TasksDic = new ConcurrentDictionary<Guid, LongTaskModel>();
        
        public BasicLongTaskService()
        {
        }

        public virtual Guid StartTask<T>(Func<Task<T>> taskFunc)
        {
            var resultGuid = Guid.NewGuid();

            var resultTask = Task.Run(async () => (object)await taskFunc());

            TasksDic.AddOrUpdate(resultGuid, new LongTaskModel(resultTask), (key, oldValue) => oldValue);

            return resultGuid;
        }

        public object CheckResult(Guid taskGuid)
        {
            LongTaskModel value;
            Task<object> resultTask;
            try
            {
                resultTask = TasksDic[taskGuid].LongTask;
            }
            catch(KeyNotFoundException e)
            {
                throw new TaskKeyNotFoundException($"Task associated with GUID {taskGuid} was not found", e);
            }

            if (resultTask.Status == TaskStatus.RanToCompletion)
            { 
                TasksDic.TryRemove(taskGuid, out value);
                return resultTask.Result;
            }
            else if (resultTask.IsFaulted)
            {
                TasksDic.TryRemove(taskGuid, out value);
                throw new AggregateException("Scheduled task ended with exception", resultTask.Exception);
            }
            else if (resultTask.IsCanceled)
            {
                TasksDic.TryRemove(taskGuid, out value);
                throw new TaskCanceledException("Scheduled task was cancelled. No result can be retrieved.", resultTask.Exception);
            }

            return null;
        }
        
    }
    
}