using SpellCheckingTool;
using System.Runtime.InteropServices;

namespace TestProject.Unit
{
    [TestClass]
    public unsafe class PersistenceServiceTests
    {
#pragma warning disable CS8618 
        WordTree tree;
#pragma warning restore CS8618

        [TestInitialize]
        public void SetupTests()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Word[] words = Word.ParseWords(alphabet, new string[] { "these", "are", "some", "random", "english", "words", "containing", "only", "latin", "characters" });
            tree = new WordTree(alphabet);
            tree.Add(words);
        }

        [TestMethod]
        public void SerializeAndDeserializeWordTree_ShouldSucceed()
        {
            bool success = tree.Serialize(new FilePath(Directory.GetCurrentDirectory() + @"\test.wdb"));
            Assert.IsTrue(success);
            WordTree tree2 = tree.Deserialize(new FilePath(Directory.GetCurrentDirectory() + @"\test.wdb"));
            Assert.IsTrue(tree2 != null);
            Assert.AreEqual(tree.metaData.wordBufferLength, tree2.metaData.wordBufferLength);
            Assert.AreEqual(tree.metaData.wordCount, tree2.metaData.wordCount);
            Assert.AreEqual(tree.metaData.nodeCount, tree2.metaData.nodeCount);
            Assert.AreEqual(tree.metaData.serializationLength, tree2.metaData.serializationLength);
            Assert.IsTrue(tree.alphabet.GetChars().SequenceEqual(tree2.alphabet.GetChars()));
            Assert.AreEqual(tree.alphabet.GetLength(), tree2.alphabet.GetLength());
            Assert.AreEqual(tree.Contains(new Word(tree.alphabet, "these")), tree2.Contains(new Word(tree2.alphabet, "these")));
            File.Delete(Directory.GetCurrentDirectory() + @"\test.wdb");
            Assert.IsTrue(!File.Exists(Directory.GetCurrentDirectory() + @"\test.wdb"));
        }

        [TestMethod]
        public void CreateWordFilePathWithInvalidPath_ShouldThrowException()
        {
            Assert.ThrowsException<Exception>(() => { new FilePath(@"this/is/an/invalid/filepath"); });
        }

        [TestMethod]
        public void CreateWordFilePathInDirectory_ShouldThrowException()
        {
            Assert.ThrowsException<Exception>(() => { new FilePath(Directory.GetCurrentDirectory() + @"\"); });
        }

        [TestMethod]
        public void SaveToNonWDBFile_ShouldThrowException()
        {
            Assert.ThrowsException<Exception>(() => { tree.Serialize(new FilePath(Directory.GetCurrentDirectory() + @"\thisIsNotAValidFileFormat.txt")); });
        }

        [TestMethod]
        public void DeserializeNonExistantFile_ShouldThrowException()
        {
            Assert.ThrowsException<Exception>(() => { tree.Deserialize(new FilePath(@"this/is/an/invalid/filepath")); });
        }

        [TestMethod]
        public void DeserializeTreeFromNonWDBFile_ShouldThrowException()
        {
            //create the invalid file
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\thisIsNotAValidFileFormat.txt", "");
            Assert.IsTrue(File.Exists(Directory.GetCurrentDirectory() + @"\thisIsNotAValidFileFormat.txt"));
            Assert.ThrowsException<Exception>(() => { tree.Deserialize(new FilePath(Directory.GetCurrentDirectory() + @"\thisIsNotAValidFileFormat.txt")); });
            File.Delete(Directory.GetCurrentDirectory() + @"\thisIsNotAValidFileFormat.txt");
            Assert.IsFalse(File.Exists(Directory.GetCurrentDirectory() + @"\thisIsNotAValidFileFormat.txt"));
        }
    } 
}
