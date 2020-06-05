using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class TryCatch
    {
        /// <summary>
        /// Catch exceptions which may be deep within an aggregate exception.
        /// </summary>
        public static void Try(Action tryAction, Action<Exception> catchAction, List<Type> catchableExceptions)
        {
            try
            {
                tryAction();
            }
            catch (Exception caughtException)
            {
                if (catchableExceptions.Any(catchableException => catchableException == caughtException.GetType()))
                {
                    catchAction(caughtException);
                    return;
                }

                if (caughtException is AggregateException aggregateException)
                {
                    aggregateException = aggregateException.Flatten();
                    if (aggregateException.InnerExceptions != null)
                    {
                        Exception innerCaughtException = aggregateException
                            .InnerExceptions
                            .FirstOrDefault(innerException => catchableExceptions
                                .Any(catchableException => catchableException == innerException.GetType()));

                        if (innerCaughtException != default(Exception))
                        {
                            catchAction(innerCaughtException);
                            return;
                        }

                    }
                }

                throw;
            }
        }
    }
}
