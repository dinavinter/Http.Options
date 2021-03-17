using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Http.Options.Playground472
{
    public class Listner
    {        private static object allListeners = new object();
        private static IDisposable networkSubscription;

        
        static IDisposable listenerSubscriptions = DiagnosticListener.AllListeners.Subscribe(
            delegate(DiagnosticListener listener)
            {
                // if (listener.Name == "System.Net.Http")
                // {
                lock (allListeners)
                {
                    // if (networkSubscription != null)
                    //     networkSubscription.Dispose();

                    networkSubscription = listener.Subscribe((KeyValuePair<string, object> evnt) =>
                        Console.WriteLine("From Listener {0} Received Event {1} with payload {2}",
                            listener.Name, evnt.Key, evnt.Value.ToString()));
                }

                // }
            });

        public Listner()
        {
            // Create the callback delegate
            // Action<KeyValuePair<string, object>> callback = (KeyValuePair<string, object> evnt) =>
            //     Console.WriteLine("From Listener {0} Received Event {1} with payload {2}", networkListener.Name, evnt.Key, evnt.Value.ToString());
            //
            // // Turn it into an observer (using System.Reactive.Core's AnonymousObserver)
            // IObserver<KeyValuePair<string, object>> observer = new AnonymousObserver<KeyValuePair<string, object>>(callback);
            //
            // // Create a predicate (asks only for one kind of event)
            // Predicate<string> predicate = (string eventName) => eventName == "RequestStart";
            //
            // // Subscribe with a filter predicate
            // IDisposable subscription = listener.Subscribe(observer, predicate);
        }
        
    }
}