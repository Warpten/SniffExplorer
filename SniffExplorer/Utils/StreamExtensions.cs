﻿using System;
using System.IO;

namespace SniffExplorer.Utils
{
    public static unsafe class StreamExtensions
    {
        public static T ReadStruct<T>(this BinaryReader br) where T : struct
        {
            if (SizeCache<T>.TypeRequiresMarshal)
            {
                throw new ArgumentException(
                    "Cannot read a generic structure type that requires marshaling support. Read the structure out manually.");
            }

            // OPTIMIZATION!
            var ret = new T();
            fixed (byte* b = br.ReadBytes(SizeCache<T>.Size))
            {
                var tPtr = (byte*)SizeCache<T>.GetUnsafePtr(ref ret);
                UnsafeNativeMethods.CopyMemory(tPtr, b, SizeCache<T>.Size);
            }
            return ret;
        }
        public static T[] ReadStruct<T>(this BinaryReader br, long count) where T : struct
        {
            return br.ReadStruct<T>((int)count);
        }

        public static T[] ReadStruct<T>(this BinaryReader br, int count) where T : struct
        {
            if (SizeCache<T>.TypeRequiresMarshal)
            {
                throw new ArgumentException(
                    "Cannot read a generic structure type that requires marshaling support. Read the structure out manually.");
            }

            if (count == 0)
                return new T[0];

            var ret = new T[count];
            fixed (byte* pB = br.ReadBytes(SizeCache<T>.Size * count))
            {
                var genericPtr = (byte*)SizeCache<T>.GetUnsafePtr(ref ret[0]);
                UnsafeNativeMethods.CopyMemory(genericPtr, pB, SizeCache<T>.Size * count);
            }
            return ret;
        }

        public static T[] ReadArray<T>(this BinaryReader br, int count) where T : struct
        {
            if (count == 0)
                return new T[0];

            if (SizeCache<T>.TypeRequiresMarshal)
                throw new ArgumentException(
                    "Cannot read a generic structure type that requires marshaling support. Read the structure out manually.");

            // NOTE: this may be safer to just call Read<T> each iteration to avoid possibilities of moved memory, etc.
            // For now, we'll see if this works.
            var ret = new T[count];
            fixed (byte* pB = br.ReadBytes(SizeCache<T>.Size * count))
            {
                var genericPtr = (byte*)SizeCache<T>.GetUnsafePtr(ref ret[0]);
                UnsafeNativeMethods.CopyMemory(genericPtr, pB, SizeCache<T>.Size * count);
            }
            return ret;
        }
    }
}