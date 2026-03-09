using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace eqshopping.Services
{
    public class ImageHelper
    {
        public static (long OriginalSize, long ResizedSize) ResizeImage(Stream inputStream, string outputPath, int maxWidth, int maxHeight)
        {
            long originalSize = inputStream.Length;

            // Load the image from the stream
            using (var image = Image.Load(inputStream))
            {
                // Resize the image
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max
                }));

                // Save the resized image
                image.Save(outputPath, new JpegEncoder());
            }

            // Get the resized file size
            long resizedSize = new FileInfo(outputPath).Length;

            return (originalSize, resizedSize);
        }
    }
}
