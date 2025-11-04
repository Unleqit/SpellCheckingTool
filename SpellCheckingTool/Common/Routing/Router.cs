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
#pragma warning disable IL2026, IL2075, CS8602
        private void RegisterRoutes(Assembly asm)
        {
            foreach (var type in asm.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    foreach (var attr in method.GetCustomAttributes<HttpAttribute>())
                    {
                        Action<HttpListenerContext> methodDelegate = (context) => method.Invoke(null, new object?[] { context });
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


#pragma warning restore IL2026, IL2075, CS8602

    }
}
