using Avalonia.Controls;
using Avalonia.Controls.Templates;
using JankiBusiness.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace JankiAvalonia.Services
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class ViewForPageAttribute : Attribute
    {
        public Type VMType { get; private set; }

        public ViewForPageAttribute(Type VMType) => this.VMType = VMType;
    }

    public class PageViewLocator : IDataTemplate
    {
        private Dictionary<Type, Type>? views;

        public IControl Build(object param)
        {
            if (views == null)
                BuildViewTypeCache();

            IControl control = (IControl)Activator.CreateInstance(views[param.GetType()])!;
            return control;
        }

        [MemberNotNull(nameof(views))]
        private void BuildViewTypeCache()
        {
            views = new Dictionary<Type, Type>();

            foreach (var item in typeof(PageViewLocator).Assembly.DefinedTypes)
            {
                ViewForPageAttribute? attr = item.GetCustomAttribute<ViewForPageAttribute>();
                if (attr != null)
                    views.Add(attr.VMType, item);
            }
        }

        public bool Match(object data) => data is PageViewModel;
    }
}