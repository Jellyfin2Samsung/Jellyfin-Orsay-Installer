using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Jellyfin.Orsay.Installer.ViewModels;

namespace Jellyfin.Orsay.Installer
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var vmName = param.GetType().FullName!;

            // Try convention: ViewModels.Pages.FooPageViewModel -> Views.Pages.FooPage
            var viewName = vmName
                .Replace(".ViewModels.", ".Views.", StringComparison.Ordinal)
                .Replace("ViewModel", "", StringComparison.Ordinal);

            var type = Type.GetType(viewName);

            // Fallback: ViewModels.FooViewModel -> Views.FooView
            if (type == null)
            {
                viewName = vmName.Replace("ViewModel", "View", StringComparison.Ordinal);
                type = Type.GetType(viewName);
            }

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + vmName };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
