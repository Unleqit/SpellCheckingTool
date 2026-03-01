using System.Text;

namespace SpellCheckingTool.Domain.Alphabet;

    //TODO: This class still contains unsafe pointers, needs to be adjusted
    public abstract class BaseAlphabet : IAlphabet
    {
        
        protected char[] chars;
        protected int length;

        //prepare the construction of an alphabet object
        public BaseAlphabet(char[] chars)
        {
            //check for duplicates - lovely O(n^2) solution using LINQ
            if (chars.Distinct().Count() != chars.Length)
                throw new Exception("Duplicate characters in alphabet array");

            this.length = chars.Length;
            this.chars = chars;
        }

        public virtual char[] GetChars()
        {
            return chars;
        }

        public virtual int GetLength()
        {
            return length;
        }

        //must be implemented by each deriving class
        //example: the chars 'a' to 'z' are adjacent to each other in the alphabet, while this is not the case when including the german special characters 'ä', 'ö', 'ü', 'ß'
        public abstract int GetCharPositionInArray(char c);


        public static unsafe byte[] Serialize(IAlphabet alphabet)
        {
            byte[] chars = Encoding.Unicode.GetBytes(alphabet.GetChars());
            byte[] result = new byte[sizeof(int) + chars.Length];

            //set the length of the type info and alphabet
            fixed (byte* pResult = result)
                *(int*)pResult = alphabet.GetLength();

            //copy the character array into the result array  at index `sizeof(int)` (=4)
            chars.CopyTo(result, sizeof(int));

            return result;
        }

        public static unsafe CustomAlphabet Deserialize(byte[] serializedAlphabet)
        {
            int charArrayLength;

            fixed (byte* pSerializedAlphabet = serializedAlphabet)
                charArrayLength = *(int*)pSerializedAlphabet;

            //Unicode, hence two bytes width for each char. C# has sizeof(char) = 2 defined usually.
            if (charArrayLength * sizeof(char) != serializedAlphabet.Length - sizeof(int))
                throw new Exception("Invalid serialized alphabet");

            char[] chars = Encoding.Unicode.GetChars(serializedAlphabet, sizeof(int), charArrayLength * sizeof(char));

            return new CustomAlphabet(chars);
        }
    }
