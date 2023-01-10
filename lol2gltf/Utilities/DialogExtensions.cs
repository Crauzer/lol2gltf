using CommunityToolkit.Diagnostics;
using HanumanInstitute.MvvmDialogs;
using LeagueToolkit.Core.Mesh;
using System.ComponentModel;
using System.Threading.Tasks;

namespace lol2gltf.Utilities
{
    public static class DialogExtensions
    {
        public static async Task<(string path, SkinnedMesh skinnedMesh)> ShowLoadSkinnedMeshViewAsync(
            this IDialogService dialogService,
            INotifyPropertyChanged ownerViewModel
        )
        {
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(ownerViewModel, nameof(ownerViewModel));

            string result = await dialogService.ShowOpenFileDialogAsync(
                ownerViewModel,
                new()
                {
                    AllowMultiple = false,
                    Title = "aaa",
                    Filters = new() { new("Simple Skin files (.skn)", new[] { "skn" }) }
                }
            );

            return result switch
            {
                null => (null, null),
                _ => (result!, SkinnedMesh.ReadFromSimpleSkin(result!))
            };
        }
    }
}
