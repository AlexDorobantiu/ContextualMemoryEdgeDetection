using System;
using System.Collections.Generic;
using System.Text;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling
{
    public class ImageDescription
    {
        public int sizeX { get; set; }

        public int sizeY { get; set; }

        public bool grayscale { get; set; }

        public byte[,] alpha { get; set; }   // alpha value

        public byte[,] r { get; set; }       // red component

        public byte[,] g { get; set; }       // green component

        public byte[,] b { get; set; }       // blue component

        public byte[,] gray { get; set; }    // gray component

        private Dictionary<ColorChannelEnum, byte[,]> computedChannels = new Dictionary<ColorChannelEnum, byte[,]>();

        public void computeGrayscale()
        {
            if (grayscale)
            {
                return;
            }
            if (gray == null)
            {
                gray = new byte[sizeY, sizeX];
                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        gray[i, j] = (byte)((r[i, j] + g[i, j] + b[i, j] + 1) / 3);
                    }
                }
            }
        }

        public void computeGrayscaleAsLuminance()
        {
            if (grayscale)
            {
                return;
            }
            if (gray == null)
            {
                gray = new byte[sizeY, sizeX];
            }
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    gray[i, j] = (byte)Math.Round(((float)r[i, j] * 0.3f + (float)g[i, j] * 0.59f + (float)b[i, j] * 0.11f) / 3.0f);
                }
            }
        }

        public byte[,] getColorChannel(ColorChannelEnum colorChannel)
        {
            switch (colorChannel)
            {
                case ColorChannelEnum.Red:
                    return r;
                case ColorChannelEnum.Green:
                    return g;
                case ColorChannelEnum.Blue:
                    return b;
                case ColorChannelEnum.Gray:
                    return gray;
                default:
                    if (computedChannels.ContainsKey(colorChannel))
                    {
                        return computedChannels[colorChannel];
                    }
                    return null;
            }
        }

        public void setColorChannel(ColorChannelEnum colorChannel, byte[,] channel)
        {
            switch (colorChannel)
            {
                case ColorChannelEnum.Red:
                    r = channel;
                    break;
                case ColorChannelEnum.Green:
                    g = channel;
                    break;
                case ColorChannelEnum.Blue:
                    b = channel;
                    break;
                case ColorChannelEnum.Gray:
                    gray = channel;
                    break;
                default:
                    computedChannels[colorChannel] = channel;
                    break;
            }
        }
    }
}
