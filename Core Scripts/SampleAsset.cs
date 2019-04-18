using UnityEngine;
namespace Zarf.SVME
{
    [CreateAssetMenu(menuName = "SampleAsset", fileName = "NewSampleAsset")]
    public class SampleAsset : ScriptableObject
    {
        public GameObject samplePrefab;
        public TextAsset samplesCsvData;
        public TextAsset sampleAssetCsvData;
        [Range(0, 1)]
        [Tooltip("已知数据在所有数据的比例。越偏向1,已知数据量越大")]
        public float knownProportion = 0.5f;
    }
}