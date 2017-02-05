using System.Threading.Tasks;
using Contracts.Entity;

namespace SmartFaceAligner.Processor.Services
{
    public interface IFileManagementService
    {
        Task<int> ImportFolder(Project p, string sourceFolder);
    }
}