using System;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public static class LangUtils
    {
        public static T ThrowIfNull<T>(this T self) where T : class
        {
            if (self == null)
            {
                throw new NullReferenceException();
            }

            return self;
        }

        public static string Shorten(this string self, int maxCount, string elipsis = "...")
        {
            if (self.Length <= maxCount) return self;

            var omitCharLength = self.Length - maxCount;
            var leftLength = self.Length / 2 - omitCharLength / 2;
            var leftSelf = self.Substring(0, leftLength);
            var rightIndex = self.Length - leftLength;
            var rightSelf = self.Substring(rightIndex, self.Length - rightIndex);
            return $"{leftSelf}{elipsis}{rightSelf}";
        }

        public static bool CheckNotNull(this string self, out string output)
        {
            if (string.IsNullOrEmpty(self))
            {
                output = "";
                return false;
            }

            output = self;
            return true;
        }

        public static string TakeString(this string text, int maxCount)
        {
            return text.Substring(0, Mathf.Min(maxCount, text.Length));
        }

        public static string TakeFirstLines(this string self, int count)
        {
            return string.Join("\n", self.Split('\n').Take(count));
        }
    }
}