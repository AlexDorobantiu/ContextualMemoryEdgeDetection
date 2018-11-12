using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ContextualMemoryEdgeDetection.Benchmark
{
    class BerkeleyEdgeDetectionBenchmark : EdgeDetectionBenchmark
    {
        string basePath;
        List<string> trainingFilesPaths = new List<string>();
        List<string> testFilesPaths = new List<string>();
        List<string> validationFilesPaths = new List<string>();

        Dictionary<string, string> trainingFilesPathToGroundTruth = new Dictionary<string, string>();
        Dictionary<string, string> testFilesPathToOutput = new Dictionary<string, string>();
        Dictionary<string, string> testFilesPathToGroundTruth = new Dictionary<string, string>();
        int? trainingSetSizeLimit;

        const string groundTruthFileExtension = ".png";
        const string groundTruthSuffix = "_gt";
        private const string trainingFolderName = "train";
        private const string testFolderName = "test";
        private const string testGroundTruthFolderName = "test_gt";
        private const string testGroundTruthFileExtension = ".png";
        private const string dataTrainingFolderName = "data";
        private static string[] augmentedTrainingFolderNames = { "aug_data" };
        private static string[] augmentedScaledTrainingFolderNames = { "aug_data_scale_0.5", "aug_data_scale_1.5" };

        public BerkeleyEdgeDetectionBenchmark(string basePath, string outputFolder, bool useAugmentedSet = false, bool useScaledSet = false, int? trainingSetSizeLimit = null)
        {
            this.basePath = basePath;
            this.trainingSetSizeLimit = trainingSetSizeLimit;
            if (Directory.Exists(basePath))
            {
                string testFolder = Path.Combine(basePath, testFolderName);
                string testGroundTruthFolder = Path.Combine(basePath, testGroundTruthFolderName);
                testFilesPaths.AddRange(Directory.GetFiles(testFolder));
                foreach (string testFile in testFilesPaths)
                {
                    string outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(testFile));
                    testFilesPathToOutput.Add(testFile, outputFile);
                    string outputFileGroundTruth = Path.Combine(testGroundTruthFolder, Path.ChangeExtension(Path.GetFileNameWithoutExtension(testFile), testGroundTruthFileExtension));
                    testFilesPathToGroundTruth.Add(testFile, outputFileGroundTruth);
                }

                string trainingFolder = Path.Combine(basePath, trainingFolderName);
                
                string dataFolder = Path.Combine(trainingFolder, dataTrainingFolderName);
                string groundTruthFolder = Path.Combine(trainingFolder, dataTrainingFolderName + groundTruthSuffix);
                if (Directory.Exists(dataFolder) && Directory.Exists(groundTruthFolder))
                {
                    string[] files = Directory.GetFiles(dataFolder);
                    foreach (string filePath in files)
                    {
                        string groundTruthFile = Path.Combine(groundTruthFolder, Path.GetFileNameWithoutExtension(filePath) + groundTruthFileExtension);
                        trainingFilesPathToGroundTruth.Add(filePath, groundTruthFile);
                    }
                    trainingFilesPaths.AddRange(files);
                }
                if (useAugmentedSet)
                {
                    List<string> augmentedFolders = new List<string>(augmentedTrainingFolderNames);
                    if (useScaledSet)
                    {
                        augmentedFolders.AddRange(augmentedScaledTrainingFolderNames);
                    }
                    foreach (string augmentedDataFolder in augmentedFolders)
                    {
                        string augmentedTrainingFolder = Path.Combine(trainingFolder, augmentedDataFolder);
                        string augmentedTrainingFolderGroundTruth = Path.Combine(trainingFolder, augmentedDataFolder + groundTruthSuffix);
                        if (!Directory.Exists(augmentedTrainingFolder))
                        {
                            continue;
                        }
                        string[] directories = Directory.GetDirectories(augmentedTrainingFolder);
                        foreach (string directoryName in directories)
                        {
                            string simpleDirectoryName = directoryName.Substring(directoryName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                            string augmentedTrainingSubFolderGroundTruth = Path.Combine(augmentedTrainingFolderGroundTruth, simpleDirectoryName);
                            string[] files = Directory.GetFiles(directoryName);
                            foreach (string filePath in files)
                            {
                                string groundTruthFile = Path.Combine(augmentedTrainingSubFolderGroundTruth, Path.GetFileNameWithoutExtension(filePath) + groundTruthFileExtension);
                                trainingFilesPathToGroundTruth.Add(filePath, groundTruthFile);
                            }
                            trainingFilesPaths.AddRange(files);
                        }
                    }
                }
            }
            else
            {
                throw new Exception(basePath + " should point to an existing directory.");
            }

            if (trainingSetSizeLimit != null)
            {
                trainingFilesPaths.RemoveRange(trainingSetSizeLimit.Value, trainingFilesPaths.Count - trainingSetSizeLimit.Value);
            }
        }

        public string getTestFileOutputPathWithoutExtension(string testFilePath)
        {
            return testFilesPathToOutput[testFilePath];
        }

        public List<string> getTestFilesPathList()
        {
            return testFilesPaths;
        }

        public string getTrainingFileGroundTruth(string trainingFilePath)
        {
            return trainingFilesPathToGroundTruth[trainingFilePath];
        }

        public List<string> getTrainingFilesPathList()
        {
            return trainingFilesPaths;
        }

        public string getTestingFileGroundTruth(string testingFilePath)
        {
            return testFilesPathToGroundTruth[testingFilePath];
        }
    }
}
