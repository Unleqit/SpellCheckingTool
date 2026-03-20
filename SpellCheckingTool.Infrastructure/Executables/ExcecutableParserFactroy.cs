using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Infrastructure.Executables
{
    public static class ExecutableParserFactory
    {
        public static IExecutableParser Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsExecutableParser();
            return new LinuxExecutableParser();
        }
    }
}
