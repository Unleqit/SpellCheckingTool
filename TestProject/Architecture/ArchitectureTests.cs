using NetArchTest.Rules;
using NetArchTestResult = NetArchTest.Rules.TestResult;

namespace TestProject.Architecture;

[TestClass]
public class CleanArchitectureTests
{

    private const string DomainNamespace = "SpellCheckingTool.Domain";
    private const string ApplicationNamespace = "SpellCheckingTool.Application";
    private const string InfrastructureNamespace = "SpellCheckingTool.Infrastructure";
    private const string PresentationNamespace = "SpellCheckingTool.Presentation";

    private static void AssertArchitecture(NetArchTestResult result)
    {
        if (result.IsSuccessful)
            return;

        var failing = result.FailingTypes ?? Enumerable.Empty<System.Type>();

        Assert.Fail(
            "Architecture violation:\n" +
            string.Join("\n", failing.Select(t => t.FullName))
        );
    }

    [TestMethod]
    public void Domain_must_not_depend_on_Application_Infrastructure_or_Presentation()
    {
        var result = Types
            .InAssembly(typeof(SpellCheckingTool.Domain.WordTree.WordTree).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, PresentationNamespace)
            .GetResult();

        AssertArchitecture(result);
    }

    [TestMethod]
    public void Application_must_not_depend_on_Infrastructure_or_Presentation()
    {
        var result = Types
            .InAssembly(typeof(SpellCheckingTool.Application.Spellcheck.SpellcheckService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, PresentationNamespace)
            .GetResult();

        AssertArchitecture(result);
    }

    [TestMethod]
    public void Infrastructure_must_not_depend_on_Presentation()
    {
        var result = Types
            .InAssembly(typeof(SpellCheckingTool.Infrastructure.FilePersistence.FilePersistenceService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(PresentationNamespace)
            .GetResult();

        AssertArchitecture(result);
    }

    [TestMethod]
    public void Presentation_must_not_depend_on_Infrastructure_except_composition_root()
    {
        var presentationAssembly = typeof(SpellCheckingTool.Presentation.Program).Assembly;

        // All Presentation types except Program must NOT depend on Infrastructure
        var result = Types
            .InAssembly(presentationAssembly)
            .That()
            .DoNotHaveName("Program")
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace)
            .GetResult();

        AssertArchitecture(result);
    }
}