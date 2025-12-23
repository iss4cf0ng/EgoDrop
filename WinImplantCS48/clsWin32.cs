using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;

namespace WinImplantCS48
{
    public class clsWin32
    {
        #region Struct

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        #endregion

        public class clsKernel32
        {
            public const int EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
            public const int PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x00020016;

            [DllImport("kernel32.dll")]
            public static extern int CreatePseudoConsole(
                COORD size,
                IntPtr hInput,
                IntPtr hOutput,
                uint flags,
                out IntPtr phPC
            );

            [DllImport("kernel32.dll")]
            public static extern int ResizePseudoConsole(
                IntPtr hPC,
                COORD size
            );

            [DllImport("kernel32.dll")]
            public static extern void ClosePseudoConsole(IntPtr hPC);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CreatePipe(
                out SafeFileHandle hReadPipe,
                out SafeFileHandle hWritePipe,
                IntPtr lpPipeAttributes,
                int nSize
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadFile(
                SafeFileHandle hFile,
                byte[] lpBuffer,
                int nNumberOfBytesToRead,
                out int lpNumberOfBytesRead,
                IntPtr lpOverlapped
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteFile(
                SafeFileHandle hFile,
                byte[] lpBuffer,
                int nNumberOfBytesToWrite,
                out int lpNumberOfBytesWritten,
                IntPtr lpOverlapped
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool InitializeProcThreadAttributeList(
                IntPtr lpAttributeList,
                int dwAttributeCount,
                int dwFlags,
                ref IntPtr lpSize
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool UpdateProcThreadAttribute(
                IntPtr lpAttributeList,
                int dwFlags,
                IntPtr attribute,
                IntPtr lpValue,
                IntPtr cbSize,
                IntPtr lpPreviousValue,
                IntPtr lpReturnSize
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern void DeleteProcThreadAttributeList(
                IntPtr lpAttributeList
            );

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool CreateProcessW(
                string lpApplicationName,
                string lpCommandLine,
                IntPtr lpProcessAttributes,
                IntPtr lpThreadAttributes,
                bool bInheritHandles,
                int dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                ref STARTUPINFOEX lpStartupInfo,
                out PROCESS_INFORMATION lpProcessInformation
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr hObject);
        }
    }
}
