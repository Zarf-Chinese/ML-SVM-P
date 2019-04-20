using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Zarf.SVMP
{
    public class Sample : MonoBehaviour
    {
        public TextMeshPro textMesh;
        public string Text { get => textMesh.text; set => textMesh.text = value; }
        [Tooltip("状态的结果，也就是预计或已知的极性")]
        public int result;
        public List<string> rawData;
        public Status status;
        public void Start()
        {
            this.transform.position = new Vector3(status[0], status[1], status[2]) * 10;
        }
    }
}