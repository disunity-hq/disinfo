using System;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Disunity.Disinfo.Services {

    [AsSingleton]
    public class LoggingService<T> {

        private readonly ILogger<T> _log;

        public LoggingService(ILogger<T> log) {
            _log = log;
        }
        
        
        public Task LogMessage(LogMessage message) {
            LogLevel severity;

            switch (message.Severity) {
                case LogSeverity.Critical: {
                    severity = LogLevel.Critical;
                    break;
                }

                case LogSeverity.Debug: {
                    severity = LogLevel.Debug;
                    break;
                }

                case LogSeverity.Info: {
                    severity = LogLevel.Information;
                    break;
                }

                case LogSeverity.Warning: {
                    severity = LogLevel.Warning;
                    break;
                }

                case LogSeverity.Verbose: {
                    severity = LogLevel.Trace;
                    break;
                }

                default: {
                    severity = LogLevel.Information;
                    break;
                }
            }


            if (message.Exception != null) {
                _log.LogError(message.Exception, message.Message);
                _log.LogError(message.Exception.Message);

                if (message.Exception is OptionsValidationException e) {
                    foreach (var failure in e.Failures) {
                        _log.LogInformation(failure);
                    }
                }
            } else {
                _log.Log(severity, message.Message);
            }

            return Task.CompletedTask;
        }

    }

}