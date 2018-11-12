using System;
using System.Collections.Generic;
using System.Text;

namespace ContextualMemoryEdgeDetection.Benchmark
{
    public interface EdgeDetectionBenchmark
    {
        List<string> getTrainingFilesPathList();

        string getTrainingFileGroundTruth(string trainingFilePath);

        List<string> getTestFilesPathList();

        string getTestFileOutputPathWithoutExtension(string testFilePath);

        string getTestingFileGroundTruth(string testingFilePath);
    }
}
