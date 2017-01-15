using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using NLog;

namespace Astute.Communication
{
    public static class Input
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Creates an observable stream of strings from TCP.
        /// </summary>
        public static IObservable<string> TcpInput { get; } =
            Observable.Create<string>(observer =>
                {
                    Exception exception = null;

                    try
                    {
                        // Create and start the listener.
                        var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7000);
                        listener.Start();
                        Logger.Info("TcpListener started. ");

                        // Listen in an endless loop. 
                        while (true)
                        {
                            string value;

                            using (var networkStream = listener.AcceptTcpClient().GetStream())
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    networkStream.CopyTo(memoryStream);
                                    value = Encoding.UTF8.GetString(memoryStream.ToArray());
                                }
                            }

                            Logger.Trace($"Received: {value}");
                            // BUG When inputs are received too fast, two inputs maybe sent in one string
                            // e.g.: TOO_QUICK#TOO_QUICK#
                            observer.OnNext(value);
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        Logger.Error(exception);
                    }
                    finally
                    {
                        if (exception != null)
                        {
                            Logger.Warn(exception);
                            observer.OnError(exception);
                        }
                        else // completed successfully
                        {
                            Logger.Info("Observable completed. ");
                            observer.OnCompleted();
                        }
                    }

                    return () => Logger.Info("Observable has unsubscribed. ");
                }
            );
    }
}