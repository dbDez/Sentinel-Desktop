using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace SafetySentinel
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleTitleW([MarshalAs(UnmanagedType.LPWStr)] string lpConsoleTitle);

        protected override void OnStartup(StartupEventArgs e)
        {
            AllocConsole();
            SetConsoleTitleW("SENTINEL — Debug Console");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔══════════════════════════════════════════╗");
            Console.WriteLine("║  SENTINEL Debug Console                  ║");
            Console.WriteLine("╚══════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] App starting...");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Base directory: {AppDomain.CurrentDomain.BaseDirectory}");

            // Catch unhandled exceptions and log them
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[FATAL] Unhandled exception: {args.ExceptionObject}");
                Console.ResetColor();
            };

            DispatcherUnhandledException += (s, args) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] UI exception: {args.Exception.Message}");
                Console.WriteLine(args.Exception.StackTrace);
                Console.ResetColor();
                args.Handled = true;
            };

            base.OnStartup(e);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MainWindow launching...");
        }
    }
}
