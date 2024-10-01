using System;

namespace CustomInventoryIV
{
    internal class Helper
    {

        public static T[] ResizeArray<T>(T[] original, int newSize)
        {
            T[] newArray = new T[newSize];

            if (newSize < original.Length)
                Array.Copy(original, newArray, newSize);
            else
                Array.Copy(original, newArray, original.Length);

            return newArray;
        }

    }
}
