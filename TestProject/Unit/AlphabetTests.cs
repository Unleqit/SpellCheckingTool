using SpellCheckingTool;

namespace TestProject.Unit
{
    [TestClass]
    public class AlphabetTests
    {
        [TestMethod]
        public void CreateLatinAlphabet_ShouldSucceed()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Assert.IsTrue(alphabet != null);
            Assert.AreEqual(alphabet.GetChars().SequenceEqual(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }), true);
            Assert.AreEqual(alphabet.GetLength(), 26);
        }

        [TestMethod]
        public void GetIndexOfCharHInLatinAlphabet_ShouldReturn7()
        {
            IAlphabet alphabet = new LatinAlphabet();
            Assert.IsTrue(alphabet != null);
            int indexOfCharHInAlphabet = alphabet.GetCharPositionInArray('h');
            Assert.AreEqual(indexOfCharHInAlphabet, 7);
        }

        [TestMethod]
        public void CreateCustomAlphabet_ShouldSucceed()
        {
            char[] germanChars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
            IAlphabet alphabet = new CustomAlphabet(germanChars);
            Assert.IsTrue(alphabet != null);
            Assert.AreEqual(alphabet.GetChars(), germanChars);
            Assert.AreEqual(alphabet.GetLength(), germanChars.Length);
        }

        [TestMethod]
        public void GetIndexOfCharHInCustomAlphabet_ShouldReturn7()
        {
            char[] germanChars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
            IAlphabet alphabet = new CustomAlphabet(germanChars);
            Assert.IsTrue(alphabet != null);
            int indexOfCharHInAlphabet = alphabet.GetCharPositionInArray('h');
            Assert.AreEqual(indexOfCharHInAlphabet, 7);
        }

        [TestMethod]
        public void GetIndexOfCharÜInCustomAlphabet_ShouldReturn28()
        {
            char[] germanChars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
            IAlphabet alphabet = new CustomAlphabet(germanChars);
            Assert.IsTrue(alphabet != null);
            int indexOfCharHInAlphabet = alphabet.GetCharPositionInArray('ü');
            Assert.AreEqual(indexOfCharHInAlphabet, 28);
        }

        [TestMethod]
        public void CreateAlphabetWithDuplicateCharactersInArray_ShouldThrowError()
        {
            Assert.ThrowsException<Exception>(() => { IAlphabet alphabet = new CustomAlphabet(new char[] { 'a', 'b', 'c', 'c' }); });
        }

        [TestMethod]
        public void SerializeAndDeserializeAlphabet_ShouldSucceed()
        {
            char[] germanChars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
            IAlphabet alphabet = new CustomAlphabet(germanChars);
            byte[] serializedAlphabet = CustomAlphabet.Serialize(alphabet);
            Assert.IsTrue(serializedAlphabet != null);
            Assert.AreEqual(serializedAlphabet.Length, sizeof(int) + germanChars.Length * (sizeof(char)));
            CustomAlphabet deserializedAlphabet = CustomAlphabet.Deserialize(serializedAlphabet);
            Assert.IsTrue(deserializedAlphabet != null);
            Assert.IsTrue(deserializedAlphabet.GetChars().SequenceEqual(alphabet.GetChars()));
            Assert.AreEqual(deserializedAlphabet.GetLength(), alphabet.GetLength());
        }
    }
}
