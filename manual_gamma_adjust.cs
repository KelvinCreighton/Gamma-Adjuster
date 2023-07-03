using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Program
{
    class Program
    {
		// Keystrokes
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(String lpModuleName);
		
        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x0100;
		private static int WM_KEYUP = 0x0101;
        private static IntPtr hook = IntPtr.Zero;
        private static LowLevelKeyboardProc llkProcedure = HookCallback;
		
		private static bool RCTRL = false;
		
		
		// Gamma
		[DllImport("gdi32.dll")]
		private static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);
		[DllImport("user32.dll")]
		static extern IntPtr GetDC(IntPtr hWnd);
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
		
		private struct RAMP
		{
			[ MarshalAs(UnmanagedType.ByValArray, SizeConst=256)]
			public UInt16[] Red;
			[ MarshalAs(UnmanagedType.ByValArray, SizeConst=256)]
			public UInt16[] Green;
			[ MarshalAs(UnmanagedType.ByValArray, SizeConst=256)]
			public UInt16[] Blue;
		}
		private static RAMP ramp = new RAMP();
		
		private static int totalGamma = 10;		// Start at default
		
		
        public static void Main(string[] args)
        {
            hook = SetHook(llkProcedure);
			SetGamma("0");						// Reset gamma before starting program
            Application.Run();
            UnhookWindowsHookEx(hook);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
			// -------
			// Keydown
			// -------
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                int vkCode = Marshal.ReadInt32(lParam);
				string key = ((Keys)vkCode).ToString();
				
				if (key == "RControlKey")
					RCTRL = true;
				
				if (key == "Up")
					SetGamma("+");
				else if (key == "Down")
					SetGamma("-");
				else if (key == "Left")
					SetGamma("0");
				else if (key == "Right")
					SetGamma("0");
            }
			
			// -----
			// Keyup
			// -----
			if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) {
                int vkCode = Marshal.ReadInt32(lParam);
				string key = ((Keys)vkCode).ToString();
				
				if (key == "RControlKey")
					RCTRL = false;
			}
			
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
		
		private static void SetGamma(string changeType)	// 3 to 44, 10 as default, 3 being the highest
		{
			if (RCTRL) {
				if (changeType == "+")
					totalGamma -= 1;			// Increase gamma
				else if (changeType == "-")
					totalGamma += 1;			// Decrease gamma
				else
					totalGamma = 10;			// Reset gamma
				
				// Min and max
				if (totalGamma > 17)
					totalGamma = 17;
				if (totalGamma < 3)
					totalGamma = 3;
				
				
				Console.WriteLine("Gamma level: " + (10-totalGamma));
				
				
				ramp.Red = new ushort[256];
				ramp.Green = new ushort[256];
				ramp.Blue = new ushort[256];
				
				for (int i = 0; i < 256; i++)
				{
					ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] =
					(ushort)(Math.Min(65535,
					Math.Max(0, Math.Pow((i+1) / 256.0, totalGamma*0.1) * 65535 + 0.5)));
				}
				SetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);
			}
		}

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModule = currentProcess.MainModule;
            String moduleName = currentModule.ModuleName;
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            return SetWindowsHookEx(WH_KEYBOARD_LL, llkProcedure, moduleHandle, 0);
		}
    }
}