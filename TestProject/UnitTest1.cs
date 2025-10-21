using SpellCheckingTool;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestHelloWorld()
        {
            Assert.AreEqual(Program.Test(), "Hello World!");
        }
    }
}