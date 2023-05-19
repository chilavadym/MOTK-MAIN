using Avalonia.Media.Imaging;
using MOTK.Statics;

namespace MOTK.Models;

public class Cable
{
    public Cable(string text, string imageName)
    {
        Text = text;
        Image = AssetsLocator.GetImageAsset(imageName);
    }

    public string Text { get; set; }
    public Bitmap? Image { get; set; }
}