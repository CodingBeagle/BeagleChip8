using System;
using System.Runtime.InteropServices;

namespace BeagleSdl
{
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct SdlEvent
    {
        [FieldOffset(0)] public UInt32 Type;
        [FieldOffset(0)] public SDLKeyboardEvent Key;
        [FieldOffset(0)] public SdlQuitEvent Quit;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SdlQuitEvent
    {
        public UInt32 Type;
        public UInt32 Timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDLRect
    {
        public int X;
        public int Y;
        public int W;
        public int H;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDLKeyboardEvent
    {
        public uint Type;
        public uint Timestamp;
        public uint WindowId;
        public byte State;
        public byte Repeat;
        public SDLKeySym KeySym;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDLKeySym
    {
        public int Scancode; // Not used right now
        public SDLKeyCode Sym;
        public ushort Mod;
        public int Unusued; // Unused field
    }
    
    public enum SDLKeyCode
    {
        SdlScancodeSpace = ' ',
        A = 'a',
        B = 'b',
        C = 'c',
        D = 'd',
        W = 'w',
        S = 's',
        Q = 'q',
        E = 'e',
        Z = 'z',
        X = 'x',
        R = 'r',
        F = 'f',
        V = 'v',
        Key1 = '1',
        Key2 = '2',
        Key3 = '3',
        Key4 = '4',
    }

    public enum InitializationFlags
    {
        Timer = 0x00000001,
        Audio = 0x00000010,
        Video = 0x00000010,
        Events = 0x00004000
    }

    public enum RendererFlags
    {
        Software = 0x00000001,
        Accelerated = 0x00000002
    }

    public enum WindowFlags
    {
        OpenGl = 0x00000002
    }

    public enum EventType
    {
        Quit = 0x100,
        KeyDown = 0x300,
        KeyUp = 0x301
    }

    public static class Sdl
    {
        private const string SdlDynamicLibraryName = "SDL2.dll";

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_Init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Init(int flags);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_Quit", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Quit();

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_CreateWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateWindow(string title, int x, int y, int w, int h, int flags);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_DestroyWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyWindow(IntPtr window);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_CreateRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateRenderer(IntPtr window, int index, int flags);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_RenderClear", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderClear(IntPtr renderer);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_RenderPresent", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderPresent(IntPtr renderer);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_PollEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PollEvent(out SdlEvent theEvent);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_GetError", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetError();

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_SetRenderDrawColor", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetRenderDrawColor(IntPtr renderer, byte red, byte green, byte blue, byte alpha);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_GetTicks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTicks();

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_RenderDrawRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderDrawRect(IntPtr renderer, ref SDLRect rect);

        [DllImport(SdlDynamicLibraryName, EntryPoint = "SDL_RenderFillRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderFillRect(IntPtr renderer, ref SDLRect rect);
    }
}