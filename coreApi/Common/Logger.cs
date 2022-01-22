
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using System;

namespace coreApi.Common
{
    class Logger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(EventId id,ILogger logger, string exception)
        {
            #if DEBUG
                logger.LogError(id, exception);
            #else
                logger.LogError(id, exception.Split(Environment.NewLine)[0]);
            #endif
        }
    }
}