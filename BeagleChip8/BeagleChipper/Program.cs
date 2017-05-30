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

            // Set Render draw color
            Sdl.SetRenderDrawColor(mainRenderer, 0, 0, 0, 255);

            // Loop variables
            double t = 0.0;
            const double dt = 1.0 / 120.0;

            double currentTime = Sdl.GetTicks() / 1000.0;
            double accumulator = 0.0;

            SDLRect pixel = new SDLRect {H = 10, W = 10, X = 0, Y = 0};

            bool isRunning = true;
            while (isRunning)
            {
                double newTime = Sdl.GetTicks() / 1000.0;
                double frameTime = newTime - currentTime;
                currentTime = newTime;

                accumulator += frameTime;

                SdlEvent currentEvent;
                while (Sdl.PollEvent(out currentEvent) == 1)
                {
                    if (currentEvent.Type == (int)EventType.Quit)
                    {
                        isRunning = false;
                    }
                    else if (currentEvent.Type == (int)EventType.KeyDown)
                    {
                        switch (currentEvent.Key.KeySym.Sym)
                        {
                            case SDLKeyCode.A:
                                keyboard.SetKeyboardState(4, 1);
                                break;
                            case SDLKeyCode.D:
                                keyboard.SetKeyboardState(6, 1);
                                break;
                            case SDLKeyCode.W:
                                keyboard.SetKeyboardState(2, 1);
                                break;
                            case SDLKeyCode.SdlScancodeSpace:
                            case SDLKeyCode.S:
                                keyboard.SetKeyboardState(5, 1);
                                break;
                            case SDLKeyCode.Q:
                                keyboard.SetKeyboardState(1, 1);
                                break;
                            case SDLKeyCode.E:
                                keyboard.SetKeyboardState(3, 1);
                                break;
                            case SDLKeyCode.Z:
                                keyboard.SetKeyboardState(7, 1);
                                break;
                            case SDLKeyCode.X:
                                keyboard.SetKeyboardState(8, 1);
                                break;
                            case SDLKeyCode.C:
                                keyboard.SetKeyboardState(9, 1);
                                break;
                            case SDLKeyCode.R:
                                keyboard.SetKeyboardState(0xC, 1);
                                break;
                            case SDLKeyCode.F:
                                keyboard.SetKeyboardState(0xD, 1);
                                break;
                            case SDLKeyCode.V:
                                keyboard.SetKeyboardState(0xE, 1);
                                break;
                            case SDLKeyCode.Key1:
                                keyboard.SetKeyboardState(0xA, 1);
                                break;
                            case SDLKeyCode.Key2:
                                keyboard.SetKeyboardState(0x0, 1);
                                break;
                            case SDLKeyCode.Key3:
                                keyboard.SetKeyboardState(0xB, 1); ;
                                break;
                            case SDLKeyCode.Key4:
                                keyboard.SetKeyboardState(0xF, 1);
                                break;
                        }
                    }
                    else if (currentEvent.Type == (int)EventType.KeyUp)
                    {
                        switch (currentEvent.Key.KeySym.Sym)
                        {
                            case SDLKeyCode.A:
                                keyboard.SetKeyboardState(4, 0);
                                break;
                            case SDLKeyCode.D:
                                keyboard.SetKeyboardState(6, 0);
                                break;
                            case SDLKeyCode.W:
                                keyboard.SetKeyboardState(2, 0);
                                break;
                            case SDLKeyCode.SdlScancodeSpace:
                            case SDLKeyCode.S:
                                keyboard.SetKeyboardState(5, 0);
                                break;
                            case SDLKeyCode.Q:
                                keyboard.SetKeyboardState(1, 0);
                                break;
                            case SDLKeyCode.E:
                                keyboard.SetKeyboardState(3, 0);
                                break;
                            case SDLKeyCode.Z:
                                keyboard.SetKeyboardState(7, 0);
                                break;
                            case SDLKeyCode.X:
                                keyboard.SetKeyboardState(8, 0);
                                break;
                            case SDLKeyCode.C:
                                keyboard.SetKeyboardState(9, 0);
                                break;
                            case SDLKeyCode.R:
                                keyboard.SetKeyboardState(0xC, 0);
                                break;
                            case SDLKeyCode.F:
                                keyboard.SetKeyboardState(0xD, 0);
                                break;
                            case SDLKeyCode.V:
                                keyboard.SetKeyboardState(0xE, 0);
                                break;
                            case SDLKeyCode.Key1:
                                keyboard.SetKeyboardState(0xA, 0);
                                break;
                            case SDLKeyCode.Key2:
                                keyboard.SetKeyboardState(0x0, 0);
                                break;
                            case SDLKeyCode.Key3:
                                keyboard.SetKeyboardState(0xB, 0);
                                break;
                            case SDLKeyCode.Key4:
                                keyboard.SetKeyboardState(0xF, 0);
                                break;
                        }
                    }
                }

                system.ExecuteInstructionCycle();

                while (accumulator >= dt)
                {
                    // Update timers
                    if (cpu.DelayTimer > 0)
                        cpu.DelayTimer -= 1;

                    if (cpu.SoundTimer > 0)
                        cpu.SoundTimer -= 1;

                    accumulator -= dt;
                    t += dt;
                }
                
                Sdl.SetRenderDrawColor(mainRenderer, 255, 255, 255, 255);

                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        byte pixelValue = gpu.Memory[(y * 64) + x];

                        if (pixelValue == 1)
                        {
                            pixel.X = x * 10;
                            pixel.Y = y * 10;

                            Sdl.RenderDrawRect(mainRenderer, ref pixel);
                            Sdl.RenderFillRect(mainRenderer, ref pixel);
                        }
                        else
                        {
                            pixel.X = x * 10;
                            pixel.Y = y * 10;

                            Sdl.SetRenderDrawColor(mainRenderer, 0, 0, 0, 255);
                            Sdl.RenderDrawRect(mainRenderer, ref pixel);
                            Sdl.RenderFillRect(mainRenderer, ref pixel);
                            Sdl.SetRenderDrawColor(mainRenderer, 255, 255, 255, 255);
                        }
                    }
                }

                Sdl.SetRenderDrawColor(mainRenderer, 0, 0, 0, 255);

                Sdl.RenderPresent(mainRenderer);
            }
        }
    }
}