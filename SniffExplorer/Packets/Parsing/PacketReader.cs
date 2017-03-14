using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SniffExplorer.Packets.Types;

namespace SniffExplorer.Packets.Parsing
{
    public sealed class PacketReader : BinaryReader
    {
        private int _dataSize;

        private byte _bitpos = 8;
        private byte _curbitval;

        public PacketReader(Stream baseStream, int dataSize) : base(baseStream)
        {
            _dataSize = dataSize;
        }

        private void CheckValid(int size)
        {
            if (_dataSize < size)
                throw new InvalidOperationException(nameof(size));

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _dataSize -= size;
        }

        #region Readers
        public new long ReadInt64()
        {
            ResetBitReader();
            CheckValid(sizeof(long));
            return base.ReadInt64();
        }

        public new ulong ReadUInt64()
        {
            ResetBitReader();
            CheckValid(sizeof(ulong));
            return base.ReadUInt64();
        }

        public new int ReadInt32()
        {
            ResetBitReader();
            CheckValid(sizeof(int));
            return base.ReadInt32();
        }

        public new uint ReadUInt32()
        {
            ResetBitReader();
            CheckValid(sizeof(uint));
            return base.ReadUInt32();
        }

        public new short ReadInt16()
        {
            CheckValid(sizeof(short));
            return base.ReadInt16();
        }

        public new ushort ReadUInt16()
        {
            ResetBitReader();
            CheckValid(sizeof(ushort));
            return base.ReadUInt16();
        }

        public new byte ReadByte()
        {
            ResetBitReader();
            CheckValid(sizeof(byte));
            return base.ReadByte();
        }

        public new sbyte ReadSByte()
        {
            ResetBitReader();
            CheckValid(sizeof(sbyte));
            return base.ReadSByte();
        }

        public new float ReadSingle()
        {
            ResetBitReader();
            CheckValid(sizeof(float));
            return base.ReadSingle();
        }

        public new double ReadDouble()
        {
            ResetBitReader();
            CheckValid(sizeof(double));
            return base.ReadDouble();
        }

        public new bool ReadBoolean()
        {
            ResetBitReader();
            CheckValid(sizeof(byte));
            return base.ReadByte() != 0;
        }

        public void ResetBitReader()
        {
            _bitpos = 8;
        }

        public bool ReadBit()
        {
            ++_bitpos;

            if (_bitpos > 7)
            {
                _bitpos = 0;
                CheckValid(sizeof(byte));
                _curbitval = base.ReadByte();
            }

            return ((_curbitval >> (7 - _bitpos)) & 1) != 0;
        }

        public uint ReadBits(int bits)
        {
            uint value = 0;
            for (var i = bits - 1; i >= 0; --i)
                if (ReadBit())
                    value |= (uint)(1 << i);

            return value;
        }

        public new string ReadString()
        {
            var bytes = new List<byte>();

            byte b;
            while ((b = ReadByte()) != 0)  // CDataStore::GetCString calls CanRead too
                bytes.Add(b);

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public string ReadWoWString(int stringSize)
        {
            CheckValid(stringSize);
            return Encoding.UTF8.GetString(ReadBytes(stringSize));
        }

        public ulong ReadPackedUInt64()
        {
            return ReadPackedUInt64(ReadByte());
        }

        public ulong ReadPackedUInt64(byte mask)
        {
            if (mask == 0)
                return 0;

            ulong res = 0;

            var i = 0;
            while (i < 8)
            {
                if ((mask & 1 << i) != 0)
                    res += (ulong)ReadByte() << (i * 8);

                i++;
            }

            return res;
        }

        public ObjectGuid ReadObjectGuid()
        {
            var objGuid = new ObjectGuid();
            objGuid.Read(this);
            return objGuid;
        }
        #endregion
    }
}
