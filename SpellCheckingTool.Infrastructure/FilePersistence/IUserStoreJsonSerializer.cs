using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Infrastructure.FilePersistence
{
    public interface IUserStoreJsonSerializer
    {
        T ReadOrDefault<T>(string path, T defaultValue);
        void Write<T>(string path, T data);
    }
}
