using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DialogGeneratorServer
{
    internal class WindowHandle
    {

        // Win32 APIの定義
        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int threadId, EnumThreadDelegate callback, IntPtr param);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // EnumThreadWindowsのコールバックデリゲート
        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr param);

        public static List<IntPtr> GetProcessHandles(Process process)
        {
            var handles = new List<IntPtr>();
            var callback = (EnumThreadDelegate)((IntPtr hWnd, IntPtr param) => {
                if (IsWindowVisible(hWnd))
                {
                    GetWindowThreadProcessId(hWnd, out uint processId);

                    // ハンドルが指定したプロセスに属している場合
                    if (processId == process.Id)
                    {
                        handles.Add(hWnd);
                    }
                }
                return true;
            });
            foreach (ProcessThread thread in process.Threads)
            {
                EnumThreadWindows(thread.Id, callback, IntPtr.Zero);
            }

            return handles;
        }
    }
}
