using System;
using System.Reflection;
using System.Windows.Markup;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using lol2gltf.ViewModels;

namespace lol2gltf
{
    /// <summary>
    /// Maps view models to views.
    /// </summary>
    public class ViewLocator : ViewLocatorBase
    {
        /// <inheritdoc />
        protected override string GetViewName(object viewModel) =>
            viewModel.GetType().FullName!.Replace("ViewModel", "View");
    }
}
