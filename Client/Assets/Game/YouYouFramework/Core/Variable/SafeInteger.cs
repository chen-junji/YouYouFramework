using UnityEngine;
using System.Collections;

namespace YouYou
{

    public struct SafeInteger
    {
        int m_iValue;
        const int Mask = 9981;

        public static implicit operator int(SafeInteger si)
        {
            int v = si.m_iValue ^ Mask;
            return (int)((uint)v << 16 | (uint)v >> 16);
        }

        /// <summary>
        /// 真实值 在Lua中用
        /// </summary>
        public int RealValue
        {
            get
            {
                int v = m_iValue ^ Mask;
                return (int)((uint)v << 16 | (uint)v >> 16);
            }
        }

        public static implicit operator SafeInteger(int n)
        {
            SafeInteger si;
            n = (int)((uint)n << 16 | (uint)n >> 16);
            si.m_iValue = n ^ Mask;
            return si;
        }

        public static SafeInteger operator ++(SafeInteger si)
        {
            si += 1;
            return si;
        }

        public static SafeInteger operator --(SafeInteger si)
        {
            si -= 1;
            return si;
        }

        public override string ToString()
        {
            int v = (int)this;
            return v.ToString();
        }
    }
}