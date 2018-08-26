/*-----------------------------------------------------------------------
 GCNet - A Grand Chase Packet Library
 Copyright © 2018 Gabriel F. (Frihet Dev)

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program. If not, see <http://www.gnu.org/licenses/>.
-----------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GCNet
{
    public sealed class BinaryDeserializer : IDeserializer
    {
        public int Position { get; set; } = 0;

        private byte[] _data;

        public BinaryDeserializer(byte[] data)
        {
            _data = data;
        }

        public byte[] ReadRawBytes(int count)
        {
            return GetByteRange(count).ToArray();
        }
        
        public T Read<T>()
        {
            dynamic value;

            if (typeof(T) == typeof(byte))
            {
                value = _data[Position++];
            }
            else if (typeof(T) == typeof(char))
            {
                value = (char)_data[Position++];
            }
            else if (typeof(T) == typeof(bool))
            {
                value = _data[Position++] != 0;
            }
            else if (typeof(T) == typeof(short))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(short));
                value = BitConverter.ToInt16(readBytes, 0);
            }
            else if (typeof(T) == typeof(ushort))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(ushort));
                value = BitConverter.ToUInt16(readBytes, 0);
            }
            else if (typeof(T) == typeof(int))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(int));
                value = BitConverter.ToInt32(readBytes, 0);
            }
            else if (typeof(T) == typeof(uint))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(uint));
                value = BitConverter.ToUInt32(readBytes, 0);
            }
            else if (typeof(T) == typeof(long))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(long));
                value = BitConverter.ToInt64(readBytes, 0);
            }
            else if (typeof(T) == typeof(ulong))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(ulong));
                value = BitConverter.ToUInt64(readBytes, 0);
            }
            else if (typeof(T) == typeof(float))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(float));
                value = BitConverter.ToSingle(readBytes, 0);
            }
            else if (typeof(T) == typeof(double))
            {
                byte[] readBytes = ReadBytesAsBigEndian(sizeof(double));
                value = BitConverter.ToDouble(readBytes, 0);
            }
            else if (typeof(T) == typeof(string))
            {
                int stringSize = Read<int>();
                value = Encoding.Unicode.GetString(_data, Position, stringSize);
                Position += stringSize;
            }
            else if (typeof(T) == typeof(char[]))
            {
                value = ReadArray<char>();
            }
            else if (typeof(IDeserializable).IsAssignableFrom(typeof(T)))
            {
                T obj = (T)FormatterServices.GetUninitializedObject(typeof(T));
                ((IDeserializable)obj).Deserialize(this);

                value = obj;
            }
            else
            {
                throw new NotSupportedException(
                    "Unsupported type. It isn't a primitive type nor provides the IDeserializable interface.");
            }
            return value;
        }

        public T[] ReadArray<T>()
        {
            int arrayLength = Read<int>();
            var array = new T[arrayLength];

            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = Read<T>();
            }
            return array;
        }

        public List<T> ReadVector<T>()
        {
            int vectorLength = Read<int>();
            var vector = new List<T>(vectorLength);

            for (int i = 0; i < vectorLength; i++)
            {
                vector[i] = Read<T>();
            }
            return vector;
        }

        public LinkedList<T> ReadList<T>()
        {
            int listLength = Read<int>();
            var list = new LinkedList<T>();

            for (int i = 0; i < listLength; i++)
            {
                list.AddLast(Read<T>());
            }
            return list;
        }

        public SortedSet<T> ReadSet<T>()
        {
            int setLength = Read<int>();
            var set = new SortedSet<T>();

            for (int i = 0; i < setLength; i++)
            {
                set.Add(Read<T>());
            }
            return set;
        }

        public Dictionary<TKey, TValue> ReadMap<TKey, TValue>()
        {
            int mapLength = Read<int>();
            var map = new Dictionary<TKey, TValue>(mapLength);

            for (int i = 0; i < mapLength; i++)
            {
                TKey key = Read<TKey>();
                TValue value = Read<TValue>();
                map.Add(key, value);
            }
            return map;
        }

        public Tuple<T1, T2> ReadPair<T1, T2>()
        {
            T1 firstValue = Read<T1>();
            T2 secondValue = Read<T2>();

            return new Tuple<T1, T2>(firstValue, secondValue);
        }

        private IEnumerable<byte> GetByteRange(int count)
        {            
            IEnumerable<byte> byteRange = _data.Skip(Position).Take(count);
            Position += count;

            return byteRange;
        }

        private byte[] ReadBytesAsBigEndian(int count)
        {
            return GetByteRange(count).Reverse().ToArray();
        }
    }
}