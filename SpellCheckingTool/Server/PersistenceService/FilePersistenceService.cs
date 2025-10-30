using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SpellCheckingTool
{
    public unsafe class FilePersistenceService : IPersistenceService
    {
        //FILE LAYOUT:
        //Bytes                                                                                             Data
        //[0-4]                                                                                             word count of the tree
        //[5-8]                                                                                             node count of the ree
        //[9-12]                                                                                            length of the alphabet used to build the tree
        //[13-16]                                                                                           amount of padding bytes at the end of the alphabet content
        //[17-20]                                                                                           length of all words in the tree joined together by a separator ('\n') => totalWordLength
        //[17 + serializedAlphabetLength]                                                                   content of the alphabet used to build the tree
        //[17 + serializedAlphabetLength + alphabetPadding]                                                 content of the word indices buffer, which holds the starting position of each word serialized into the file
        //[17 + serializedAlphabetLength + alphabetPadding + (tree.metaData.wordCount + 1) * sizeof(int)]   content of the tree
        /// <summary>
        /// Serializes a WordTree into a .wdb file
        /// </summary>
        /// <param name="tree">The tree to be serialized</param>
        /// <param name="filepath">The path to save the serialized tree to. Must be a valid directory path followed by a customly selectable file name ending with ".wdb"</param>
        /// <returns>Returns a boolean indicating whether the serialization process was successful</returns>
        /// <exception cref="Exception">Throws an Exception, if the file path is invalid or the native file save operation fails.</exception>
        public bool Save(WordTree tree, FilePath filepath)
        {
            string path = filepath.Path;

            if (tree == null)
                throw new Exception("Tree is unspecified");

            if (!path.EndsWith(".wdb"))
                throw new Exception("File not supported");

            //the two hardest things in computer science are cache invalidation, naming things, and off-by-one errors
            //we need the one extra space, as the root node of the tree gets passed as well
            WordTreeNode[] stack = new WordTreeNode[tree.metaData.wordBufferLength + 1];
            int stackIndex = 0;

            //serialize alphabet
            byte[] serializedAlphabet = BaseAlphabet.Serialize(tree.alphabet);
            int serializedAlphabetLength = serializedAlphabet.Length;

            //the serializedAlphabetLength property needs to be aligned to sizeof(int) bytes
            int alphabetPadding = 0;
            if ((alphabetPadding = serializedAlphabetLength % sizeof(int)) != 0)
                serializedAlphabetLength += alphabetPadding;

            //offset the file content by the amount of indices required for the start of each word in the file + the space required to
            //hold the number of those indices + end of last word for convenience
            int metaOffset = 5 * sizeof(int) + (tree.metaData.wordCount + 1) * sizeof(int) + serializedAlphabetLength;
            int serializationLength = tree.metaData.serializationLength;

            //'fileContent' is a char pointer with an element width of sizeof(char), while 'metaOffset' interprets it as byte pointer
            //with the element width sizeof(byte), hence we need to align it
            char* fileContent = (char*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(char) * serializationLength + metaOffset) : API.linux_malloc(sizeof(char) * serializationLength + metaOffset));

            int fileLength = metaOffset / sizeof(char);

            //this is a native string used to extract each word from the tree and write it to the serialization file
            char* wordBuffer = (char*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(char) * tree.metaData.wordBufferLength) : API.linux_malloc(sizeof(char) * tree.metaData.wordBufferLength));
            int wordBufferIndex = 0;

            //this is the meta index buffer and the index in said buffer
            int* wordIndicesBuffer = (int*)(fileContent) + 5 + (serializedAlphabetLength / sizeof(int));
            int wordIndicesBufferIndex = 0;

            char[] alphabetChars = tree.alphabet.GetChars();

            //write all required metainfo to the file buffer
            *((int*)(fileContent) + 0) = tree.metaData.wordCount;
            *((int*)(fileContent) + 1) = tree.metaData.nodeCount;
            *((int*)(fileContent) + 2) = serializedAlphabetLength;
            *((int*)(fileContent) + 3) = alphabetPadding;
            *((int*)(fileContent) + 4) = tree.metaData.serializationLength;

            //copy the serialized alphabet into the file buffer
            for (int i = 0; i < serializedAlphabetLength; ++i)
                *(((byte*)fileContent) + 5 * sizeof(int) + i) = serializedAlphabet[i];

            //we can't use recursion, as deep trees may cause a StackOverflowException, hence we walk the tree in an iterative manner.
            //in order for this to work efficiently, without having to manage a deep stack, we leverage the 'WordBufferLength' property of the tree
            stack[stackIndex++] = tree.rootNode;
            WordTreeNode current = tree.rootNode;

            //cache alphabetLength
            int alphabetLength = tree.alphabet.GetLength();

            //traverse the tree as long as the stack contains elements
            while (stackIndex > 0)
            {
                //all childs of this node were visited already, hence it can be popped
                if (current.reserved == alphabetLength)
                {
                    current = stack[stackIndex - 1];
                    --wordBufferIndex;
                }

                //traverse to bottom of tree to get first whole word
                //used is the bitwise property indicating which childs are in use, hence the value of the used property will be zero for leaf nodes without children 
                while (current.reserved < alphabetLength)
                {
                    //find smallest child of node which is yet to be serialized and save its index in the reserved property of the node
                    for (int characterIndex = current.reserved; characterIndex < alphabetLength + 0; ++characterIndex)
                        if (current.Nodes[characterIndex] != null)
                        {
                            //the leftmost non-zero child pointer is traversed in order to find its children and pushed on the stack for backtracing
                            stack[stackIndex++] = current.Nodes[characterIndex];
                            wordBuffer[wordBufferIndex++] = alphabetChars[characterIndex];

                            //examine the next child on future visits, as this one is non-null
                            current.reserved = characterIndex + 1;

                            //take a look at the leftmost child of this node and interrupt the traversal for this one
                            //the index of the first child node after this one that has yet to be checked is stored in the 'reserved' property (may be null))
                            current = current.Nodes[characterIndex];
                            break;
                        }
                        else
                            //mark note as 'traversed', as it is null and contains no children
                            current.reserved = characterIndex + 1;
                }

                //check if nodes where all child nodes have been traversed represent a word themselves; if not, pop it from the stack and resume with the next node
                if (current.reserved == alphabetLength && current.IsWord != true)
                {
                    stackIndex--;
                    continue;
                }

                //save beginning of current word to index partition of file
                *(wordIndicesBuffer + wordIndicesBufferIndex++) = fileLength;

                //the current node represents the last character of a word stored in the tree - save it by copying it to the file buffer and appending a unix-style line separator after it
                for (int copyIndex = 0; copyIndex < wordBufferIndex; ++copyIndex)
                    *(fileContent + fileLength++) = *(wordBuffer + copyIndex);
                *(fileContent + fileLength++) = '\n';

                //the current node and its children have been fullt examined and saved - pop it from the stack and resume with the next node
                stackIndex--;
            }

            //write final word info
            *(wordIndicesBuffer + wordIndicesBufferIndex) = fileLength;

            //write the data to a file
            int result = API._saveFile(path, (byte*)fileContent, fileLength * sizeof(char));
            if (result != 0)
                throw new Exception("Native file save failed");
       /* */

            //free unmanaged resources
        //    Marshal.FreeHGlobal((IntPtr)fileContent);
        //    Marshal.FreeHGlobal((IntPtr)wordBuffer);

            //zero pointers
            fileContent = null;
            wordBuffer = null;

            return result == 0;
        }

        /// <summary>
        /// Loads a serialized .wdb file and parses it into a WordTree
        /// </summary>
        /// <param name="filepath">The path to load the file from</param>
        /// <returns>The parsed WordTree</returns>
        /// <exception cref="Exception">Throws Exceptions, if the native file open process fails</exception>
        public WordTree Load(FilePath filepath)
        {
            string path = filepath.Path;

            if (!path.EndsWith(".wdb"))
                throw new Exception("File not supported");

            byte* fileContent;
            int fileSize = API._openFile(path, &fileContent);

            if (fileSize < 0)
                throw new Exception("Native file open failed");

            //parse offsets from file
            int wordCount = *(int*)(fileContent + 0);
            int nodeCount = *(int*)(fileContent + sizeof(int));
            int serializedAlphabetLength = *(int*)(fileContent + 2 * sizeof(int));
            int alphabetPadding = *(int*)(fileContent + 3 * sizeof(int));
            int totalWordLength = *(int*)(fileContent + 4 * sizeof(int));
            int wordIndicesBufferIndex = 5 * sizeof(int) + serializedAlphabetLength + alphabetPadding;
            int wordIndicesBufferLength = (wordCount + 1) *sizeof(int);


            //check file integrity
            if ((fileSize - (5 * sizeof(int) + serializedAlphabetLength + alphabetPadding + wordIndicesBufferLength + sizeof(char) * totalWordLength)) > sizeof(char)) //account for newline weirdness
                throw new Exception("Invalid .wdb file");

            //parse alphabet to be used for this tree
            byte[] serializedAlphabet = new byte[serializedAlphabetLength];
            //convert to managed memory for interoperability
            for (int i = 0; i < serializedAlphabetLength; ++i)
                serializedAlphabet[i] = *(((byte*)fileContent) + 5 * sizeof(int) + i);

            IAlphabet deserializedAlphabet = BaseAlphabet.Deserialize(serializedAlphabet);

            string[] words = new string[wordCount];
            char[] alphabetChars = deserializedAlphabet.GetChars();

            int start = 0;
            int wordIndex = 0;
            int wordLength = 0;
            int maxLength = 0;
            char* word = null;


            //populate the words array with the words from the .wdb file
            for (int i = 0; i < wordCount; ++i)
            {
                start = *(int*)(fileContent + wordIndicesBufferIndex);
                wordLength = ((*(int*)(fileContent + sizeof(int) + wordIndicesBufferIndex) - start)) - 1;
                word = (char*)(fileContent) + start;
                wordIndicesBufferIndex += sizeof(int);

                //keep track of wordBufferLength property while parsing
                if (wordLength > maxLength)
                    maxLength = wordLength;

                //add words from the file to the string[] words
                for (wordIndex = 0; wordIndex < wordLength; ++wordIndex)
                {
                    words[i] += word[wordIndex];
                }
            }

            //remove any empty words
            words = words.Where((w) => w != null).ToArray();

            WordTree tree = new WordTree(deserializedAlphabet, new FilePersistenceService());
            Word[] parsedWords = Word.ParseWords(deserializedAlphabet, words);
            tree.Add(parsedWords);
            return tree;
        }
    }
}
