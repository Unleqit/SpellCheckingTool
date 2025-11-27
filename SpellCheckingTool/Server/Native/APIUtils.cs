using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public unsafe partial class API
    {
        /// <summary>
        /// Convenience method handling platform-specific malloc()-calls.
        /// </summary>
        public static void* malloc(long length)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(length) : API.linux_malloc(length);
        }

        /// <summary>
        /// Convenience method handling platform-specific realloc()-calls.
        /// </summary>
        public static void* realloc(void* ptr, long length)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_realloc(ptr, length) : API.linux_realloc(ptr, length);
        }

        /// <summary>
        /// Returns zero if the file was saved successfully.
        /// </summary>
        [MethodImpl(256)]
        public static int _saveFile(string szPathName, byte* lpBits, long size)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr hFile = API.windows_fopen(szPathName, "wb");
                if (hFile == IntPtr.Zero)
                    return -1;

                uint tmp = (uint)API.windows_fwrite(lpBits, 1, (int)size, hFile);

                if (tmp != size)
                    return -1;

                return API.windows_fclose(hFile);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                IntPtr hFile = API.linux_fopen(szPathName, "wb");
                if (hFile <= IntPtr.Zero)
                  return -1;

                uint tmp = (uint)API.linux_fwrite(lpBits, 1, (int)size, hFile);

                if (tmp != size)
                   return -1;

                return API.linux_fclose(hFile);
            }
            else return -1; //not supported
        }

        /// <summary>
        /// Returns the file size if the file was loaded successfully, or -1.
        /// </summary>
        [MethodImpl(256)]
        public static int _openFile(string filename, byte** data)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (data == null)
                    return -1;

                IntPtr hFile = API.windows_fopen(filename, "rb");

                if (hFile == IntPtr.Zero)
                    return -1;

                long sz = _fsize(hFile);
                *data = (byte*)API.windows_malloc(sz);

                int read = API.windows_fread(*data, 1, (int)sz, hFile);

                if (read != sz)
                    return -1;

                return API.windows_fclose(hFile) == 0 ? (int)sz : -1;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (data == null)
                    return -1;

                IntPtr hFile = API.linux_fopen(filename, "rb");

                if (hFile == IntPtr.Zero)
                    return -1;

                long sz = _fsize(hFile);
                *data = (byte*)API.linux_malloc(sz);

                int read = API.linux_fread(*data, 1, (int)sz, hFile);

                if (read != sz)
                    return -1;

                return API.linux_fclose(hFile) == 0 ? (int)sz : -1;
            }
            else return -1; //not supported
        }
        

        /// <summary>
        /// Returns the size of a file provided by the corresponding handle
        /// </summary>
        /// <param name="hFile"></param>
        /// <returns></returns>
        public static long _fsize(IntPtr hFile)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                int err = API.windows_fseek(hFile, 0, 2);
                long sz = API.windows_ftell(hFile);

                if (err != 0)
                    return -1;

                err = API.windows_fseek(hFile, 0, 0);
                return err == 0 ? sz : -1;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                int err = API.linux_fseek(hFile, 0, 2);
                long sz = API.linux_ftell(hFile);
                API.linux_fseek(hFile, 0, 0);
                return sz;
            }
            else return -1; //not supported
        }
    }
}
