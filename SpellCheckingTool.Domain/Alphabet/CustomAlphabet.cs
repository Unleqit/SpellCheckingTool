namespace SpellCheckingTool.Domain.Alphabet;
    public class CustomAlphabet : BaseAlphabet
    {
        int[] mapping;

        //call the base constructor of the BaseAlphabet class to bundle common logic
        public CustomAlphabet(char[] chars) : base(chars)
        {
            this.mapping = new int[65536];

            //map chars to lookup array for quicker access
            for (int i = 0; i < chars.Length; ++i)
                mapping[chars[i]] = i;
        }

        //Note: non-ASCII characters ('ä', è, î, ...) are scattered all over the place in the unicode table, and searching for the location of each of them in the alphabets
        //character array is far too expensive (O(n)), therefore this mapping technique is used.
        //Example: Suppose the character 'ä' is supplied as the 27. letter of the german alphabet.
        //At the position of 'ä' in the unicode table, 228, which is obtainable via `(int)'ä'`, the 27 gets saved (e.g.: `this.mapping[228] = 27;`).
        //This way, when searching for 'ä' in the custom alphabets' character table (used when walking the WordTree while performing add/search/delete operations for words), we can simply look
        //at the position 228 of its mapping table, which stores the position 27, thus creating a feasible O(1) solution.
        public override int GetCharPositionInArray(char c)
        {
            return this.mapping[c];
        }

    }
