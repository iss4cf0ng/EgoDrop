using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsTerminal
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static void OpenConsole()
        {
            AllocConsole();
        }

        public static void CloseConsole()
        {
            FreeConsole();
        }
    }
}
