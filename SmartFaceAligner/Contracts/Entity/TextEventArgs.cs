using System;

namespace Contracts.Entity
{
    public class TextEventArgs : EventArgs
    {
        public TextEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
