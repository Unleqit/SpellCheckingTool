using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public unsafe class FilePersistenceService : IPersistenceService
    {
        private WalkWordTreeService walkWordTreeService;

        public FilePersistenceService(WordTree tree)
        {
            this.walkWordTreeService = new WalkWordTreeLeftToRightService(tree);
        }

        public FilePersistenceService(WalkWordTreeService walkWordTreeService)
        {
            this.walkWordTreeService = walkWordTreeService;
        }

        //FILE LAYOUT:
        //Bytes                                                                                             Data
        //[0-4]                                                                                             word count of the tree
        //[5-8]                                                                                             node count of the ree
        //[9-12]                                                                                            length of the alphabet used to build the tree
        //[13-16]                                                                                           amount of padding bytes at the end of the alphabet content
        //[17-20]                                                                                           length of all words in the tree joined together by a separator ('\n') => totalWordLength
        //[21 + serializedAlphabetLength]                                                                   content of the alphabet used to build the tree
        //[21 + serializedAlphabetLength + alphabetPadding]                                                 content of the word indices buffer, which holds the starting position of each word serialized into the file
        //[21 + serializedAlphabetLength + alphabetPadding + (tree.metaData.wordCount + 1) * sizeof(int)]   content of the tree
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

            //this is the meta index buffer and the index in said buffer
            int* wordIndicesBuffer = (int*)(fileContent) + 5 + (serializedAlphabetLength / sizeof(int));
            int wordIndicesBufferIndex = 0;

            //write all required metainfo to the file buffer
            *((int*)(fileContent) + 0) = tree.metaData.wordCount;
            *((int*)(fileContent) + 1) = tree.metaData.nodeCount;
            *((int*)(fileContent) + 2) = serializedAlphabetLength;
            *((int*)(fileContent) + 3) = alphabetPadding;
            *((int*)(fileContent) + 4) = tree.metaData.serializationLength;

            //copy the serialized alphabet into the file buffer
            for (int i = 0; i < serializedAlphabetLength; ++i)
                *(((byte*)fileContent) + 5 * sizeof(int) + i) = serializedAlphabet[i];


            //walk the wordTree using the provided WalkWordTreeService, appending each word (contained in the native char* wordBuffer) in the tree to the file
            this.walkWordTreeService.WalkTree((string wordBuffer) =>
            {
                //save beginning of current word to index partition of file
                *(wordIndicesBuffer + wordIndicesBufferIndex++) = fileLength;

                //the current node represents the last character of a word stored in the tree - save it by copying it to the file buffer and appending a unix-style line separator after it
                for (int copyIndex = 0; copyIndex < wordBuffer.Length; ++copyIndex)
                    *(fileContent + fileLength++) = wordBuffer[copyIndex];
                *(fileContent + fileLength++) = '\n';
            });

            //write final word info
            *(wordIndicesBuffer + wordIndicesBufferIndex) = fileLength;

            //write the data to a file
            int result = API._saveFile(path, (byte*)fileContent, fileLength * sizeof(char));
            if (result != 0)
                throw new Exception("Native file save failed");

            //zero pointers
            fileContent = null;

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

            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = deserializedAlphabet });
            Word[] parsedWords = Word.ParseWords(deserializedAlphabet, words);
            tree.Add(parsedWords);
            return tree;
        }
    }
}
