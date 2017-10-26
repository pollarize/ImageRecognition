using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PixelClusteringProgram
{
    class PixelClusterEngine
    {
        private Bitmap image;
        private Color[] centroids;
        private Dictionary<Color, int> histogram;
        private double[,] distanceMatrix;
        private bool[,] groupMatrix;
        private bool[,] groupMatrixPrevoius;
        private int itteractionLimiter;
        private double percentageLoss;

        public PixelClusterEngine(String img, int numberOfClusters)
        {
            Console.WriteLine("Get Image...");
            image = new Bitmap(img);


            Console.WriteLine("Start extracting RGB histogram...");
            ExtractColourHistogram();
            Console.WriteLine("Founded colors in current image: {0}",histogram.Count);

            Console.WriteLine("Initializing clusters...");
            Console.WriteLine("Try to set {0}clusters", numberOfClusters);
            InitCentroid(numberOfClusters);
            groupMatrix = new bool[centroids.Length, histogram.Count];
            groupMatrixPrevoius = new bool[centroids.Length, histogram.Count];
            distanceMatrix = new double[centroids.Length, histogram.Count];
            itteractionLimiter = 0;
            percentageLoss = 0;
        }

        public PixelClusterEngine(String img, int numberOfClusters, int maxCountOfIteractions)
        {
            image = new Bitmap(img);
            ExtractColourHistogram();
            InitCentroid(numberOfClusters);
            groupMatrix = new bool[centroids.Length, histogram.Count];
            groupMatrixPrevoius = new bool[centroids.Length, histogram.Count];
            distanceMatrix = new double[centroids.Length, histogram.Count];
            itteractionLimiter = maxCountOfIteractions;
        }

        public void CalculateClusterCenters()
        {
            bool isCalculationReady = true;
            int countOfItteractions = 0;

            Console.WriteLine("Invoking K-means clustering");
            long startTime = DateTime.Now.Millisecond;

            while (isCalculationReady)
            {
                //First time the Centroids are staticly inittialized
                if (0 < countOfItteractions)
                {
                    DetermineCentroids();
                }

                //Distance calcultion
                CalculateDistances();
                //Clustering the data by distance
                ClusteringJudgement();

                //Check is grouping from previous calculation is same as the current one
                isCalculationReady = !isReadyToStopCalculating();

                //Limit by itteractions
                if (itteractionLimiter != 0 && itteractionLimiter == countOfItteractions)
                {
                    isCalculationReady = false;
                }
                else
                {
                    countOfItteractions++;
                }
            }

            Console.WriteLine("K-means clustering finished: {0} itteractions - {1}ms",countOfItteractions, DateTime.Now.Millisecond - startTime);
        }

        public Color[] getCentroids()
        {
            return this.centroids;
        }

        public Color getCentroidByIndex(int index)
        {
            return centroids[index];
        }

        public List<int> getListOfIndexColoursInCluster(int clusterIndex)
        {
            List<int> list = new List<int>();
            for(int i = 0; i <histogram.Count ;i++)
            {
                bool data = groupMatrix[clusterIndex, i];

                if (false != data)
                {
                    list.Add(i);
                }
            }
            return list;
        }

        public Dictionary<Color,Color> getListOfColoursInCluster(int clusterIndex)
        {
            Dictionary<Color,Color> dict = new Dictionary<Color,Color>();
            List<int> list = getListOfIndexColoursInCluster(clusterIndex);

            for (int i = 0; i < list.Count; i++)
            {
                if (!dict.ContainsKey(histogram.ToList()[list[i]].Key))
                {
                    dict.Add(histogram.ToList()[list[i]].Key, centroids[clusterIndex]);
                }
            }

            return dict;
        }

        public Dictionary<Color, int> getListOfColoursByClusters()
        {
            Dictionary<Color, int> dict = new Dictionary<Color, int>();
            List<int> list = new List<int>();
            var listHistogram = histogram.ToList();

            for (int j = 0; j < centroids.Length; j++)
            {
                list = getListOfIndexColoursInCluster(j);
                for (int i = 0; i < list.Count; i++)
                {
                    if (!dict.ContainsKey(histogram.ToList()[list[i]].Key))
                    { 
                        dict.Add(listHistogram[list[i]].Key, j);
                    }
                }
            }
            return dict;
        }

        public Dictionary<Color, int> getListOfColoursByClustersSpeedUp()
        {
            Dictionary<Color, int> dict = histogram;
            List<int> list = new List<int>();
            var listHistogram = histogram.ToList();

            for (int j = 0; j < centroids.Length; j++)
            {
                list = getListOfIndexColoursInCluster(j);
                for (int i = 0; i < list.Count; i++)
                {
                    dict[listHistogram[list[i]].Key] = j;
                }
            }
            return dict;
        }

        public Bitmap getSegmentationImage()
        {
            Bitmap newImage = image;
            Dictionary<Color, int> dictionaryOfCluster = getListOfColoursByClustersSpeedUp();
            Console.WriteLine("Start segmentation of Image...");
            long startTime = DateTime.Now.Millisecond;
            for (int i = 0; i < newImage.Width; i++)
            {
                for (int j = 0; j < newImage.Height; j++)
                {
                    Color pixel = newImage.GetPixel(i, j);
                    newImage.SetPixel(i, j, centroids[dictionaryOfCluster[pixel]]);
                }
            }

            percentageLoss = (double)100 - ((double )100 / ((double)histogram.Count / (double)centroids.Length));
            Console.WriteLine("Finished segmentation of Image: {0}ms", DateTime.Now.Millisecond - startTime);
            Console.WriteLine("Percentage Loss: {0}%",percentageLoss);
            return newImage;
        } 

        public void CreateSegmentationImage(String pathToSave)
        {
            getSegmentationImage().Save(pathToSave);
            Console.WriteLine("Saving Image...\n\n");
        }

        private double calculateDistanceObjects(Color centroid, Color obejct)
        {
            double distance = 0;
            double sqrtR = Math.Pow((obejct.R - centroid.R),2);
            double sqrtG = Math.Pow((obejct.G - centroid.G), 2);
            double sqrtB = Math.Pow((obejct.B - centroid.B), 2);
            distance = Math.Sqrt(sqrtR + sqrtG + sqrtB);
            return distance;
        }

        private void ExtractColourHistogram()
        {
            histogram = new Dictionary<Color, int>();
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color currentColor = image.GetPixel(i, j);
                    if (!histogram.ContainsKey(currentColor))
                    {
                        histogram.Add(currentColor, 1);
                    }
                    else
                    {
                        histogram[currentColor] += 1;
                    }
                }
            }
        }

        private void InitCentroid(int numberOfClusters)
        {

            //Init centroids
            if (histogram.Count >= numberOfClusters)
            {
                centroids = new Color[numberOfClusters];
            }
            else
            {
                Console.WriteLine("The number of clusters was reduced to : {0} clusters", histogram.Count);
                centroids = new Color[histogram.Count];
            }

            //Arrange by count of equal pixels
            var sortedDict = from entry in histogram orderby entry.Value descending select entry;

            //Set dictionary as list for index accessing
            var list = sortedDict.ToList();

            //Extract colour centroids
            for (int i = 0; i < numberOfClusters; i++)
            {
                centroids[i] = (list[i].Key);
            }
        }

        private void CalculateDistances()
        {
            int row = 0;
            int col = 0;

            foreach (var centroid in centroids)
            {
                col = 0;
                foreach (var pixel in histogram)
                {
                    distanceMatrix[row, col] = calculateDistanceObjects(centroid, pixel.Key);
                    col++;
                }
                row++;
            }
        }

        private void ClusteringJudgement()
        {
            int row = 0;
            int col = 0;
            double[] columnExtract = new double[centroids.Length];
            

            foreach (var pixel in histogram)
            {
                int minIndex = 0;
                row = 0;
                foreach (var centroid in centroids)
                {
                    columnExtract[row] = distanceMatrix[row, col];
                    row++;
                }

                //Get nearest distance
                minIndex = Array.IndexOf(columnExtract, columnExtract.Min());

                //Assign true to nearest 
                groupMatrix[minIndex, col] = true;

                //Increment column
                col++;
            }
        }

        private void DetermineCentroids()
        {
            int numberOfPixelsInCluster = histogram.Count / centroids.Length;

            List<int> clusterMembers = new List<int>();

            for (int i = 0; i < centroids.Length;i++)
            {
                //Pre-Init on Every centroid
                clusterMembers = new List<int>();

                //Extract 
                for (int j = 0; j < histogram.Count; j++)
                {
                    if (false != groupMatrix[i, j])
                    {
                        clusterMembers.Add(j);
                    }
                }

                if (clusterMembers.Count > 1)
                {
                    centroids[i] = calculateNewCentroid(clusterMembers);
                }

            }
        }

        private Color calculateNewCentroid(List<int> members)
        {
            Color newCentroid;
            var colours = histogram.ToList();
            int R = 0;
            int G = 0;
            int B = 0;

            for (int i = 0; i < members.Count;i++)
            {
                R += colours[members[i]].Key.R;
                G += colours[members[i]].Key.G;
                B += colours[members[i]].Key.B;
            }

            R = R / members.Count;
            G = G / members.Count;
            B = B / members.Count;

            newCentroid = Color.FromArgb(255, R, G, B);

            return newCentroid;
        }

        private bool isReadyToStopCalculating()
        {
            bool isReady = true;

            for(int i = 0; i < centroids.Length;i++)
            {
                for (int j = 0; j < histogram.Count; j++)
                {
                    if (groupMatrix [i,j] != groupMatrixPrevoius[i, j])
                    {
                        isReady = false;
                        break;
                    }
                }
            }

            if (!isReady)
            {
                Array.Copy(groupMatrix, groupMatrixPrevoius, groupMatrix.Length);
            }

            return isReady;
        }
    }

}
