using System;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UnityAdsTest
{
    public class LogText : MonoBehaviour
    {
        [SerializeField]
        Text _text;

        LogQueue _logQueue;
        StringBuilder _stringBuilder;

        void Reset()
        {
            _text = GetComponent<Text>();
        }

        void Start()
        {
            _logQueue = new LogQueue(100).AddTo(this);
            _stringBuilder = new StringBuilder();
        }

        void Update()
        {
            if (!_logQueue.IsDirty) return;
            _logQueue.IsDirty = false;

            _stringBuilder.Clear();
            foreach (var line in _logQueue.GetLines(null, LogQueue.LogLevel.All))
            {
                _stringBuilder.AppendLine(line.Text);
            }

            _text.text = _stringBuilder.ToString();
        }
    }
}