namespace SpellCheckingTool.Domain.Alphabet;
    public class ExecutableNameAlphabet : CustomAlphabet
    {
        public ExecutableNameAlphabet() : base(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_', '-'  })
        {
            //nothing to do here, everything gets handled by the base class
        }

        //subtract the offsets of two chars in the ASCII table to get their location in the char array
        public override int GetCharPositionInArray(char c)
        {
            return base.GetCharPositionInArray(c);
        }

    }
