using System.Threading.Tasks;

namespace AspNetCoreApp.Services
{
    public interface ILongRespondingService
    {
        Task<string> DoJob(string requestUri);
    }
}