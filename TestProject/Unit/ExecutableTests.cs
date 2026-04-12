using SpellCheckingTool.Infrastructure.Executables;
using System.Runtime.InteropServices;
using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Alphabet;

namespace TestProject.Unit;

[TestClass]
public class ExecutableTests
{
    [TestMethod]
    public void GetExecutableSuggestionsOnCurrentPlatform_ShouldContainApplicationsInstalledOnSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var executables = new WindowsExecutableParser().GetAllShellExecutables();
            Assert.IsTrue(executables.Contains(new Word(new LatinAlphabet(), "powershell")));
        }
        else
        {
            var executables = new LinuxExecutableParser().GetAllShellExecutables();
            Assert.IsTrue(executables.Contains(new Word(new LatinAlphabet(), "bash")));
        }
    }
}
