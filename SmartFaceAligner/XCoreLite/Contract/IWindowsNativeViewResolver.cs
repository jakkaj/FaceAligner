using System;
using System.Windows;
using System.Windows.Controls;

namespace XCoreLite.Contract
{
    public interface IWindowsNativeViewResolver
    {
        Type ResolvePageType(object content);
        FrameworkElement Resolve(object content);
    }
}