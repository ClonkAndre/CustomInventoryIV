using System;
using System.Collections.Generic;

namespace CustomInventoryIV
{
    internal class Helper
    {

        public static T[] ResizeArray<T>(T[] original, int newCapacity, out List<T> leftBehindItems) where T : class
        {
            T[] newArray = new T[newCapacity];

            if (newCapacity < original.Length)
            {
                // Calculate how many items there will be left behind
                int leftBehindItemsCount = original.Length - newCapacity;

                // Get the items that are gonna be left behind and return them to the sender
                if (leftBehindItemsCount > 0)
                {
                    leftBehindItems = new List<T>(leftBehindItemsCount);

                    for (int i = 0; i < leftBehindItemsCount; i++)
                    {
                        T item = original[newCapacity + i];

                        if (item == null)
                            continue;

                        leftBehindItems.Add(item);
                    }
                }
                else
                {
                    leftBehindItems = null;
                }

                Array.Copy(original, newArray, newCapacity);
            }
            else
            {
                Array.Copy(original, newArray, original.Length);
                leftBehindItems = null;
            }

            return newArray;
        }

    }
}
