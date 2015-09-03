using System;
using System.Collections.Generic;
using System.Text;

namespace Bitcoin.Sha26
{
    public enum EndianType
    {
        /// <summary>The Least Significant Byte is first.</summary>
        LittleEndian,

        /// <summary>The Most Significant Byte is first.</summary>
        BigEndian
    };
}
