using SpellCheckingTool.Application.Executables;
using System.Runtime.InteropServices;

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
            Assert.IsTrue(executables.Contains("powershell"));
        }
        else
        {
            var executables = new LinuxExecutableParser().GetAllShellExecutables();
            Assert.IsTrue(executables.Contains("bash"));
        }
    }
}
