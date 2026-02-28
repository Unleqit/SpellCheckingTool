using SpellCheckingTool.Application.WalkWordTreeService;
using SpellCheckingTool.Domain.WordTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Application.PersistenceService;
    public interface IPersistenceService
    {
        bool Save(WordTree tree, FilePath filepath, IWalkWordTreeService walkWordTreeService);
        WordTree Load(FilePath filepath);
    }
