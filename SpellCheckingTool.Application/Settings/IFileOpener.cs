using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Application.Settings
{
    public interface IFileOpener
    {
        void Open(string filePath);
    }
}
