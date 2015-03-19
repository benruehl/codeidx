using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Services
{
    public class ErrorProvider
    {
        public static ErrorProvider Instance { get; private set; }
        private ILog _Logger;

        static ErrorProvider()
        {
            Instance = new ErrorProvider();
        }

        public void Init()
        {
            _Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void LogError(string message, Exception exception = null)
        {
            _Logger.Error(message, exception);
        }

        public void LogInfo(string message, Exception exception = null)
        {
            if (_Logger == null)
                return;

            _Logger.Info(message, exception);
        }

        public void LogWarning(string message, Exception exception = null)
        {
            if (_Logger == null)
                return;

            _Logger.Warn(message, exception);
        }

        public void LogDebug(string message, Exception exception = null)
        {
            if (_Logger == null)
                return;

            _Logger.Debug(message, exception);
        }

    }
}
