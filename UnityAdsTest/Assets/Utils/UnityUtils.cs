using UnityEngine;

namespace Utils
{
    public static class UnityUtils
    {
        public static bool IsNullOrDestroyed(this Object self)
        {
            return self == null | !self;
        }

        public static T OrNull<T>(this T self) where T : Object
        {
            return self.IsNullOrDestroyed() ? null : self;
        }

        public static void Destroy(this Object self)
        {
            Object.Destroy(self);
        }
    }
}