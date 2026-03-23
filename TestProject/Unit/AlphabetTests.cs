using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;

namespace TestProject.Unit;

[TestClass]
public class AlphabetTests
{
    [TestMethod]
    public void CreateLatinAlphabet_ShouldSucceed()
    {
        IAlphabet alphabet = new LatinAlphabet();
        Assert.IsNotNull(alphabet);

        Assert.IsTrue(alphabet.GetChars().SequenceEqual(new[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'
        }));

        Assert.AreEqual(26, alphabet.GetLength());
    }

    [TestMethod]
    public void GetIndexOfCharHInLatinAlphabet_ShouldReturn7()
    {
        IAlphabet alphabet = new LatinAlphabet();
        int index = alphabet.GetCharPositionInArray('h');
        Assert.AreEqual(7, index);
    }

    [TestMethod]
    public void CreateCustomAlphabet_ShouldSucceed()
    {
        char[] germanChars = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
        IAlphabet alphabet = new CustomAlphabet(germanChars);

        Assert.IsNotNull(alphabet);
        Assert.IsTrue(alphabet.GetChars().SequenceEqual(germanChars));
        Assert.AreEqual(germanChars.Length, alphabet.GetLength());
    }

    [TestMethod]
    public void GetIndexOfCharHInCustomAlphabet_ShouldReturn7()
    {
        char[] germanChars = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
        IAlphabet alphabet = new CustomAlphabet(germanChars);

        int index = alphabet.GetCharPositionInArray('h');
        Assert.AreEqual(7, index);
    }

    [TestMethod]
    public void GetIndexOfCharÜInCustomAlphabet_ShouldReturn28()
    {
        char[] germanChars = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
        IAlphabet alphabet = new CustomAlphabet(germanChars);

        int index = alphabet.GetCharPositionInArray('ü');
        Assert.AreEqual(28, index);
    }

    [TestMethod]
    public void CreateAlphabetWithDuplicateCharactersInArray_ShouldThrowError()
    {
        Assert.ThrowsException<DuplicateAlphabetCharacterException>(() =>
            new CustomAlphabet(new[] { 'a', 'b', 'c', 'c' }));
    }

    [TestMethod]
    public void SerializeAndDeserializeAlphabet_ShouldSucceed()
    {
        char[] germanChars = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ü', 'ß' };
        IAlphabet alphabet = new CustomAlphabet(germanChars);

        byte[] serialized = CustomAlphabet.Serialize(alphabet);
        Assert.IsNotNull(serialized);

        Assert.AreEqual(sizeof(int) + germanChars.Length * sizeof(char), serialized.Length);

        CustomAlphabet deserialized = CustomAlphabet.Deserialize(serialized);
        Assert.IsNotNull(deserialized);

        Assert.IsTrue(deserialized.GetChars().SequenceEqual(alphabet.GetChars()));
        Assert.AreEqual(alphabet.GetLength(), deserialized.GetLength());
    }
}