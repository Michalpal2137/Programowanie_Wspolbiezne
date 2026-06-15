using System.Threading.Tasks;

namespace Dane
{
    public interface IDiagnosticWriter
    {
        Task WriteAsync(DiagnosticData data);
        bool IsReady { get; }
        int QueueSize { get; }
    }
}