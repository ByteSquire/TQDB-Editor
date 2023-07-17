using Avalonia.Controls;
using Prism.Events;
using System;
using System.Runtime.InteropServices;
using TQDBEditor.Events;

namespace TQDBEditor.Windows.WinModules.Services
{
    public class WinTaskbarProgressHandler
    {
        private static ITaskbarList3? _taskbarList;
        private static readonly object _taskbarLock = new();
        internal static ITaskbarList3 TaskbarList
        {
            get
            {
                if (_taskbarList == null)
                {
                    lock (_taskbarLock)
                    {
                        if (_taskbarList == null)
                        {
                            _taskbarList = (ITaskbarList3)new CTaskbarList();
                            _taskbarList.HrInit();
                        }
                    }
                }
                return _taskbarList;
            }
        }

        private readonly nint _windowHandle;

        /// <summary>
        /// Determines if the application is running on Windows 7
        /// </summary>
        public static bool RunningOnWin7 =>
            // Verifies that OS version is 6.1 or greater, and the Platform is WinNT.
            Environment.OSVersion.Platform == PlatformID.Win32NT &&
            Environment.OSVersion.Version.CompareTo(new(6, 1)) >= 0;

        /// <summary>
        /// Throws PlatformNotSupportedException if the application is not running on Windows 7
        /// </summary>
        public static void ThrowIfNotWin7()
        {
            if (!RunningOnWin7)
            {
                throw new PlatformNotSupportedException("Not running on Win7 or greater!");
            }
        }

        public WinTaskbarProgressHandler(TopLevel mainWindow, IEventAggregator ea)
        {
            ThrowIfNotWin7();
            _windowHandle = mainWindow.TryGetPlatformHandle()!.Handle;
            ea.GetEvent<MainProgressEvent>().Subscribe(UpdateProgress);
        }

        public void UpdateProgress(MainProgressEventPayload payload)
        {
            try
            {
                TaskbarList.SetProgressValue(_windowHandle, payload.CurrentProgress, payload.MaxProgress);
                if (Enum.TryParse(payload.State?.ToString(), out ThumbnailProgressState progressState))
                    TaskbarList.SetProgressState(_windowHandle, progressState);
            }
            catch (InvalidOperationException)
            {
            }
        }

        #region Windows Taskbar COM
        /// <summary>
        /// Represents the thumbnail progress bar state.
        /// </summary>
        public enum ThumbnailProgressState
        {
            /// <summary>
            /// No progress is displayed.
            /// </summary>
            NoProgress = 0,
            /// <summary>
            /// The progress is indeterminate (marquee).
            /// </summary>
            Indeterminate = 0x1,
            /// <summary>
            /// Normal progress is displayed.
            /// </summary>
            Normal = 0x2,
            /// <summary>
            /// An error occurred (red).
            /// </summary>
            Error = 0x4,
            /// <summary>
            /// The operation is paused (yellow).
            /// </summary>
            Paused = 0x8
        }

        [ComImport()]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig]
            void HrInit();
            [PreserveSig]
            void AddTab(IntPtr hwnd);
            [PreserveSig]
            void DeleteTab(IntPtr hwnd);
            [PreserveSig]
            void ActivateTab(IntPtr hwnd);
            [PreserveSig]
            void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig]
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
            void SetProgressState(IntPtr hwnd, ThumbnailProgressState tbpFlags);
        }

        [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
        [ClassInterface(ClassInterfaceType.None)]
        [ComImport()]
        internal class CTaskbarList { }
        #endregion
    }
}
