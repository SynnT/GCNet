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
using System.IO;

namespace GCNet
{
    public sealed class PayloadWriter
    {
        private const int HEADER_LENGTH = 7;
        private const int ZEROES_PADDING_LENGTH = 4;
        private const int ZEROES_PADDING = 0;

        private ushort _opcode;

        public PayloadWriter(ushort opcode)
        {
            _opcode = opcode;
        }

        public byte[] GetPayload(byte[] content)
        {
            return BuildPayload(content, false);
        }

        public byte[] GetCompressedPayload(byte[] content)
        {
            return BuildPayload(content, true);
        }

        private byte[] BuildPayload(byte[] content, bool compress)
        {
            if (compress)
                content = GetCompressedContent(content);

            var payloadData = new byte[HEADER_LENGTH + content.Length + ZEROES_PADDING_LENGTH];
            using (var serializer = new BinarySerializer(payloadData))
            {
                serializer.Write(_opcode);
                serializer.Write(payloadData.Length);
                serializer.Write(compress);
                serializer.WriteRawBytes(content, 0, content.Length);
                serializer.Write(ZEROES_PADDING); // padding
            }
            return payloadData;
        }

        private byte[] GetCompressedContent(byte[] content)
        {
            var ms = new MemoryStream();
            using (var compressor = new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.Level1))
            {
                var writer = new BinaryWriter(ms);

                writer.Write(content.Length);
                compressor.Write(content, 0, content.Length);

                return ms.ToArray();
            }            
        }
    }
}