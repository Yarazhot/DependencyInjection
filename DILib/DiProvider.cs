using NLog;

namespace DILib
{
    public class DiProvider:IDiProvider
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IDiConfig _diConfig;

        public DiProvider(IDiConfig diConfig)
        {
            _diConfig = diConfig;
        }

        public T Inject<T>()
        {
            _logger.Trace("Inject "+typeof(T).Name);
            return (T) _diConfig.Get(typeof(T));
        }
    }

    public interface IDiProvider
    {
        public T Inject<T>();
    }
}