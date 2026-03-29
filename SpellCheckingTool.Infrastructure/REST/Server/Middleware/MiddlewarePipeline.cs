using System.Net;

namespace SpellCheckingTool.Infrastructure.Http.Servers;
    public class MiddlewarePipeline
    {
        private readonly List<Middleware> Middlewares = new();

        public void Use(Middleware middleware)
        {
            Middlewares.Add(middleware);
        }

        public void Execute(HttpListenerContext context, Action endpoint)
        {
            int index = 0;

            Action next = null!;

            next = () =>
            {
                if (index < Middlewares.Count)
                {
                    Middleware current = Middlewares[index];
                    index++;
                    current(context, next);
                }
                else
                {
                    endpoint();
                }
            };

            next();
        }
    }
