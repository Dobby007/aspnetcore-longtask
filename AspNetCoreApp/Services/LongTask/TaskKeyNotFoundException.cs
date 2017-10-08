using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreApp.Services.LongTask
{
    [Serializable]
    public class TaskKeyNotFoundException : Exception
    {
        public TaskKeyNotFoundException() { }
        public TaskKeyNotFoundException(string message) : base(message) { }
        public TaskKeyNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected TaskKeyNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
