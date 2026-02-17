using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KitBox.ViewModels;

namespace KitBox
{
    public class ViewLocator : IDataTemplate
    {
        // On ajoute le '?' pour indiquer que data peut être nul
        public Control? Build(object? data)
        {
            if (data == null) return null;

            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type == null)
            {
                name = data.GetType().FullName!.Replace("ViewModel", "Page");
                type = Type.GetType(name);
            }

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}