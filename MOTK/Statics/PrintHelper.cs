using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Skia;
using Avalonia.Skia.Helpers;
using Avalonia.VisualTree;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MOTK.Statics
{
    public static class PrintHelper
    {
        public static void ToFile(string fileName, params IVisual[] visuals) => ToFile(fileName, visuals.AsEnumerable());
        public static void ToFile(string fileName, IEnumerable<IVisual> visuals)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("The file name cannot be null or empty.", nameof(fileName));
            }

            // Get the full path of the file
            fileName = Path.GetFullPath(fileName);

            using var doc = SKDocument.CreatePdf(fileName);
            foreach (var visual in visuals)
            {
                var bounds = visual.Bounds;
                var page = doc.BeginPage((float)bounds.Width, (float)bounds.Height);
                using var context = new DrawingContext(DrawingContextHelper.WrapSkiaCanvas(page, SkiaPlatform.DefaultDpi));
                ImmediateRenderer.Render(visual, context);
                doc.EndPage();
            }
            doc.Close();
        }

        public static IEnumerable<IVisual> GetScrollViewVisuals(ScrollViewer scrollView)
        {
            var rootVisual = scrollView.GetVisualRoot();
            var size = rootVisual.Bounds.Size;

            var bounds = scrollView.Bounds;

            // Calculate the scaling factor
            var scaleX = bounds.Width / size.Width;
            var scaleY = bounds.Height / size.Height;

            // Set the size of the root visual to match the bounds of the scroll view
            rootVisual.RenderTransform = new ScaleTransform(scaleX, scaleY);


            // Get all visuals in the visual tree, starting from the root visual
            var visuals = new List<IVisual>();
            visuals.Add(rootVisual);
            visuals.AddRange(rootVisual.GetVisualDescendants());

            return visuals;
        }
    }


}

