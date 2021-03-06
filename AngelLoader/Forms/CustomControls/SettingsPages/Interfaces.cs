﻿using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    internal static class Interfaces
    {
        [PublicAPI]
        internal interface ISettingsPage
        {
            bool IsVisible { get; }
            void SetVScrollPos(int value);
            int GetVScrollPos();
            void ShowPage();
            void HidePage();
        }
    }
}
