using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.FileHandling
{
    class ImageFileHandler
    {
        public static ImageDescription loadFromPath(string filePath)
        {
            Bitmap bitmap = new Bitmap(filePath);
            return ImageDescriptionUtil.fromBitmap(bitmap);
        }

        public static void saveToPath(ImageDescription imageDescription, string filePath, string fileExtension)
        {
            Image imageToSave = ImageDescriptionUtil.convertToBitmap(imageDescription);
            string fullFileName = Path.ChangeExtension(filePath, fileExtension);
            switch (fileExtension)
            {
                case ".png": imageToSave.Save(fullFileName, ImageFormat.Png); break;
                case ".jpg": imageToSave.Save(fullFileName, ImageFormat.Jpeg); break;
                case ".bmp": imageToSave.Save(fullFileName, ImageFormat.Bmp); break;
                case ".gif": imageToSave.Save(fullFileName, ImageFormat.Gif); break;
                case ".ico": imageToSave.Save(fullFileName, ImageFormat.Icon); break;
                case ".emf": imageToSave.Save(fullFileName, ImageFormat.Emf); break;
                case ".exif": imageToSave.Save(fullFileName, ImageFormat.Exif); break;
                case ".tiff": imageToSave.Save(fullFileName, ImageFormat.Tiff); break;
                case ".wmf": imageToSave.Save(fullFileName, ImageFormat.Wmf); break;
                default: imageToSave.Save(fullFileName, ImageFormat.Png); break;
            }
        }

    }
}
