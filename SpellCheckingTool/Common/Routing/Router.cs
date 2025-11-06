using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpellCheckingTool.Common.Routing.Attributes;
using System.Net;
using System.Reflection;

namespace SpellCheckingTool
{
    public class Router
    {
        private readonly Dictionary<(string Method, string Path), Action<HttpListenerContext>> Routes = new();

        public Router(Assembly asm)
        {
            RegisterRoutes(asm);
        }

        //basically the linker doesn't know whether these methods exist at compile time
        //Suppress the warnings, as trmming, e.g. the removal of "unused" methods in the scope of this application was disabled in the project settings
#pragma warning disable IL2026, IL2075, IL3050, CS8602
        private void RegisterRoutes(Assembly asm)
        {
            foreach (var type in asm.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    foreach (var attr in method.GetCustomAttributes<HttpAttribute>())
                    {
                        //define method delegate for the C# method which defines this route
                        Action<HttpListenerContext> methodDelegate = (context) =>
                        {
                            var parameters = method.GetParameters();
                            var paramInfos = new (ParameterInfo Param, bool FromBody)[parameters.Length];

                            for (int i = 0; i < parameters.Length; i++)
                                paramInfos[i] = (parameters[i], parameters[i].GetCustomAttribute<FromBodyAttribute>() != null);
                            
                            object?[] args = new object?[parameters.Length];

                            JObject? bodyObj = null;
                            if (paramInfos.Any(p => p.FromBody))
                            {
                                using var reader = new StreamReader(context.Request.InputStream);
                                string body = reader.ReadToEnd();
                                try
                                {
                                    bodyObj = JObject.Parse(body);
                                }
                                catch (JsonException e)
                                {
                                    context.Response.StatusCode = 400;
                                    using var writer = new StreamWriter(context.Response.OutputStream);
                                    writer.Write($"Invalid JSON body ({e.Message})");
                                    context.Response.Close();
                                    return;
                                }
                            }

                            //check if method parameter contains [FromBody] attribute to be implicitly parsed from the request bodys JSON content
                            for (int i = 0; i < paramInfos.Length; i++)
                            {
                                var (param, fromBody) = paramInfos[i];

                                if (fromBody && bodyObj != null)
#pragma warning disable CS8604
                                    args[i] = bodyObj[param.Name]?.ToObject(param.ParameterType);
#pragma warning restore CS8604
                                else if (param.ParameterType == typeof(HttpListenerContext))
                                    args[i] = context;
                                else
                                    args[i] = null; // fallback, could extend for query/header params
                            }

                            // Invoke the method once with all arguments
                            method.Invoke(null, args);
                        };


                        Routes[(attr.Method, attr.Path)] = methodDelegate;
                    }
                }
            }
        }

        public Action<HttpListenerContext>? TryGetRoute(string method, string path)
        {
            Routes.TryGetValue((method, path), out var handler);
            return handler;
        }


#pragma warning restore IL2026, IL2075, IL3050, CS8602

    }
}
