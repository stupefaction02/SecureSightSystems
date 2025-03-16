using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureSightSystems.Core.Extensions
{
    public static class ByteArrayExtensions
    {
        public static int FindSubArray(this byte[] array, byte[] needle, int startIndex, int sourceLength)
        {
            int needleLen = needle.Length;
            int index;

            while (sourceLength >= needleLen)
            {
                index = Array.IndexOf(array, needle[0], startIndex, sourceLength - needleLen + 1);

                if (index == -1)
                    return -1;

                int i, p;
                for (i = 0, p = index; i < needleLen; i++, p++)
                {
                    if (array[p] != needle[i])
                    {
                        break;
                    }
                }

                if (i == needleLen)
                {
                    return index;
                }

                sourceLength -= (index - startIndex + 1);
                startIndex = index + 1;
            }
            return -1;
        }
    }
}
