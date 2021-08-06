using System.Collections.Generic;
using System.Linq;

namespace AwsDataAccess
{
    public class Batcher
    {
        public static List<List<T>> Batch<T>(int batchSize, List<T> classifications)
        {
            List<List<T>> classificationBatches = new List<List<T>>();

            while (classifications.Any())
            {
                classificationBatches.Add(classifications.Take(batchSize).ToList());
                classifications = classifications.Skip(batchSize).ToList();
            }

            return classificationBatches;
        }
    }
}
