namespace SpellCheckingTool
{
    public class Word
    {
        string word;

        public Word(IAlphabet alphabet, string word)
        {
            this.word = word;
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
    }
}
