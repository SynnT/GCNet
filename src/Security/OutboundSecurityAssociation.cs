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
using System.Linq;
using System.Security.Cryptography;

namespace GCNet
{
    public sealed class OutboundSecurityAssociation : SecurityAssociation
    {        
        private const int BLOCK_LENGTH = 8;

        private uint _sequenceNum = 1;

        public byte[] GetSecureBuffer(byte[] payload)
        {
            byte[] iv = GenerateIv();
            byte[] encryptedPayload = EncryptPayload(payload, iv);

            byte[] authData = WriteAuthData(iv, payload);
            byte[] icv = CalculateIcv(authData);

            return WriteSecureBuffer(authData, icv);
        }

        public void IncrementSequenceNum()
        {
            _sequenceNum++;
        }

        private static byte[] GenerateIv()
        {
            byte[] outputIV = new byte[8];
            byte ivByte;

            var random = new Random();
            ivByte = (byte)random.Next(0x00, 0xFF);

            for (int i = 0; i < outputIV.Length; i++)
            {
                outputIV[i] = ivByte;
            }
            return outputIV;
        }

        private byte[] EncryptPayload(byte[] payload, byte[] iv)
        {
            using (var cryptoProvider = new DESCryptoServiceProvider())
            using (ICryptoTransform encryptor = cryptoProvider.CreateEncryptor(_cryptoKey, iv))
            {
                byte[] padding = GeneratePadding(payload.Length);
                byte[] paddedData = payload.Concat(padding).ToArray();

                return encryptor.TransformFinalBlock(paddedData, 0, paddedData.Length);
            }
        }

        private byte[] GeneratePadding(int dataLength)
        {
            int gap = (BLOCK_LENGTH - dataLength % BLOCK_LENGTH) % BLOCK_LENGTH;

            int paddingLength = gap >= 2 ? gap : BLOCK_LENGTH + gap;
            var padding = new byte[paddingLength];

            for (byte i = 1; i < paddingLength; i++)
            {
                padding[i - 1] = i;
            }
            padding[paddingLength - 1] = padding[paddingLength - 2];

            return padding;
        }

        private byte[] WriteAuthData(byte[] iv, byte[] payload)
        {
            var authData = new byte[sizeof(ushort) + sizeof(uint) + Constants.IV_LENGTH + payload.Length];
            
            using (var writer = new BinaryWriter(new MemoryStream(authData)))
            {
                writer.Write(_spi);
                writer.Write(_sequenceNum);
                writer.Write(iv);
                writer.Write(payload);

                return authData;
            }
        }

        private static byte[] WriteSecureBuffer(byte[] authData, byte[] icv)
        {
            var secureBuffer = new byte[sizeof(short) + authData.Length + Constants.ICV_LENGTH];
            
            using (var writer = new BinaryWriter(new MemoryStream(secureBuffer)))
            {
                writer.Write((short)secureBuffer.Length);
                writer.Write(authData);
                writer.Write(icv);

                return secureBuffer;
            }
        }
    }
}