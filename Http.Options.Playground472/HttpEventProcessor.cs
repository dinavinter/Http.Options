using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;

namespace Http.Options.Playground472
{
         public class HttpEventProcessor : BaseProcessor<Activity>
         {
             private readonly HttpEventListener _eventListener;
 
             public HttpEventProcessor(HttpEventListener eventListener)
             {
                 _eventListener = eventListener;
             }
 
             public override void OnStart(Activity data)
             {
               
                 base.OnStart(data);

                 if (data.Kind == ActivityKind.Client)
                 {
                     foreach (var tag in data.Parent?.TagObjects ?? Enumerable.Empty<KeyValuePair<string, object>>())

                     {
                         data.SetTag(tag.Key, tag.Value);
                     }
                 }
                 // if (_eventListener.Activities.TryGetValue(data.Id, out var eventSource))
                 // {
                 //     data.AddTag("netHttp.start", getMsg());
                 // }
                 //
                 // string getMsg()
                 // {
                 //     return string.Join(Environment.NewLine, eventSource.Select(e =>
                 //     {
                 //         var msgIndex = e.PayloadNames?.IndexOf("message");
                 //         if (msgIndex > -1)
                 //         {
                 //             return e.Payload?[msgIndex.Value]?.ToString();
                 //         }
                 //
                 //         return null;
                 //     }));
                 //
                 // }
             }
 
             public override void OnEnd(Activity data)
             {
                 base.OnEnd(data);
 
                 if (_eventListener.Activities.TryGetValue(data.Id, out var eventSource))
                 {
                     data.AddTag("netHttp.end", getMsg());
 
                 }
 
                 string getMsg()
                 {
                     return string.Join(Environment.NewLine, eventSource.Select(e =>
                     {
                         var msgIndex = e.PayloadNames?.IndexOf("message");
                         if (msgIndex > -1)
                         {
                             return e.Payload?[msgIndex.Value]?.ToString();
                         }
 
                         return null;
                     }));
 
                 }
             }
         }
}