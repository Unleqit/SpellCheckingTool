namespace SpellCheckingTool
{
    public class Word
    {
        string word;

        public Word(IAlphabet alphabet, string word)
        {
            this.word = word.ToLower();
            char[] alphabetChars = alphabet.GetChars();

            if (this.word.Any(c => !alphabetChars.Contains(c)))
                throw new Exception("Word contains invalid characters");
        }

        //define array operator, length property and ToString()-Method on Word class for convenience
        public char this[int i]
        {
            get { return this.word[i]; }
        }

        public int Length
        {
            get { return word.Length; }
        }

        public override string ToString()
        {
            return word.ToString();
        }

        //convenience method
        public static Word[] ParseWords(IAlphabet alphabet, string[] rawWords)
        {
            List<Word> words = new List<Word>();
            for (int i = 0; i < rawWords.Length; ++i)
            {
                try
                {
                    Word word = new Word(alphabet, rawWords[i]);
                    words.Add(word);
                }
#pragma warning disable CS0168
                catch (Exception e)
                {
                    //just skip this word and do not add it to the list
                    continue;
                }
#pragma warning restore CS0168
            }
            return words.ToArray();
        }
    }
}
