using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm
{
    static class EdgeDetectionAlgorithmUtil
    {
        public static EdgeDetectionAlgorithm loadAlgorithmFromCompressedFile(string inputModelFilename)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (GZipStream zippedStream = new GZipStream(new FileStream(inputModelFilename, FileMode.Open), CompressionMode.Decompress, false))
            {
                EdgeDetectionAlgorithm algorithm = (EdgeDetectionAlgorithm)bf.Deserialize(zippedStream);
                return algorithm;
            }
        }

        public static void saveToCompressedFile(EdgeDetectionAlgorithm algorithm, string filename)
        {
            using (GZipStream zippedStream = new GZipStream(new FileStream(filename, FileMode.Create), CompressionMode.Compress, false))
            {
                algorithm.save(zippedStream);
            }
        }
    }
}
