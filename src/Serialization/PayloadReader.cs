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

using Ionic.Zlib;

namespace GCNet
{
    public sealed class PayloadReader
    {
        int OPCODE_INDEX = 0;
        int CONTENT_LENGTH_INDEX = 2;
        int COMPRESSION_FLAG_INDEX = 6;
        int CONTENT_INDEX = 7;
        int COMPRESSED_DATA_INDEX = 11;

        private BinaryDeserializer _deserializer;

        public PayloadReader(byte[] payloadData)
        {
            _deserializer = new BinaryDeserializer(payloadData);
        }

        public ushort ReadOpcode()
        {
            _deserializer.Position = OPCODE_INDEX;
            return _deserializer.Read<ushort>();
        }

        public int ReadContentLength()
        {
            _deserializer.Position = CONTENT_LENGTH_INDEX;
            return _deserializer.Read<int>();
        }

        public bool ReadCompressionFlag()
        {
            _deserializer.Position = COMPRESSION_FLAG_INDEX;
            return _deserializer.Read<bool>();
        }

        public byte[] ReadContent()
        {
            if (ReadCompressionFlag())
            {
                int compressedDataLength = ReadContentLength() - 4;

                _deserializer.Position = COMPRESSED_DATA_INDEX;
                byte[] compressedData = _deserializer.ReadRawBytes(compressedDataLength);

                return DecompressData(compressedData);
            }
            else
                return ReadRawContent();
        }

        public byte[] ReadRawContent()
        {
            int contentLength = ReadContentLength();

            _deserializer.Position = CONTENT_INDEX;
            return _deserializer.ReadRawBytes(contentLength);
        }

        private byte[] DecompressData(byte[] compressedData)
        {
            return ZlibStream.UncompressBuffer(compressedData);
        }
    }
}