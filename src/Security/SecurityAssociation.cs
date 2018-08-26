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
    public abstract class SecurityAssociation
    {
        private static readonly byte[] DEFAULT_CRYPTO_KEY = { 0xC7, 0xD8, 0xC4, 0xBF, 0xB5, 0xE9, 0xC0, 0xFD };
        private static readonly byte[] DEFAULT_AUTH_KEY = { 0xC0, 0xD3, 0xBD, 0xC3, 0xB7, 0xCE, 0xB8, 0xB8 };

        protected byte[] _cryptoKey;
        protected byte[] _authKey;

        protected ushort _spi;

        public SecurityAssociation(ushort spi = 0) : this(DEFAULT_CRYPTO_KEY, DEFAULT_AUTH_KEY, spi) { }

        public SecurityAssociation(byte[] cryptoKey, byte[] authKey, ushort spi = 0)
        {
            _cryptoKey = cryptoKey;
            _authKey = authKey;
            _spi = spi;
        }

        protected byte[] CalculateIcv(byte[] authData)
        {
            using (var hmac = new HMACMD5(_authKey))
            {
                byte[] hash = hmac.ComputeHash(authData);
                return hash.Take(Constants.ICV_LENGTH).ToArray();
            }
        }
    }
}