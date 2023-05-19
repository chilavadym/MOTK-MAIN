using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using System.Reflection;

namespace MOTK.Statics;

internal static class AssetsLocator
{
    public static Bitmap? GetAsset(string? assetName)
    {
        var dir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;

        var assetsDir = $"{dir}\\Assets";

        if (assetName == null) return null;

        var imageName = Path.Combine(assetsDir, assetName);

        return new Bitmap(imageName);
    }

    public static Bitmap? GetImageAsset(string imageName)
    {
        var path = "/Assets/Branding/Default/Images/";
        var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        var image = new Bitmap(assetLoader?.Open(new Uri("avares://" + assemblyName + path + imageName + ".png")));
        return image;
    }
}