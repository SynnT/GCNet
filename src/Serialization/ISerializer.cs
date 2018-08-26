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

namespace GCNet
{
    public interface ISerializer
    {
        void WriteRawBytes(byte[] bytes, int startIndex, int count);
        void Write<T>(T value);
        void WriteArray<T>(T[] array);
        void WriteList<T>(LinkedList<T> list);
        void WriteMap<TKey, TValue>(Dictionary<TKey, TValue> map);
        void WritePair<T1, T2>(Tuple<T1, T2> pair);
        void WriteSet<T>(SortedSet<T> set);
        void WriteVector<T>(List<T> vector);
        byte[] GetData();
    }
}