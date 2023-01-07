using FluentAvalonia.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lol2gltf.ViewModels
{
    public abstract class PageViewModel : ViewModelBase
    {
        public string Name { get; protected set; }
        public string Tooltip { get; protected set; }
        public Symbol Icon { get; protected set; }

        public PageViewModel(string name, string tooltip, Symbol icon)
        {
            this.Name = name;
            this.Tooltip = tooltip;
            this.Icon = icon;
        }
    }
}
