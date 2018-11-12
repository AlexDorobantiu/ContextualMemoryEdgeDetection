using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling
{
    static class ImageDescriptionUtil
    {
        public static readonly ISet<ColorChannelEnum> colorChannels = new HashSet<ColorChannelEnum> { ColorChannelEnum.Red, ColorChannelEnum.Green, ColorChannelEnum.Blue };

        public static readonly ISet<ColorChannelEnum> grayscaleChannel = new HashSet<ColorChannelEnum> { ColorChannelEnum.Gray };

        public static byte getPixel(ImageDescription imageDescription, byte[,] colorChannel, int positionX, int positionY)
        {
            if (positionX < 0 || positionY < 0 || positionX >= imageDescription.sizeX || positionY >= imageDescription.sizeY)
            {
                return 0;
            }
            return colorChannel[positionY, positionX];
        }

        public static int outsideMirroredPosition(int position, int maxPositions)
        {
            if (position < 0)
            {
                position = -position;
            }
            if (position >= maxPositions)
            {
                position = maxPositions + maxPositions - position - 1;
            }
            return position;
        }

        /// <summary>
        /// instead of returning a black pixel when out of bounds, it mirrores the position
        /// will not work if the position is off more than one imagesize
        /// </summary>
        public static byte getPixelMirrored(ImageDescription imageDescription, byte[,] colorChannel, int positionX, int positionY)
        {
            return colorChannel[outsideMirroredPosition(positionY, imageDescription.sizeY), outsideMirroredPosition(positionX, imageDescription.sizeX)];
        }

        public static byte getPixelMirrored(byte[,] colorChannel, int positionX, int positionY)
        {
            return colorChannel[outsideMirroredPosition(positionY, colorChannel.GetLength(0)), outsideMirroredPosition(positionX, colorChannel.GetLength(1))];
        }

        public static ImageDescription createGrayscaleImageWithSameSize(ImageDescription inputImage)
        {
            ImageDescription outputImage = new ImageDescription();
            outputImage.sizeX = inputImage.sizeX;
            outputImage.sizeY = inputImage.sizeY;
            outputImage.grayscale = true;
            outputImage.gray = new byte[inputImage.sizeY, inputImage.sizeX];
            return outputImage;
        }

        // convolution matrix needs to have an odd size
        public static ImageDescription mirroredMarginConvolution(ImageDescription inputImage, ISet<ColorChannelEnum> colorChannelsToFilter, float[,] convolutionMatrix)
        {
            ImageDescription outputImage = new ImageDescription();
            outputImage.sizeX = inputImage.sizeX;
            outputImage.sizeY = inputImage.sizeY;
            foreach (ColorChannelEnum colorChannel in Enum.GetValues(typeof(ColorChannelEnum)))
            {
                outputImage.setColorChannel(colorChannel, inputImage.getColorChannel(colorChannel));
            }

            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int filterHalfSizeX = filterSizeX / 2;
            int filterHalfSizeY = filterSizeY / 2;

            foreach (ColorChannelEnum channelEnum in colorChannelsToFilter)
            {
                byte[,] inputChannel = inputImage.getColorChannel(channelEnum);
                byte[,] outputChannel = new byte[outputImage.sizeY, outputImage.sizeX];
                outputImage.setColorChannel(channelEnum, outputChannel);

                for (int y = 0; y < inputImage.sizeY; y++)
                {
                    for (int x = 0; x < inputImage.sizeX; x++)
                    {
                        float sum = 0;
                        for (int i = -filterHalfSizeY; i <= filterHalfSizeY; i++)
                        {
                            for (int j = -filterHalfSizeX; j <= filterHalfSizeX; j++)
                            {
                                sum += convolutionMatrix[i + filterHalfSizeY, j + filterHalfSizeX] *
                                    inputChannel[outsideMirroredPosition(y + i, inputImage.sizeY), outsideMirroredPosition(x + j, inputImage.sizeX)];
                            }
                        }
                        if (sum < 0)
                        {
                            sum = 0;
                        }
                        if (sum > 255)
                        {
                            sum = 255;
                        }
                        outputChannel[y, x] = (byte)(sum + 0.5f);
                    }
                }
            }
            return outputImage;
        }

        public static ImageDescription fromBitmap(Bitmap bitmap)
        {
            ImageDescription imageDescription = new ImageDescription();
            imageDescription.sizeX = bitmap.Width;
            imageDescription.sizeY = bitmap.Height;

            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    {
                        imageDescription.alpha = new byte[imageDescription.sizeY, imageDescription.sizeX];
                        imageDescription.r = new byte[imageDescription.sizeY, imageDescription.sizeX];
                        imageDescription.g = new byte[imageDescription.sizeY, imageDescription.sizeX];
                        imageDescription.b = new byte[imageDescription.sizeY, imageDescription.sizeX];
                        for (int i = 0; i < imageDescription.sizeY; i++)
                        {
                            for (int j = 0; j < imageDescription.sizeX; j++)
                            {
                                Color pixel = bitmap.GetPixel(j, i);
                                imageDescription.alpha[i, j] = pixel.A;
                                imageDescription.r[i, j] = pixel.R;
                                imageDescription.g[i, j] = pixel.G;
                                imageDescription.b[i, j] = pixel.B;
                            }
                        }
                    }
                    break;
                case PixelFormat.Format24bppRgb:
                    {
                        imageDescription.r = new byte[imageDescription.sizeY, imageDescription.sizeX];
                        imageDescription.g = new byte[imageDescription.sizeY, imageDescription.sizeX];
                        imageDescription.b = new byte[imageDescription.sizeY, imageDescription.sizeX];
                        for (int i = 0; i < imageDescription.sizeY; i++)
                        {
                            for (int j = 0; j < imageDescription.sizeX; j++)
                            {
                                Color pixel = bitmap.GetPixel(j, i);
                                imageDescription.r[i, j] = pixel.R;
                                imageDescription.g[i, j] = pixel.G;
                                imageDescription.b[i, j] = pixel.B;
                            }
                        }
                    }
                    break;
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    {
                        if (bitmap.Palette.Flags == 2) //grayscale
                        {
                            imageDescription.grayscale = true;
                            imageDescription.gray = new byte[imageDescription.sizeY, imageDescription.sizeX];
                            for (int i = 0; i < imageDescription.sizeY; i++)
                            {
                                for (int j = 0; j < imageDescription.sizeX; j++)
                                {
                                    Color pixel = bitmap.GetPixel(j, i);
                                    imageDescription.gray[i, j] = pixel.R; // any component should be fine
                                }
                            }
                        }
                        else // palette
                        {
                            imageDescription.alpha = new byte[imageDescription.sizeY, imageDescription.sizeX];
                            imageDescription.r = new byte[imageDescription.sizeY, imageDescription.sizeX];
                            imageDescription.g = new byte[imageDescription.sizeY, imageDescription.sizeX];
                            imageDescription.b = new byte[imageDescription.sizeY, imageDescription.sizeX];
                            for (int i = 0; i < imageDescription.sizeY; i++)
                            {
                                for (int j = 0; j < imageDescription.sizeX; j++)
                                {
                                    Color pixel = bitmap.GetPixel(j, i);
                                    imageDescription.alpha[i, j] = pixel.A;
                                    imageDescription.r[i, j] = pixel.R;
                                    imageDescription.g[i, j] = pixel.G;
                                    imageDescription.b[i, j] = pixel.B;
                                }
                            }
                        }
                    }
                    break;
                default:
                    {
                        throw new Exception("Format " + bitmap.PixelFormat + " not supported!");
                    }
            }

            return imageDescription;
        }

        public static Bitmap convertToBitmap(ImageDescription imageDescription)
        {
            Bitmap bitmap = null;
            if (imageDescription.grayscale)
            {
                if (imageDescription.alpha != null)
                {
                    bitmap = new Bitmap(imageDescription.sizeX, imageDescription.sizeY, PixelFormat.Format32bppArgb);
                    for (int i = 0; i < imageDescription.sizeY; i++)
                    {
                        for (int j = 0; j < imageDescription.sizeX; j++)
                        {
                            bitmap.SetPixel(j, i, Color.FromArgb(imageDescription.alpha[i, j], imageDescription.gray[i, j], imageDescription.gray[i, j], imageDescription.gray[i, j]));
                        }
                    }
                }
                else
                {
                    bitmap = new Bitmap(imageDescription.sizeX, imageDescription.sizeY, PixelFormat.Format8bppIndexed);

                    // copy pixel values
                    BitmapData data = bitmap.LockBits(new Rectangle(0, 0, imageDescription.sizeX, imageDescription.sizeY), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                    byte[] bytes = new byte[data.Height * data.Stride];
                    for (int i = 0; i < imageDescription.sizeY; i++)
                    {
                        for (int j = 0; j < imageDescription.sizeX; j++)
                        {
                            bytes[i * data.Stride + j] = imageDescription.gray[i, j];
                        }
                    }
                    Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                    bitmap.UnlockBits(data);

                    // create grayscale palette
                    ColorPalette paletteCopy = bitmap.Palette;
                    Color[] paletteEntries = paletteCopy.Entries;
                    for (int i = 0; i < 256; i++)
                    {
                        paletteEntries[i] = Color.FromArgb((byte)i, (byte)i, (byte)i);
                    }
                    bitmap.Palette = paletteCopy;
                }
            }
            else
            {
                if (imageDescription.alpha != null)
                {
                    bitmap = new Bitmap(imageDescription.sizeX, imageDescription.sizeY, PixelFormat.Format32bppArgb);
                    for (int i = 0; i < imageDescription.sizeY; i++)
                    {
                        for (int j = 0; j < imageDescription.sizeX; j++)
                        {
                            bitmap.SetPixel(j, i, Color.FromArgb(imageDescription.alpha[i, j], imageDescription.r[i, j], imageDescription.g[i, j], imageDescription.b[i, j]));
                        }
                    }
                }
                else
                {
                    bitmap = new Bitmap(imageDescription.sizeX, imageDescription.sizeY, PixelFormat.Format24bppRgb);
                    for (int i = 0; i < imageDescription.sizeY; i++)
                    {
                        for (int j = 0; j < imageDescription.sizeX; j++)
                        {
                            bitmap.SetPixel(j, i, Color.FromArgb(imageDescription.r[i, j], imageDescription.g[i, j], imageDescription.b[i, j]));
                        }
                    }
                }
            }

            return bitmap;
        }

        public static Bitmap resizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static void makeAllImagesSameSize(List<ImageDescription> inputImages, out int newSizeX, out int newSizeY, out List<ImageDescription> outputImages)
        {
            newSizeX = 0;
            newSizeY = 0;
            foreach (ImageDescription image in inputImages)
            {
                if (image.sizeX > newSizeX)
                {
                    newSizeX = image.sizeX;
                }
                if (image.sizeY > newSizeY)
                {
                    newSizeY = image.sizeY;
                }
            }

            outputImages = new List<ImageDescription>(inputImages.Count);
            ResizeFilter rf = new ResizeFilter(newSizeX, newSizeY, new HashSet<ColorChannelEnum> { ColorChannelEnum.Gray });
            foreach (ImageDescription image in inputImages)
            {
                if (image.sizeX != newSizeX || image.sizeY != newSizeY)
                {
                    outputImages.Add(rf.filter(image));
                }
                else
                {
                    outputImages.Add(image);
                }
            }
        }

        public static float[,] mirroredMarginConvolution(byte[,] colorChannel, float[,] convolutionMatrix)
        {
            int colorChannelSizeX = colorChannel.GetLength(1);
            int colorChannelSizeY = colorChannel.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int filterMinX = filterSizeX / 2;
            int filterMinY = filterSizeY / 2;
            int filterMaxX = filterSizeX / 2 + filterSizeX % 2;
            int filterMaxY = filterSizeY / 2 + filterSizeY % 2;

            float[,] output = new float[colorChannelSizeY, colorChannelSizeX];

            for (int y = 0; y < colorChannelSizeY; y++)
            {
                for (int x = 0; x < colorChannelSizeX; x++)
                {
                    float sum = 0;
                    for (int i = -filterMinY; i < filterMaxY; i++)
                    {
                        for (int j = -filterMinX; j < filterMaxX; j++)
                        {
                            sum += convolutionMatrix[i + filterMinY, j + filterMinX] *
                                colorChannel[outsideMirroredPosition(y + i, colorChannelSizeY), outsideMirroredPosition(x + j, colorChannelSizeX)];
                        }
                    }
                    output[y, x] = sum;
                }
            }
            return output;
        }

        public static float[,] mirroredMarginConvolution(float[,] inputMatrix, float[,] convolutionMatrix)
        {
            int sizeX = inputMatrix.GetLength(1);
            int sizeY = inputMatrix.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int filterMinX = filterSizeX / 2;
            int filterMinY = filterSizeY / 2;
            int filterMaxX = filterSizeX / 2 + filterSizeX % 2;
            int filterMaxY = filterSizeY / 2 + filterSizeY % 2;

            float[,] output = new float[sizeY, sizeX];

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    float sum = 0;
                    for (int i = -filterMinY; i < filterMaxY; i++)
                    {
                        for (int j = -filterMinX; j < filterMaxX; j++)
                        {
                            sum += convolutionMatrix[i + filterMinY, j + filterMinX] *
                                inputMatrix[outsideMirroredPosition(y + i, sizeY), outsideMirroredPosition(x + j, sizeX)];
                        }
                    }
                    output[y, x] = sum;
                }
            }
            return output;
        }
    }
}
