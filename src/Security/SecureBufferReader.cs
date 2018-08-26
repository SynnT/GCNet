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
using System.IO;

namespace GCNet
{
    internal sealed class SecureBufferReader : IDisposable
    {
        private const int LENGTH_INDEX = 0;
        private const int SPI_INDEX = 2;        
        private const int AUTH_DATA_INDEX = 2;
        private const int IV_INDEX = 8;
        private const int PAYLOAD_INDEX = 16;

        private BinaryReader _reader;

        public SecureBufferReader(byte[] buffer, int startIndex)
        {
            var ms = new MemoryStream(buffer, startIndex, buffer.Length - startIndex);
            _reader = new BinaryReader(ms);
        }

        public ushort ReadSpi()
        {
            SetCurrentPosition(SPI_INDEX);
            return _reader.ReadUInt16();
        }

        public byte[] ReadIv()
        {
            SetCurrentPosition(IV_INDEX);
            return _reader.ReadBytes(Constants.IV_LENGTH);
        }

        public byte[] ReadPayload()
        {
            int payloadLength = ReadLength() - PAYLOAD_INDEX - Constants.ICV_LENGTH;

            SetCurrentPosition(PAYLOAD_INDEX);
            return _reader.ReadBytes(payloadLength);
        }

        public byte[] ReadStoredIcv()
        {
            int storedIcvIndex = ReadLength() - Constants.ICV_LENGTH;

            SetCurrentPosition(storedIcvIndex);
            return _reader.ReadBytes(Constants.ICV_LENGTH);
        }

        public byte[] ReadAuthenticatedData()
        {
            int authDataLength = ReadLength() - AUTH_DATA_INDEX - Constants.ICV_LENGTH;

            SetCurrentPosition(AUTH_DATA_INDEX);
            return _reader.ReadBytes(authDataLength);
        }

        private short ReadLength()
        {
            SetCurrentPosition(LENGTH_INDEX);
            return _reader.ReadInt16();
        }

        private void SetCurrentPosition(int newPosition)
        {
            _reader.BaseStream.Seek(newPosition, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}