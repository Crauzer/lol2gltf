using lol2gltf.UI.Pages;
using System;
using System.Collections.Generic;

namespace lol2gltf.UI.MVVM
{
    public class CommandPages : List<PageItem>
    {
        public CommandPages()
        {
            Add(new PageItem(typeof(ConvertModelToGltfPage), "Convert Model to glTF"));
        }
    }

    public class PageItem
    {
        public string Name { get; }
        public Uri PageUri { get; }

        public PageItem(Type pageType, string name)
        {
            this.Name = name;
            this.PageUri = new Uri($"Pages/{pageType.Name}.xaml", UriKind.Relative);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
