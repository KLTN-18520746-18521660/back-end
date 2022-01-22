
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using System;

namespace coreApi.Common
{
    class Logger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(ILogger logger, string exception)
        {
            #if DEBUG
                logger.LogError(exception);
            #else
                logger.LogError(exception.Split(Environment.NewLine)[0]);
            #endif
        }
    }
}