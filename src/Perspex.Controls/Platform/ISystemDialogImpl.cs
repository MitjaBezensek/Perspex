﻿// Copyright (c) The Perspex Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System.Threading.Tasks;
using Perspex.Platform;

namespace Perspex.Controls.Platform
{
    /// <summary>
    /// Defines a platform-specific system dialog implementation.
    /// </summary>
    public interface ISystemDialogImpl
    {
        /// <summary>
        /// Shows a file dialog.
        /// </summary>
        /// <param name="dialog">The details of the file dialog to show.</param>
        /// <param name="parent">The parent window.</param>
        /// <returns>A task returning the selected filenames.</returns>
        Task<string[]> ShowFileDialogAsync(FileDialog dialog, IWindowImpl parent);
    }
}
