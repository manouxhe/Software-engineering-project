using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KitBox.ViewModels;

namespace KitBox
{
    public class ViewLocator : IDataTemplate
    {

        public Control? Build(object? data)
        {
            if (data == null) return null;

            // On remplace spécifiquement "ViewModels" par "Views" 
            // et le suffixe "ViewModel" par "Page"
            var name = data.GetType().FullName!
                .Replace("ViewModels", "Views")
                .Replace("ViewModel", "Page");

            var type = Type.GetType(name);

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