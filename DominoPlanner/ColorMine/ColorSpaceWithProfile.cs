using ColorMine.ColorSpaces.Conversions;
using System;

namespace ColorMine.ColorSpaces
{
    public interface IColorSpaceWithProfile
    {
        IColorSpace Color { get; set; }

        Uri Profile { get; set; }
    }

    public class ColorSpaceWithProfile : IColorSpaceWithProfile
    {
        public IColorSpace Color { get; set; }

        public Uri Profile { get; set; }
    }

    public static class ColorSpaceExtensions
    {
        public static IColorSpaceWithProfile WithProfile(this IColorSpace colorSpace, Uri profile)
        {
            return new ColorSpaceWithProfile
                {
                    Color = colorSpace,
                    Profile = profile
                };
        }

        public static T To<T>(this IColorSpaceWithProfile color) where T : class, IColorSpace, new()
        {
            if (color.Color is ICmyk)
            {
                var rgb = CmykConverter.ToColor(color.Color as ICmyk, color.Profile);
                return rgb.To<T>();
            }
            if (typeof(ICmyk).IsAssignableFrom(typeof(T)))
            {
                var rgb = color.Color.To<Rgb>();
                var item = new Cmyk();
                CmykConverter.ToColorSpace(rgb, item, color.Profile);
                return item as T;
            }
            throw new ArgumentException("Profiles require that you are converting to or from Cmyk.");
        }
    }
}