using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.MicroCom;
using Avalonia.Win32.Interop;
using Avalonia.Win32.Win32Com;

namespace Avalonia.Win32
{
    class SystemDialogImpl : ISystemDialogImpl
    {
        private const UnmanagedMethods.FOS DefaultDialogOptions = UnmanagedMethods.FOS.FOS_FORCEFILESYSTEM | UnmanagedMethods.FOS.FOS_NOVALIDATE |
            UnmanagedMethods.FOS.FOS_NOTESTFILECREATE | UnmanagedMethods.FOS.FOS_DONTADDTORECENT;

        public unsafe Task<string[]> ShowFileDialogAsync(FileDialog dialog, Window parent)
        {
            var hWnd = parent?.PlatformImpl?.Handle?.Handle ?? IntPtr.Zero;
            return Task.Factory.StartNew(() =>
            {
                string[] result = default;

                Guid clsid = dialog is OpenFileDialog ? UnmanagedMethods.ShellIds.OpenFileDialog : UnmanagedMethods.ShellIds.SaveFileDialog;
                Guid iid = UnmanagedMethods.ShellIds.IFileDialog;

                var frm = UnmanagedMethods.CreateInstance<IFileDialog>(ref clsid, ref iid);

                var openDialog = dialog as OpenFileDialog;

                //ref uint options = null;
                //frm.GetOptions(options);
                //options |= (uint)(DefaultDialogOptions);
                //if (openDialog?.AllowMultiple == true)
                //    options |= (uint)UnmanagedMethods.FOS.FOS_ALLOWMULTISELECT;
                //frm.SetOptions(options);

                //var defaultExtension = (dialog as SaveFileDialog)?.DefaultExtension ?? "";
                //frm.SetDefaultExtension(defaultExtension);
                //frm.SetFileName(dialog.InitialFileName ?? "");
                //frm.SetTitle(dialog.Title ?? "");

                //var filters = new List<UnmanagedMethods.COMDLG_FILTERSPEC>();
                //if (dialog.Filters != null)
                //{
                //    foreach (var filter in dialog.Filters)
                //    {
                //        var extMask = string.Join(";", filter.Extensions.Select(e => "*." + e));
                //        filters.Add(new UnmanagedMethods.COMDLG_FILTERSPEC { pszName = filter.Name, pszSpec = extMask });
                //    }
                //}
                //if (filters.Count == 0)
                //    filters.Add(new UnmanagedMethods.COMDLG_FILTERSPEC { pszName = "All files", pszSpec = "*.*" });
                //var filtersArr = filters.ToArray();
                //fixed (void* fixedAr = filtersArr)
                //frm.SetFileTypes((ushort)filters.Count, filters.ToArray());
                //frm.SetFileTypeIndex(0);

                //if (dialog.Directory != null)
                //{
                //    IShellItem directoryShellItem;
                //    Guid riid = UnmanagedMethods.ShellIds.IShellItem;
                //    if (UnmanagedMethods.SHCreateItemFromParsingName(dialog.Directory, IntPtr.Zero, ref riid, out directoryShellItem) == (uint)UnmanagedMethods.HRESULT.S_OK)
                //    {
                //        frm.SetFolder(directoryShellItem);
                //        frm.SetDefaultFolder(directoryShellItem);
                //    }
                //}

                frm.Show(hWnd);
                if (openDialog?.AllowMultiple == true)
                {
                    using var fileOpenDialog = frm.QueryInterface<IFileOpenDialog>();
                    var shellItemArray = fileOpenDialog.Results;
                    var count = shellItemArray.Count;
                    result = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        var shellItem = shellItemArray.GetItemAt(i);
                        result[i] = GetAbsoluteFilePath(shellItem);
                    }
                }
                else
                {
                    var shellItem = frm.Result;
                    result = new string[] { GetAbsoluteFilePath(shellItem) };
                }

                return result;
            });
        }

        public unsafe Task<string> ShowFolderDialogAsync(OpenFolderDialog dialog, Window parent)
        {
            return Task.Factory.StartNew(() =>
            {
                string result = default;

                var hWnd = parent?.PlatformImpl?.Handle?.Handle ?? IntPtr.Zero;
                Guid clsid = UnmanagedMethods.ShellIds.OpenFileDialog;
                Guid iid = UnmanagedMethods.ShellIds.IFileDialog;

                var frm = UnmanagedMethods.CreateInstance<IFileDialog>(ref clsid, ref iid);

                var options = frm.Options; // <-- fails
                options = (uint)(UnmanagedMethods.FOS.FOS_PICKFOLDERS | DefaultDialogOptions);
                frm.SetOptions(options);
                // frm.SetTitle(dialog.Title ?? "");

                if (dialog.Directory != null)
                {
                    IShellItem directoryShellItem;
                    Guid riid = UnmanagedMethods.ShellIds.IShellItem;
                    if (UnmanagedMethods.SHCreateItemFromParsingName(dialog.Directory, IntPtr.Zero, ref riid, out directoryShellItem) == (uint)UnmanagedMethods.HRESULT.S_OK)
                    {
                        frm.SetFolder(directoryShellItem);
                    }
                }

                if (dialog.Directory != null)
                {
                    IShellItem directoryShellItem;
                    Guid riid = UnmanagedMethods.ShellIds.IShellItem;
                    if (UnmanagedMethods.SHCreateItemFromParsingName(dialog.Directory, IntPtr.Zero, ref riid, out directoryShellItem) == (uint)UnmanagedMethods.HRESULT.S_OK)
                    {
                        frm.SetDefaultFolder(directoryShellItem);
                    }
                }

                frm.Show(hWnd);
                if (frm.Result is not null)
                {
                    result = GetAbsoluteFilePath(frm.Result);
                }

                return result;
            });
        }

        private unsafe string GetAbsoluteFilePath(IShellItem shellItem)
        {
            IntPtr pszString = new IntPtr(shellItem.GetDisplayName(UnmanagedMethods.SIGDN_FILESYSPATH));
            if (pszString != IntPtr.Zero)
            {
                try
                {
                    return Marshal.PtrToStringAuto(pszString);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pszString);
                }
            }
            return default;
        }
    }
}
