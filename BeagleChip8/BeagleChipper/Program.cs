using System;
using System.IO;
using BeagleChipper.Models;
using BeagleChipper.Models.SystemComponents;
using BeagleSdl;
using System = BeagleChipper.Models.System;

namespace BeagleChipper
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Initialize SDL
            Sdl.Init((int) InitializationFlags.Video);

            // Create window
            IntPtr mainWindow = Sdl.CreateWindow("BeagleChipper!", 200, 200, 640, 320, (int) WindowFlags.OpenGl);
            if (mainWindow == IntPtr.Zero)
            {
                Console.WriteLine("Failed to initialize main window! :(");
                return;
            }

            // Create renderer
            IntPtr mainRenderer = Sdl.CreateRenderer(mainWindow, -1, (int) RendererFlags.Accelerated);
            if (mainRenderer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to initialize renderer! :(");
                return;
            }

            Keyboard keyboard = new Keyboard();
            Ram ram = new Ram();
            Cpu cpu = new Cpu();
            Gpu gpu = new Gpu();

            Models.System system = new Models.System(
                mainRenderer,
                cpu,
                gpu,
                ram,
                keyboard
            );

            system.LoadProgram("INVADERS.ch8");

            system.Run();
        }
    }
}