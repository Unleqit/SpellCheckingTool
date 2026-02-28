using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.WalkWordTreeService;

    public interface IWalkWordTreeService
    {
        public void WalkTree(Action<Word> onEachWord);
    }
