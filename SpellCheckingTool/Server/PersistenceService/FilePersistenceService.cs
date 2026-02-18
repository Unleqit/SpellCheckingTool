using Newtonsoft.Json;

namespace SpellCheckingTool
{
    public unsafe class FilePersistenceService : IPersistenceService
    {
        private IWalkWordTreeService walkWordTreeService;

        public FilePersistenceService(WordTree tree)
        {
            this.walkWordTreeService = new WalkWordTreeLeftToRightService(tree);
        }

        public FilePersistenceService(IWalkWordTreeService walkWordTreeService)
        {
            this.walkWordTreeService = walkWordTreeService;
        }

        /// <summary>
        /// Serializes a WordTree into a .json file
        /// </summary>
        /// <param name="tree">The tree to be serialized</param>
        /// <param name="filepath">The path to save the serialized tree to. Must end with ".json"</param>
        /// <returns>Returns a boolean indicating whether the serialization process was successful</returns>
        /// <exception cref="Exception">Throws an Exception, if the file path is invalid or the file save operation failed.</exception>
        public bool Save(WordTree tree, FilePath filepath, IWalkWordTreeService walkWordTreeService)
        {
            string path = filepath.Path;

            if (tree == null)
                throw new Exception("Tree is unspecified");

            if (!path.EndsWith(".json"))
                throw new Exception("File not supported");

            string[] words = new string[tree.metaData.wordCount];
            int i = 0;

            walkWordTreeService.WalkTree((word) => words[i++] = word.ToString());

            WordTreeDto wordTreeDto = new WordTreeDto()
            {
                alphabet = tree.alphabet.GetChars(),
                words = words
            };

            string wordFileJson = JsonConvert.SerializeObject(wordTreeDto, Formatting.Indented);
            File.WriteAllText(path, wordFileJson);

            return true;
        }

        /// <summary>
        /// Loads a serialized .json file and parses it into a WordTree
        /// </summary>
        /// <param name="filepath">The path to load the file from</param>
        /// <returns>The parsed WordTree</returns>
        /// <exception cref="Exception">Throws Exceptions, if the file open process failed</exception>
        public WordTree Load(FilePath filepath)
        {
            string path = filepath.Path;

            if (!path.EndsWith(".json"))
                throw new Exception("File not supported");

            string wordTreeJson = File.ReadAllText(path);

            WordTreeDto wordTreeDto = JsonConvert.DeserializeObject<WordTreeDto>(wordTreeJson);
            if (wordTreeDto == null)
                throw new Exception("Deserialization of wordfile failed!");

            IAlphabet alphabet = new CustomAlphabet(wordTreeDto.alphabet);

            WordTree tree = new WordTree(new WordTreeParameters() { alphabet = alphabet });
            Word[] parsedWords = Word.ParseWords(alphabet, wordTreeDto.words);

            tree.Add(parsedWords);
            return tree;
        }
    }
}
