using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zarf.SVME
{
    public class Experiment : MonoBehaviour
    {
        public enum Option
        {
            [Tooltip("是否将测试数据的结果作为一个新的已知样本")]
            UseTestAsSample,
        }
        public List<Option> options;
        public Transform sampleRoot;
        public List<List<Sample>> samples = new List<List<Sample>>();
        public SampleAsset sampleAsset;
        public List<string> paramTypes;
        [Tooltip("(从数据中得到的)数据参数维度，相当于参数种类的数量")]
        public int paramDimension;
        public List<string> genderTypes;
        [Tooltip("(从数据中得到的)极性维度，相当于极性种类的数量")]
        public int genderAmount;
        // Start is called before the first frame update
        void Start()
        {
            this.StartCoroutine(Work());
        }
        IEnumerator Work()
        {
            var sampleAssetData = ReadCSV(this.sampleAsset.sampleAssetCsvData.text);
            genderTypes = sampleAssetData[0];
            paramTypes = sampleAssetData[1];
            genderAmount = genderTypes.Count;
            paramDimension = paramTypes.Count;
            while (this.samples.Count > this.genderAmount)
            {
                //如果gender种类过多……
                this.samples.RemoveAt(this.samples.Count - 1);
            }
            foreach (var genderSamples in samples)
            {
                //清空现有sample
                genderSamples.Clear();
            }
            while (this.samples.Count < this.genderAmount)
            {
                //如果gender种类过少……
                this.samples.Add(new List<Sample>());
            }
            yield return null;
            //获取问题的数据
            var sampleDatas = ReadCSV(this.sampleAsset.samplesCsvData.text);
            int knownSampleAmount = (int)(this.sampleAsset.knownProportion * sampleDatas.Count);
            Debug.LogFormat("开始收集数据！总数据量为{0}", knownSampleAmount);
            int i = 0;
            //收集数据
            for (; i < knownSampleAmount; i++)
            {
                var sampleData = sampleDatas[i];
                //创建sample
                var sample = this.AddSample(sampleData);
                yield return null;
            }
            Debug.LogFormat("开始测试数据！总测试量为{0}", sampleDatas.Count - knownSampleAmount);
            int successAmount = 0;
            //将剩余的数据作为测试数据
            for (; i < sampleDatas.Count; i++)
            {
                var testData = sampleDatas[i];
                var predictGender = this.Predict(testData);
                if (Test(predictGender, testData))
                {
                    //测试成功
                    successAmount++;
                }
                if (options.Contains(Option.UseTestAsSample))
                {
                    //将测试数据的结果作为一个新的已知样本
                    //创建sample
                    var sample = this.AddSample(testData, predictGender);
                }
                else
                {
                    //如果不将测试数据作为样本，则只创建该样本，在unity中显示它，
                    //而不将其纳入已知范围。
                    this.CreateSample(testData);
                }
                yield return null;
            }
            Debug.LogFormat("完成测试数据！总测试成功量达{0}个，成功率为 {1}%", successAmount, successAmount * 100f / (sampleDatas.Count - knownSampleAmount));
        }
        public Status CreateStatus(List<string> sampleData)
        {
            var sampleStatus = new Status(this.paramDimension);
            int i = 0;
            for (; i < paramDimension; i++)
            {
                sampleStatus[i] = float.Parse(sampleData[i]);
            }
            return sampleStatus;
        }
        /// <summary>
        /// 分析该预计极性与数据的真实极性是否相等
        /// </summary>
        /// <param name="predict"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        public bool Test(int predict, List<string> testData)
        {
            return predict == this.genderTypes.IndexOf(testData[testData.Count - 1]);
        }
        /// <summary>
        /// 分辨一个sample，推测它的极性
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public int Predict(List<string> testData)
        {
            var testStatus = CreateStatus(testData);
            int predictedGender = 0;
            float maxEpe = 0;
            for (var i = 0; i < this.genderAmount; i++)
            {
                var epe = this.CalculateElecticPotentialEnergy(testStatus, i);
                //记录总电势能最大的gender
                if (epe > maxEpe)
                {
                    maxEpe = epe;
                    predictedGender = i;
                }
            }
            return predictedGender;
        }
        /// <summary>
        /// 计算某个状态点对于某个极性的电势能。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        public float CalculateElecticPotentialEnergy(Status target, int gender)
        {
            //获得所有改极性的电子的总电势能
            var genderStatuses = samples[gender].ConvertAll((sample) => sample.status);
            var totalEPE = 0f;
            /**
            电场力公式：                    f= kQq/(r^2)
            电势绝对值公式：              e= |kq/r|
            假设每一个实验值的电荷量绝对值都为1
            不考虑常数的电势能绝对值公式：  e=1/r
             */
            //计算总电势能
            foreach (var genderStatus in genderStatuses)
            {
                var delta = genderStatus - target;
                var r = delta.magnitude;//得到距离
                if (r == 0)
                {
                    return float.MaxValue;
                }
                totalEPE += 1 / r;//将genderStatus在当前target位置的电势能叠加到总电势能中
            }
            return totalEPE;
        }
        /// <summary>
        /// 添入一个sample。
        /// </summary>
        /// <param name="sampleData"></param>
        /// <param name="predict">可选，对该sample的预计极性。不设值则会传入sample的真实极性</param>
        /// <returns></returns>
        public Sample AddSample(List<string> sampleData, int? predict = null)
        {
            var newSample = CreateSample(sampleData);
            //将已知的gender传给result
            newSample.result = predict ?? this.genderTypes.IndexOf(sampleData[sampleData.Count - 1]);
            if (predict.HasValue)
            {
                newSample.Text += "\nPredicted gender:    " + this.genderTypes[predict.Value];
            }
            this.samples[newSample.result].Add(newSample);
            return newSample;
        }
        public Sample CreateSample(List<string> sampleData)
        {
            var newSample = Instantiate(sampleAsset.samplePrefab, this.sampleRoot).GetComponent<Sample>();
            newSample.status = CreateStatus(sampleData);
            newSample.rawData = sampleData;
            newSample.Text = "Actual gender:   " + sampleData[sampleData.Count - 1];
            return newSample;
        }
        public List<List<string>> ReadCSV(string csvData)
        {
            var ret = new List<List<string>>();
            foreach (var line in csvData.Replace("\r\n", "\n").Split('\n'))
            {
                line.Trim();
                if (line.Length > 0)
                {
                    var cells = new List<string>();
                    foreach (var cell in line.Split(','))
                    {
                        cell.Trim();
                        if (cell.Length > 0)
                        {
                            cells.Add(cell);
                        }
                    }
                    if (cells.Count > 0)
                    {
                        ret.Add(cells);
                    }
                }
            }
            return ret;
        }
    }
}
