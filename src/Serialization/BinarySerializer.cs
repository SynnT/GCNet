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
using System.IO;
using System.Linq;
using System.Text;

namespace GCNet
{
    public sealed class BinarySerializer : IDisposable, ISerializer
    {
        private MemoryStream _dataStream;
        private BinaryWriter _writer;

        public BinarySerializer() : this(new MemoryStream()) { }

        public BinarySerializer(byte[] buffer) : this(new MemoryStream(buffer)) { }

        private BinarySerializer(MemoryStream dataStream)
        {
            _dataStream = dataStream;
            _writer = new BinaryWriter(_dataStream, Encoding.Unicode);
        }

        public void WriteRawBytes(byte[] bytes, int startIndex, int count)
        {
            _writer.Write(bytes, startIndex, count);
        }

        public void Write<T>(T value)
        {
            Type type = typeof(T);

            if (type == typeof(byte))
            { 
                _writer.Write(Convert.ToByte(value));
            }
            else if (type == typeof(char))
            {
                _writer.Write(Convert.ToByte(value));
            }
            else if (type == typeof(bool))
            {
                _writer.Write(Convert.ToBoolean(value));
            }
            else if (type == typeof(short))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToInt16(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(ushort))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToUInt16(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(int))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(uint))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToUInt32(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(long))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToInt64(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(ulong))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToUInt64(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(float))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToSingle(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(double))
            {
                byte[] bytes = BitConverter.GetBytes(Convert.ToDouble(value)).Reverse().ToArray();
                _writer.Write(bytes);
            }
            else if (type == typeof(string))
            {
                string str = Convert.ToString(value);
                _writer.Write(str.Length);
                _writer.Write(str);
            }
            else if (value is Array)
            {
                dynamic arr = value;
                WriteArray(arr);
            }
            else if (value is ISerializable)
            {
                ((ISerializable)value).Serialize(this);
            }
            else
            {
                throw new NotSupportedException(
                    "Unsupported type. It isn't a primitive type nor provides the ISerializable interface.");
            }
        }

        public void WriteArray<T>(T[] array)
        {
            Write(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                Write(array[i]);
            }
        }

        public void WriteVector<T>(List<T> vector)
        {
            Write(vector.Count);
            for (int i = 0; i < vector.Count; i++)
            {
                Write(vector[i]);
            }
        }

        public void WriteList<T>(LinkedList<T> list)
        {
            Write(list.Count);
            foreach (var element in list)
            {
                Write(element);
            }
        }

        public void WriteSet<T>(SortedSet<T> set)
        {
            Write(set.Count);
            foreach (var element in set)
            {
                Write(element);
            }
        }

        public void WriteMap<TKey, TValue>(Dictionary<TKey, TValue> map)
        {
            Write(map.Count);
            foreach (var element in map)
            {
                Write(element.Key);
                Write(element.Value);
            }
        }

        public void WritePair<T1, T2>(Tuple<T1,T2> pair)
        {
            Write(pair.Item1);
            Write(pair.Item2);
        }

        public byte[] GetData()
        {
            return _dataStream.ToArray();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}