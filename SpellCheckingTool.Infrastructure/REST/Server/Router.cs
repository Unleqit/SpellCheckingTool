using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpellCheckingTool.Infrastructure.Http.Controllers;
using SpellCheckingTool.Infrastructure.Http.Servers.Attributes;
using System.Net;
using System.Reflection;

namespace SpellCheckingTool.Infrastructure.Http.Servers;

public class Router
{
    private readonly Dictionary<(string Method, string Path), Action<HttpListenerContext>> Routes = new();

    public Action<HttpListenerContext>? TryGetRoute(string method, string path)
    {
        Routes.TryGetValue((method, path), out var handler);
        return handler;
    }

    public void RegisterController(BaseController controller)
    {
        var type = controller.GetType();

        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (var attr in method.GetCustomAttributes<HttpAttribute>())
            {
                Action<HttpListenerContext> methodDelegate = (context) =>
                {
                    var parameters = method.GetParameters();
                    var paramInfos = parameters
                        .Select(p => (Param: p, FromBody: p.GetCustomAttribute<FromBodyAttribute>() != null))
                        .ToArray();

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

                    for (int i = 0; i < paramInfos.Length; i++)
                    {
                        var (param, fromBody) = paramInfos[i];
                        if (fromBody && bodyObj != null)
                            args[i] = bodyObj[param.Name!]?.ToObject(param.ParameterType);
                        else if (param.ParameterType == typeof(HttpListenerContext))
                            args[i] = context;
                        else
                            args[i] = null;
                    }

                    method.Invoke(controller, args);
                };

                Routes[(attr.Method, attr.Path)] = methodDelegate;
            }
        }
    }
}
