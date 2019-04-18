using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zarf.SVME
{
    public class StatusEnumerator : IEnumerator<float>
    {
        public StatusEnumerator(Status status) { this.status = status; }
        Status status;
        int index = -1;
        public object Current => status[index];

        float IEnumerator<float>.Current
        {
            get
            {
                return status[index];
            }
        }

        public bool MoveNext()
        {
            index++;
            return status.dimension > index;
        }

        public void Reset()
        {
            index = -1;
        }

        public void Dispose()
        {
        }
    }
    [Serializable]
    public class Status : ICloneable, IEquatable<Status>, IEnumerable<float>
    {
        public static Status RandomUnit(int dimension)
        {
            var status = new Status(dimension);
            for (var i = 0; i < dimension; i++)
            {
                status[i] = UnityEngine.Random.Range(0f, 100f);
            }
            return status.normalized;
        }
        public Status() : this(3) { }
        public Status(int dimension)
        {
            this.dimension = dimension > 1 ? dimension : 1;
            this.values = new List<float>(3);
        }
        /// <summary>
        /// 通过index来获取某个维度的值
        /// </summary>
        /// <value></value>
        public float this[int index]
        {
            get
            {
                if (index < this.values.Count)
                {
                    return this.values[index];
                }
                else
                {
                    if (index >= this.dimension)
                    {
                        Debug.LogWarning("尝试引用一个超过了维度范围的数据！");
                    }
                    //超过范围，返回0
                    return 0;
                }
            }
            set
            {
                if (index >= this.dimension)
                {
                    Debug.LogWarning("尝试改变一个超过了维度范围的数据！");
                }
                else
                {
                    while (values.Count <= index)
                    {
                        //补全数据
                        values.Add(0);
                    }
                    //设值
                    values[index] = value;
                }
            }
        }
        public int dimension;
        public List<float> values;
        /// <summary>
        /// 该多维向量的体积
        /// </summary>
        /// <value></value>
        public float volume
        {
            get
            {
                var m = 0f;
                foreach (var v in values)
                {
                    m += v * v;
                }
                return m;
            }
        }
        /// <summary>
        /// 该向量的模值
        /// </summary>
        /// <value></value>
        public float magnitude
        {
            get => (float)Math.Sqrt(this.volume);
        }
        public Status normalized
        {
            get
            {
                var norm = (Status)this.Clone();
                norm /= this.magnitude;
                return norm;
            }
        }

        public object Clone()
        {
            var clone = new Status(this.dimension);
            clone.values.AddRange(this.values);
            return clone;
        }

        public bool Equals(Status other)
        {
            if (this.dimension == other.dimension)
            {
                //维度相等
                for (var i = 0; i < this.dimension; i++)
                {
                    //每个维度值都相等
                    if (other[i] != this[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return this.Equals((Status)obj);
        }
        IEnumerator<float> IEnumerable<float>.GetEnumerator()
        {
            return new StatusEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new StatusEnumerator(this);
        }

        public static Status operator +(Status a, Status b)
        {
            if (a.dimension != b.dimension)
            {
                Debug.LogError("不同维度的状态之间不可计算！");
            }
            var ret = (Status)a.Clone();
            for (var i = 0; i < ret.dimension; i++)
            {
                ret[i] += b[i];
            }
            return ret;
        }
        public static Status operator -(Status a)
        {
            var ret = new Status(a.dimension);
            for (var i = 0; i < ret.dimension; i++)
            {
                ret[i] = -ret[i];
            }
            return ret;
        }
        public static Status operator -(Status a, Status b)
        {

            if (a.dimension != b.dimension)
            {
                Debug.LogError("不同维度的状态之间不可计算！");
            }
            var ret = (Status)a.Clone();
            for (var i = 0; i < ret.dimension; i++)
            {
                ret[i] -= b[i];
            }
            return ret;
        }
        public static Status operator *(Status a, float d)
        {
            var ret = (Status)a.Clone();
            for (var i = 0; i < ret.dimension; i++)
            {
                ret[i] *= d;
            }
            return ret;
        }
        public static Status operator *(float d, Status a)
        {
            return a * d;
        }
        public static Status operator /(Status a, float d)
        {
            if (d == 0)
            {
                Debug.LogError("被0除！");
                //被0除，不做任何事
                return a;
            }
            else
            {
                return a * (1 / d);
            }
        }
        public static bool operator ==(Status lhs, Status rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(Status lhs, Status rhs)
        {
            return !lhs.Equals(rhs);
        }

    }
}