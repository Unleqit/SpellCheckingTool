using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public unsafe partial class API
    {
        //--------------------------------------------------Native C APIs for Windows-----------------------------------------------------------

        [DllImport("msvcrt.dll", SetLastError = false, EntryPoint = "malloc")]
        public static extern void* windows_malloc(long size);

        [DllImport("msvcrt.dll", SetLastError = false, EntryPoint = "free")]
        public static extern void windows_free(void* ptr);

        [DllImport("msvcrt.dll", SetLastError = true, EntryPoint = "_msize")]
        public static extern int windows__msize(void* ptr);

        /// <summary>
        /// <para> r     - Open for reading. Must exist.</para>
        /// <para> w     - Open for writing. Overwrites existing file. Creates if not exists.</para>
        /// <para> a     - Open for appending. Creates if not exists. Appends writes to end.</para>
        /// <para> r+    - Open for reading and writing. Must exist.</para>
        /// <para> w+    - Open for reading and writing. Overwrites existing file. Creates if not exists.</para>
        /// <para> a+    - Open for reading and appending. Creates if not exists. Appends writes to end.</para>
        /// <para> rb    - Open for reading in binary mode. Must exist.</para>
        /// <para> wb    - Open for writing in binary mode. Overwrites existing file. Creates if not exists.</para>
        /// <para> ab    - Open for appending in binary mode. Creates if not exists. Appends writes to end.</para>
        /// <para> rb+   - Open for reading and writing in binary mode. Must exist.</para>
        /// <para> wb+   - Open for reading and writing in binary mode. Overwrites existing file. Creates if not exists.</para>
        /// <para> ab+   - Open for reading and appending in binary mode. Creates if not exists. Appends writes to end.</para>
        /// </summary>
        [DllImport("msvcrt.dll", SetLastError = true, EntryPoint = "fopen")]
        public static extern IntPtr windows_fopen(string filename, string mode);

        [DllImport("msvcrt.dll", SetLastError = true, EntryPoint = "fread")]
        public static extern int windows_fread(void* buffer, int size, int number, IntPtr file);

        [DllImport("msvcrt.dll", SetLastError = true, EntryPoint = "fwrite")]
        public static extern int windows_fwrite(void* buffer, int size, int number, IntPtr file);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true, EntryPoint = "fclose")]
        public static extern int windows_fclose(IntPtr file);

        /// <summary>
        /// <para> SEEK_SET = 0</para>
        /// <para> SEEK_CUR = 1</para>
        /// <para> SEEK_END = 2</para>
        /// </summary>
        [DllImport("msvcrt.dll", SetLastError = true, EntryPoint = "fseek")]
        public static extern int windows_fseek(IntPtr stream, long offset, int origin);

        [DllImport("msvcrt.dll", SetLastError = true, EntryPoint = "ftell")]
        public static extern long windows_ftell(IntPtr stream);


        //---------------------------------------------------Native C APIs for Linux------------------------------------------------------------


        [DllImport("libc", EntryPoint = "malloc", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* linux_malloc(long size);

        [DllImport("libc", EntryPoint = "free", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linux_free(void* ptr);

        [DllImport("libc", EntryPoint = "malloc_usable_size", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linux__msize(void* ptr);

        /// <summary>
        /// <para> r     - Open for reading. Must exist.</para>
        /// <para> w     - Open for writing. Overwrites existing file. Creates if not exists.</para>
        /// <para> a     - Open for appending. Creates if not exists. Appends writes to end.</para>
        /// <para> r+    - Open for reading and writing. Must exist.</para>
        /// <para> w+    - Open for reading and writing. Overwrites existing file. Creates if not exists.</para>
        /// <para> a+    - Open for reading and appending. Creates if not exists. Appends writes to end.</para>
        /// <para> rb    - Open for reading in binary mode. Must exist.</para>
        /// <para> wb    - Open for writing in binary mode. Overwrites existing file. Creates if not exists.</para>
        /// <para> ab    - Open for appending in binary mode. Creates if not exists. Appends writes to end.</para>
        /// <para> rb+   - Open for reading and writing in binary mode. Must exist.</para>
        /// <para> wb+   - Open for reading and writing in binary mode. Overwrites existing file. Creates if not exists.</para>
        /// <para> ab+   - Open for reading and appending in binary mode. Creates if not exists. Appends writes to end.</para>
        /// </summary>
        [DllImport("libc.so.6", EntryPoint = "fopen")]
        public static extern IntPtr linux_fopen(string filename, string mode);

        [DllImport("libc.so.6", EntryPoint = "fread", CallingConvention = CallingConvention.Cdecl)]
        public static extern int linux_fread(void* ptr, int size, int nmemb, IntPtr file);

        [DllImport("libc.so.6", EntryPoint = "fwrite", SetLastError = true)]
        public static extern int linux_fwrite(void* buffer, int size, int number, IntPtr file);

        [DllImport("libc.so.6", EntryPoint = "fclose", SetLastError = true)]
        public static extern int linux_fclose(IntPtr file);

        /// <summary>
        /// <para> SEEK_SET = 0</para>
        /// <para> SEEK_CUR = 1</para>
        /// <para> SEEK_END = 2</para>
        /// </summary>
        [DllImport("libc.so.6", EntryPoint = "fseeko", SetLastError = true)]
        public static extern int linux_fseek(IntPtr stream, long offset, int origin);

        [DllImport("libc.so.6", EntryPoint = "ftello", SetLastError = true)]
        public static extern long linux_ftell(IntPtr stream);
    }
}
