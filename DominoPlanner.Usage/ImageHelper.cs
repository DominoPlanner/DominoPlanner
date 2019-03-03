using System.IO;
using System.Linq;

namespace DominoPlanner.Usage
{
    internal static class ImageHelper
    {
        internal static string GetImageOfFile(string projectpath)
        {
            string picturepath = Path.Combine(Path.GetDirectoryName(projectpath), "..", "Source Image");
            if (!Directory.Exists(picturepath))
            {
                return @"./Icons/image.ico";
            }
            picturepath = Directory.GetFiles(picturepath).Where(x => Path.GetFileNameWithoutExtension(x).Equals(Path.GetFileNameWithoutExtension(projectpath))).FirstOrDefault();
            if (picturepath == null || !File.Exists(picturepath))
            {
                picturepath = @"./Icons/image.ico";
            }
            return picturepath;
        }
    }
}
