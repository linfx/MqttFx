using System.Threading.Tasks;

namespace nMqtt.Transport.Abstractions.Internal
{
    public interface ITransport
    {
        // Can only be called once per ITransport
        Task BindAsync();
        Task UnbindAsync();
        Task StopAsync();
    }
}
