namespace SocialNetwork.Common
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    public class ImageUtility
    {
        public static string Resize(string imageDataUrl, int width, int height)
        {
            return Resize(imageDataUrl, width, height, ImageFormat.Jpeg);
        }

        private static string Resize(string imageDataUrl, int width, int height, ImageFormat format)
        {
            var oldImage = ToImage(imageDataUrl);

            var resizedImage = new Bitmap(oldImage, new Size(width, height));

            using (var memoryStream = new MemoryStream())
            {
                resizedImage.Save(memoryStream, format);
                byte[] imageBytes = memoryStream.ToArray();

                string base64String = Convert.ToBase64String(imageBytes);

                return base64String;
            }
        }

        private static Image ToImage(string imageDataUrl)
        {
            var bytes = Convert.FromBase64String(imageDataUrl);

            Image image;
            using (var memoryStream = new MemoryStream(bytes))
            {
                image = Image.FromStream(memoryStream);
            }

            return image;
        }
    }
}
