using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public interface IWalkWordTreeService
    {
        public void WalkTree(Action<Word> onEachWord);
    }
}
