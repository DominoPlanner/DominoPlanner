using DominoPlanner.Core;
using System.IO;
using System.Linq;

namespace DominoPlanner.Usage
{
    internal static class ImageHelper
    {
        internal static string GetImageOfFile(string projectpath)
        {
            var filters = Workspace.LoadImageFilters<IDominoProvider>(projectpath);
            string picturepath = null;
            if (filters != null)
            {
                var filefilters = filters.Where(f => f is BlendFileFilter).ToList();
                if (filefilters.Count != 0)
                {
                    picturepath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectpath), ((BlendFileFilter)filefilters[0]).FilePath));
                }
            }
            // Path.Combine(Path.GetDirectoryName(projectpath), "..", "Source Image");
            /*if (picturepath == "" || !File.Exists(picturepath))
            {
                return @"./Icons/image.ico";
            }*/
            //picturepath = Directory.GetFiles(picturepath).Where(x => Path.GetFileNameWithoutExtension(x).Equals(Path.GetFileNameWithoutExtension(projectpath))).FirstOrDefault();
            if (picturepath == null || !File.Exists(picturepath))
            {
                picturepath = @"./Icons/image.ico";
            }
            return picturepath;
        }
    }
}
