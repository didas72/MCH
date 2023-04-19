using System;

using Raylib_cs;

using MCH.Render;

namespace MCH.Windows
{
    internal abstract class BaseWindow
    {
        #region Palette
        public readonly static Color Background = new(51, 51, 51, 255), Mid = new(77, 77, 77, 255), Foreground = new(102, 102, 102, 255), Text = new(200, 200, 200, 255), Highlights = new(0, 206, 255, 255);
        #endregion


        public Holder Window { get; protected set; }
    }
}
