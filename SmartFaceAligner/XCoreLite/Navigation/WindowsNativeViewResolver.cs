using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using XCoreLite.Contract;


/// <summary>
/// From Xamling Core: https://github.com/jakkaj/Xamling-Core/blob/40d8b83eaec5af4a28f04c83fc7c6daec4f45304/XamlingCore/XamlingCore.UWP/Navigation/WindowsNativeViewResolver.cs
/// MIT License
/// </summary>
namespace XCoreLite.Navigation
{
    public class WindowsNativeViewResolver : IWindowsNativeViewResolver
    {
        private readonly ILifetimeScope _scope;

        readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();

        public WindowsNativeViewResolver(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public Type ResolvePageType(object content)
        {
            var t = content.GetType();

            Type resolveType = null;

            if (_typeCache.ContainsKey(t))
            {
                resolveType = _typeCache[t];
            }
            else
            {
                var typeName = t.FullName.Replace("ViewModel", "View");
                typeName = _filterTypeName(typeName);
                resolveType = Type.GetType(string.Format("{0}, {1}", typeName, t.GetTypeInfo().Assembly.FullName));
            }

            var isResolved = _isRegistered(resolveType);

            if (!isResolved)
            {
                var tInfo = t.GetTypeInfo();
                //try doing one of it's base types
                var typeNameBase = t.GetTypeInfo().BaseType.FullName.Replace("ViewModel", "View");
                typeNameBase = _filterTypeName(typeNameBase);
                var baseType = Type.GetType(string.Format("{0}, {1}", typeNameBase, t.GetTypeInfo().BaseType.GetTypeInfo().Assembly.FullName));
                if (!_isRegistered(baseType))
                {
                    return null;
                }

                return baseType;
            }

            return resolveType;
        }

        public FrameworkElement Resolve(object content)
        {
            var t = content.GetType();

            Type resolveType = null;

            if (_typeCache.ContainsKey(t))
            {
                resolveType = _typeCache[t];
            }
            else
            {
                var typeName = t.FullName.Replace("ViewModel", "View");
                typeName = _filterTypeName(typeName);
                resolveType = Type.GetType(string.Format("{0}, {1}", typeName, t.GetTypeInfo().Assembly.FullName));
            }


            var view = _resolve(resolveType);

            if (view == null)
            {
                //try doing one of it's base types
                var typeNameBase = t.GetTypeInfo().BaseType.FullName.Replace("ViewModel", "View");
                typeNameBase = _filterTypeName(typeNameBase);
                var baseType = Type.GetType(string.Format("{0}, {1}", typeNameBase, t.GetTypeInfo().BaseType.GetTypeInfo().Assembly.FullName));
                view = _resolve(baseType);
            }

            if (view != null)
            {
                if (!_typeCache.ContainsKey(t))
                {
                    _typeCache.Add(t, resolveType);
                }

                view.DataContext = content;

                //if (content is XViewModel && string.IsNullOrWhiteSpace(view.Title))
                //{
                //    view.SetBinding(Page.TitleProperty, "Title");
                //}

                return view;
            }

            throw new Exception("Page could not be resolved for ViewModel: " + t.ToString());
        }


        string _filterTypeName(string typeName)
        {
            if (typeName.IndexOf("`") == -1)
            {
                return typeName;
            }
            var result = typeName.Substring(0, typeName.IndexOf("`"));
            return result;

        }

        FrameworkElement _resolve(Type t)
        {
            if (t == null)
            {
                return null;
            }
            if (_scope.IsRegistered(t))
            {
                var tView = _scope.Resolve(t) as FrameworkElement;

                return tView;
            }

            return null;
        }

        bool _isRegistered(Type t)
        {
            if (t == null)
            {
                return false;
            }
            if (_scope.IsRegistered(t))
            {
                return true;
            }

            return false;
        }

        //Xamarin.Forms.View _resolveView(Type t)
        //{
        //    if (t == null)
        //    {
        //        return null;
        //    }

        //    if (_scope.IsRegistered(t))
        //    {
        //        var tView = _scope.Resolve(t) as Xamarin.Forms.View;
        //        return tView;
        //    }

        //    return null;
        //}
    }
}
