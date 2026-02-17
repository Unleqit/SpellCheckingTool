using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public unsafe class WalkWordTreeLeftToRightService : WalkWordTreeService, IDisposable
    {
        public WalkWordTreeLeftToRightService(WordTree tree) : base(tree)
        {
            //initialize service by calling base constructor
        }

        /// <summary>
        /// Iteratively walks the tree in order and calls the provided delegate method for each word contained in the tree.
        /// </summary>
        /// <param name="onEachWord">
        /// The first parameter of the delegate is a char* to the wordBuffer holding the current word in native memory, wrapped as a long.
        /// The second parameter of the delegate is the length of the word in native memory
        /// </param>
        public unsafe override void WalkTree(Action<string> onEachWord)
        {
            //we can't use recursion, as deep trees may cause a StackOverflowException, hence we walk the tree in an iterative manner.
            stack[stackIndex++] = tree.rootNode;
            WordTreeNode current = tree.rootNode;

            //the reserved property of each node in the tree gets "modified" on each traversal in order to keep track of all visited nodes.
            //resetting these to 0 for each node in the tree after each traversal would be too expensive, therefore this approach is employed:
            //The system has two states: The reserved property of the root node (and therefore all others in the tree) is 0 ("A run"), or it is 'alphabetLength' ("B run")
            //thse values are used to dynamically flip the bounds of loops etc. depending on whether this traversal is an "A run", or a "B run"
            int reserved_maxValue = tree.rootNode.reserved == 0 ? alphabetLength : 0;
            int reserved_incrementValue = tree.rootNode.reserved == 0 ? 1 : -1;
            int reserved_invertedMaxValue = tree.rootNode.reserved == 0 ? 0 : alphabetLength;

            while (stackIndex > 0)
            {
                //all childs of this node were visited already, hence it can be popped
                if (current.reserved == reserved_maxValue)
                {
                    current = stack[stackIndex - 1];
                    --wordBufferIndex;
                }

                //traverse to bottom of tree to get first whole word
                //used is the bitwise property indicating which childs are in use, hence the value of the used property will be zero for leaf nodes without children 
                while (current.reserved != reserved_maxValue)
                {
                    //find smallest child of node which is yet to be serialized and save its index in the reserved property of the node
                    for (int characterIndex = current.reserved; characterIndex != reserved_maxValue; characterIndex += reserved_incrementValue)
                        if (current.Nodes[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue] != null)
                        {
                            //the leftmost non-zero child pointer is traversed in order to find its children and pushed on the stack for backtracing
                            stack[stackIndex++] = current.Nodes[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue];
                            wordBuffer[wordBufferIndex++] = alphabetChars[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue];

                            //examine the next child on future visits, as this one is non-null
                            current.reserved = characterIndex + reserved_incrementValue;

                            //take a look at the leftmost child of this node and interrupt the traversal for this one
                            //the index of the first child node after this one that has yet to be checked is stored in the 'reserved' property (may be null))
                            current = current.Nodes[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue];
                            break;
                        }
                        else
                            //mark note as 'traversed', as it is null and contains no children
                            current.reserved = characterIndex + reserved_incrementValue;
                }

                //check if nodes where all child nodes have been traversed represent a word themselves; if not, pop it from the stack and resume with the next node
                if (current.reserved == reserved_maxValue && current.IsWord != true)
                {
                    stackIndex--;
                    continue;
                }

                //call onEachWord delegate with a char* to the word buffer pointer wrapped as a long (due to managed code restrictions in delegates), and pass the length of the word in the native buffer as second argument

                char[] a = new char[wordBufferIndex];
                for (int i = 0; i < wordBufferIndex; ++i)
                    a[i] = wordBuffer[i];
                string s = new string(a);
                
                onEachWord(s);

                //the current node and its children have been fullt examined and saved - pop it from the stack and resume with the next node
                stackIndex--;
            }
        }
    }
}
