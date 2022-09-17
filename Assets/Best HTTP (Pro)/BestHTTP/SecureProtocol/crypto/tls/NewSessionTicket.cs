#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
    public class NewSessionTicket
    {
        protected readonly byte[] mTicket;
        protected readonly long mTicketLifetimeHint;

        public NewSessionTicket(long ticketLifetimeHint, byte[] ticket)
        {
            mTicketLifetimeHint = ticketLifetimeHint;
            mTicket = ticket;
        }

        public virtual long TicketLifetimeHint => mTicketLifetimeHint;

        public virtual byte[] Ticket => mTicket;

        /**
         * Encode this {@link NewSessionTicket} to a {@link Stream}.
         * 
         * @param output the {@link Stream} to encode to.
         * @throws IOException
         */
        public virtual void Encode(Stream output)
        {
            TlsUtilities.WriteUint32(mTicketLifetimeHint, output);
            TlsUtilities.WriteOpaque16(mTicket, output);
        }

        /**
         * Parse a {@link NewSessionTicket} from a {@link Stream}.
         * 
         * @param input the {@link Stream} to parse from.
         * @return a {@link NewSessionTicket} object.
         * @throws IOException
         */
        public static NewSessionTicket Parse(Stream input)
        {
            var ticketLifetimeHint = TlsUtilities.ReadUint32(input);
            var ticket = TlsUtilities.ReadOpaque16(input);
            return new NewSessionTicket(ticketLifetimeHint, ticket);
        }
    }
}

#endif