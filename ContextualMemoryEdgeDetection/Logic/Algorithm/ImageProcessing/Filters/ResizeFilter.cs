using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    [Serializable]
    class ResizeFilter : ImageFilter
    {
        int newSizeX;

        int newSizeY;

        private ISet<ColorChannelEnum> colorChannelsToFilter;

        public ResizeFilter(int newSizeX, int newSizeY, ISet<ColorChannelEnum> colorChannelsToFilter)
        {
            this.newSizeX = newSizeX;
            this.newSizeY = newSizeY;
            this.colorChannelsToFilter = colorChannelsToFilter;
        }

        public ImageDescription filter(ImageDescription inputImage)
        {
            ImageDescription outputImage = new ImageDescription();
            outputImage.sizeX = newSizeX;
            outputImage.sizeY = newSizeY;

            foreach (ColorChannelEnum colorChannel in colorChannelsToFilter)
            {
                byte[,] channel = inputImage.getColorChannel(colorChannel);
                ImageDescription temp = new ImageDescription();
                temp.sizeX = inputImage.sizeX;
                temp.sizeY = inputImage.sizeY;
                temp.grayscale = true;
                temp.setColorChannel(ColorChannelEnum.Gray, channel);

                Bitmap tempBitmap = ImageDescriptionUtil.convertToBitmap(temp);
                Bitmap output = ImageDescriptionUtil.resizeImage(tempBitmap, newSizeX, newSizeY);
                temp = ImageDescriptionUtil.fromBitmap(output);
                temp.computeGrayscale();
                outputImage.setColorChannel(colorChannel, temp.gray);
            }

            if (colorChannelsToFilter.Count == 1 && colorChannelsToFilter.Contains(ColorChannelEnum.Gray))
            {
                outputImage.grayscale = true;
            }
            return outputImage;
        }

    }
}
