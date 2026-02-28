namespace SpellCheckingTool.Domain.WordTree;
    public class WordTreeMetaInfo
    {
        private int _wordCount;
        private int _nodeCount;
        private int _serializationLength;
        private int _wordBufferLength;

        public WordTreeMetaInfo(int wordCount, int nodeCount, int serializationLength, int wordBufferLength)
        {
            this._wordCount = wordCount;
            this._nodeCount = nodeCount;
            this._serializationLength = serializationLength;
            this._wordBufferLength = wordBufferLength;
        }

        public int wordCount
        {
            get
            {
                return _wordCount;
            }
            set
            {
                if (value >= 0)
                    _wordCount = value;
            }
        }

        public int nodeCount
        {
            get
            {
                return _nodeCount;
            }
            set
            {
                if (value >= 0)
                    _nodeCount = value;
            }
        }

        public int serializationLength
        {
            get
            {
                return _serializationLength;
            }
            set
            {
                if (value >= 0)
                    _serializationLength = value;
            }
        }

        public int wordBufferLength
        {
            get
            {
                return _wordBufferLength;
            }
            set
            {
                if (value >= 0)
                    _wordBufferLength = value;
            }
        }
    }
