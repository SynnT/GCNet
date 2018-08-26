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

using System.Linq;
using System.Security.Cryptography;

namespace GCNet
{
    public sealed class InboundSecurityAssociation : SecurityAssociation
    {
        public bool IsSecureDataValid(byte[] secureBuffer, int startIndex = 0)
        {
            using (var reader = new SecureBufferReader(secureBuffer, startIndex))
            {
                ushort spi = reader.ReadSpi();
                byte[] authData = reader.ReadAuthenticatedData();
                byte[] storedIcv = reader.ReadStoredIcv();

                return IsSpiValid(spi) && IsIcvValid(authData, storedIcv);
            }
        }

        public byte[] GetPayload(byte[] secureBuffer, int startIndex = 0)
        {
            using (var reader = new SecureBufferReader(secureBuffer, startIndex))
            {
                byte[] iv = reader.ReadIv();
                byte[] encryptedPayload = reader.ReadPayload();

                return DecryptPayload(encryptedPayload, iv);
            }
        }

        private bool IsSpiValid(ushort spi)
        {
            return spi == _spi;
        }

        private bool IsIcvValid(byte[] authData, byte[] storedIcv)
        {
            byte[] expectedIcv = CalculateIcv(authData);
            return expectedIcv.SequenceEqual(storedIcv);
        }

        private byte[] DecryptPayload(byte[] encryptedPayload, byte[] iv)
        {
            using (var desProvider = new DESCryptoServiceProvider() { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (ICryptoTransform decryptor = desProvider.CreateDecryptor(_cryptoKey, iv))
            {
                byte[] decryptedPayload = decryptor.TransformFinalBlock(encryptedPayload, 0, encryptedPayload.Length);

                // the value of the last padding byte is always the padding length - 1
                int paddingLength = decryptedPayload.Last() + 1;
                return decryptedPayload.Take(decryptedPayload.Length - paddingLength).ToArray();
            }
        }
    }
}