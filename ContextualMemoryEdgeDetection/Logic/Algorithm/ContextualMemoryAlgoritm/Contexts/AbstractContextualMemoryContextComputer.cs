using System;
using System.Collections.Generic;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm.Contexts
{
    [Serializable]
    public abstract class AbstractContextualMemoryContextComputer
    {
        public abstract void computeIndexes(ImageDescription inputImage, int positionX, int positionY, int computedIndex, int[,] computedIndexes);

        public abstract List<int> getTableBitSizes();
    }
}