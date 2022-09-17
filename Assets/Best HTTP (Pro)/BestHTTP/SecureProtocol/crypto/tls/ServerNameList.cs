#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    public class ServerNameList
    {
        protected readonly IList mServerNameList;

        /**
         * @param serverNameList an {@link IList} of {@link ServerName}.
         */
        public ServerNameList(IList serverNameList)
        {
            if (serverNameList == null)
                throw new ArgumentNullException("serverNameList");

            mServerNameList = serverNameList;
        }

        /**
         * @return an {@link IList} of {@link ServerName}.
         */
        public virtual IList ServerNames => mServerNameList;

        /**
         * Encode this {@link ServerNameList} to a {@link Stream}.
         * 
         * @param output
         * the {@link Stream} to encode to.
         * @throws IOException
         */
        public virtual void Encode(Stream output)
        {
            var buf = new MemoryStream();

            var nameTypesSeen = TlsUtilities.EmptyBytes;
            foreach (ServerName entry in ServerNames)
            {
                nameTypesSeen = CheckNameType(nameTypesSeen, entry.NameType);
                if (nameTypesSeen == null)
                    throw new TlsFatalAlert(AlertDescription.internal_error);

                entry.Encode(buf);
            }

            TlsUtilities.CheckUint16(buf.Length);
            TlsUtilities.WriteUint16((int)buf.Length, output);
            buf.WriteTo(output);
        }

        /**
         * Parse a {@link ServerNameList} from a {@link Stream}.
         * 
         * @param input
         * the {@link Stream} to parse from.
         * @return a {@link ServerNameList} object.
         * @throws IOException
         */
        public static ServerNameList Parse(Stream input)
        {
            var length = TlsUtilities.ReadUint16(input);
            if (length < 1)
                throw new TlsFatalAlert(AlertDescription.decode_error);

            var data = TlsUtilities.ReadFully(length, input);

            var buf = new MemoryStream(data, false);

            var nameTypesSeen = TlsUtilities.EmptyBytes;
            var server_name_list = Platform.CreateArrayList();
            while (buf.Position < buf.Length)
            {
                var entry = ServerName.Parse(buf);

                nameTypesSeen = CheckNameType(nameTypesSeen, entry.NameType);
                if (nameTypesSeen == null)
                    throw new TlsFatalAlert(AlertDescription.illegal_parameter);

                server_name_list.Add(entry);
            }

            return new ServerNameList(server_name_list);
        }

        private static byte[] CheckNameType(byte[] nameTypesSeen, byte nameType)
        {
            /*
             * RFC 6066 3. The ServerNameList MUST NOT contain more than one name of the same
             * name_type.
             */
            if (!NameType.IsValid(nameType) || Arrays.Contains(nameTypesSeen, nameType))
                return null;

            return Arrays.Append(nameTypesSeen, nameType);
        }
    }
}

#endif