// NULL_TODO
#nullable disable

// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.

using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    /// <summary>
    /// Indicates the icon to use for a task dialog.
    /// </summary>
    [PublicAPI]
    public enum TaskDialogIcon
    {
        /// <summary>
        /// A custom icon or no icon if no custom icon is specified.
        /// </summary>
        Custom,
        /// <summary>
        /// System warning icon.
        /// </summary>
        Warning = 0xFFFF, // MAKEINTRESOURCEW(-1)
        /// <summary>
        /// System Error icon.
        /// </summary>
        Error = 0xFFFE, // MAKEINTRESOURCEW(-2)
        /// <summary>
        /// System Information icon.
        /// </summary>
        Information = 0xFFFD, // MAKEINTRESOURCEW(-3)
        /// <summary>
        /// Shield icon.
        /// </summary>
        Shield = 0xFFFC // MAKEINTRESOURCEW(-4)
    }
}
