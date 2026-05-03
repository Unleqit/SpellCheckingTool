using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Executables
{
    public class ExecutableService
    {
        private readonly IExecutableParser _parser;

        public ExecutableService(IExecutableParser parser)
        {
            _parser = parser;
        }

        public IWordStorage LoadExecutables()
        {
            return _parser.GetAllShellExecutables();
        }
    }
}
