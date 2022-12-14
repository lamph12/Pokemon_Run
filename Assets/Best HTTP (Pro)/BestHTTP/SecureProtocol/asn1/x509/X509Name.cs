#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;
using System.IO;
using System.Text;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
#if SILVERLIGHT || PORTABLE || NETFX_CORE
using System.Collections.Generic;
#endif

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * <pre>
     *     RDNSequence ::= SEQUENCE OF RelativeDistinguishedName
     *     RelativeDistinguishedName ::= SET SIZE (1..MAX) OF AttributeTypeAndValue
     *     AttributeTypeAndValue ::= SEQUENCE {
     *     type  OBJECT IDENTIFIER,
     *     value ANY }
     * </pre>
     */
    public class X509Name
        : Asn1Encodable
    {
        /**
        * country code - StringType(SIZE(2))
        */
        public static readonly DerObjectIdentifier C = new DerObjectIdentifier("2.5.4.6");

        /**
        * organization - StringType(SIZE(1..64))
        */
        public static readonly DerObjectIdentifier O = new DerObjectIdentifier("2.5.4.10");

        /**
        * organizational unit name - StringType(SIZE(1..64))
        */
        public static readonly DerObjectIdentifier OU = new DerObjectIdentifier("2.5.4.11");

        /**
        * Title
        */
        public static readonly DerObjectIdentifier T = new DerObjectIdentifier("2.5.4.12");

        /**
        * common name - StringType(SIZE(1..64))
        */
        public static readonly DerObjectIdentifier CN = new DerObjectIdentifier("2.5.4.3");

        /**
        * street - StringType(SIZE(1..64))
        */
        public static readonly DerObjectIdentifier Street = new DerObjectIdentifier("2.5.4.9");

        /**
        * device serial number name - StringType(SIZE(1..64))
        */
        public static readonly DerObjectIdentifier SerialNumber = new DerObjectIdentifier("2.5.4.5");

        /**
        * locality name - StringType(SIZE(1..64))
        */
        public static readonly DerObjectIdentifier L = new DerObjectIdentifier("2.5.4.7");

        /**
        * state, or province name - StringType(SIZE(1..64))
        */
        public static readonly DerObjectIdentifier ST = new DerObjectIdentifier("2.5.4.8");

        /**
        * Naming attributes of type X520name
        */
        public static readonly DerObjectIdentifier Surname = new DerObjectIdentifier("2.5.4.4");

        public static readonly DerObjectIdentifier GivenName = new DerObjectIdentifier("2.5.4.42");
        public static readonly DerObjectIdentifier Initials = new DerObjectIdentifier("2.5.4.43");
        public static readonly DerObjectIdentifier Generation = new DerObjectIdentifier("2.5.4.44");
        public static readonly DerObjectIdentifier UniqueIdentifier = new DerObjectIdentifier("2.5.4.45");

        /**
         * businessCategory - DirectoryString(SIZE(1..128)
         */
        public static readonly DerObjectIdentifier BusinessCategory = new DerObjectIdentifier(
            "2.5.4.15");

        /**
         * postalCode - DirectoryString(SIZE(1..40)
         */
        public static readonly DerObjectIdentifier PostalCode = new DerObjectIdentifier(
            "2.5.4.17");

        /**
         * dnQualifier - DirectoryString(SIZE(1..64)
         */
        public static readonly DerObjectIdentifier DnQualifier = new DerObjectIdentifier(
            "2.5.4.46");

        /**
         * RFC 3039 Pseudonym - DirectoryString(SIZE(1..64)
         */
        public static readonly DerObjectIdentifier Pseudonym = new DerObjectIdentifier(
            "2.5.4.65");

        /**
         * RFC 3039 DateOfBirth - GeneralizedTime - YYYYMMDD000000Z
         */
        public static readonly DerObjectIdentifier DateOfBirth = new DerObjectIdentifier(
            "1.3.6.1.5.5.7.9.1");

        /**
         * RFC 3039 PlaceOfBirth - DirectoryString(SIZE(1..128)
         */
        public static readonly DerObjectIdentifier PlaceOfBirth = new DerObjectIdentifier(
            "1.3.6.1.5.5.7.9.2");

        /**
         * RFC 3039 DateOfBirth - PrintableString (SIZE(1)) -- "M", "F", "m" or "f"
         */
        public static readonly DerObjectIdentifier Gender = new DerObjectIdentifier(
            "1.3.6.1.5.5.7.9.3");

        /**
         * RFC 3039 CountryOfCitizenship - PrintableString (SIZE (2)) -- ISO 3166
         * codes only
         */
        public static readonly DerObjectIdentifier CountryOfCitizenship = new DerObjectIdentifier(
            "1.3.6.1.5.5.7.9.4");

        /**
         * RFC 3039 CountryOfCitizenship - PrintableString (SIZE (2)) -- ISO 3166
         * codes only
         */
        public static readonly DerObjectIdentifier CountryOfResidence = new DerObjectIdentifier(
            "1.3.6.1.5.5.7.9.5");

        /**
         * ISIS-MTT NameAtBirth - DirectoryString(SIZE(1..64)
         */
        public static readonly DerObjectIdentifier NameAtBirth = new DerObjectIdentifier("1.3.36.8.3.14");

        /**
         * RFC 3039 PostalAddress - SEQUENCE SIZE (1..6) OF
         * DirectoryString(SIZE(1..30))
         */
        public static readonly DerObjectIdentifier PostalAddress = new DerObjectIdentifier("2.5.4.16");

        /**
         * RFC 2256 dmdName
         */
        public static readonly DerObjectIdentifier DmdName = new DerObjectIdentifier("2.5.4.54");

        /**
         * id-at-telephoneNumber
         */
        public static readonly DerObjectIdentifier TelephoneNumber = X509ObjectIdentifiers.id_at_telephoneNumber;

        /**
         * id-at-name
         */
        public static readonly DerObjectIdentifier Name = X509ObjectIdentifiers.id_at_name;

        /**
         * Email address (RSA PKCS#9 extension) - IA5String.
         * <p>Note: if you're trying to be ultra orthodox, don't use this! It shouldn't be in here.</p>
         */
        public static readonly DerObjectIdentifier EmailAddress = PkcsObjectIdentifiers.Pkcs9AtEmailAddress;

        /**
        * more from PKCS#9
        */
        public static readonly DerObjectIdentifier UnstructuredName = PkcsObjectIdentifiers.Pkcs9AtUnstructuredName;

        public static readonly DerObjectIdentifier UnstructuredAddress =
            PkcsObjectIdentifiers.Pkcs9AtUnstructuredAddress;

        /**
        * email address in Verisign certificates
        */
        public static readonly DerObjectIdentifier E = EmailAddress;

        /*
        * others...
        */
        public static readonly DerObjectIdentifier DC = new DerObjectIdentifier("0.9.2342.19200300.100.1.25");

        /**
        * LDAP User id.
        */
        public static readonly DerObjectIdentifier UID = new DerObjectIdentifier("0.9.2342.19200300.100.1.1");

        /**
         * determines whether or not strings should be processed and printed
         * from back to front.
         */
        //        public static bool DefaultReverse = false;
        public static bool DefaultReverse
        {
            get => defaultReverse[0];
            set => defaultReverse[0] = value;
        }

        private static readonly bool[] defaultReverse = { false };

#if SILVERLIGHT || NETFX_CORE || UNITY_WP8 || PORTABLE
        /**
        * default look up table translating OID values into their common symbols following
        * the convention in RFC 2253 with a few extras
        */
        public static readonly IDictionary DefaultSymbols = Org.BouncyCastle.Utilities.Platform.CreateHashtable();

        /**
         * look up table translating OID values into their common symbols following the convention in RFC 2253
         */
        public static readonly IDictionary RFC2253Symbols = Org.BouncyCastle.Utilities.Platform.CreateHashtable();

        /**
         * look up table translating OID values into their common symbols following the convention in RFC 1779
         *
         */
        public static readonly IDictionary RFC1779Symbols = Org.BouncyCastle.Utilities.Platform.CreateHashtable();

        /**
        * look up table translating common symbols into their OIDS.
        */
        public static readonly IDictionary DefaultLookup = Org.BouncyCastle.Utilities.Platform.CreateHashtable();
#else
        /**
         * default look up table translating OID values into their common symbols following
         * the convention in RFC 2253 with a few extras
         */
        public static readonly Hashtable DefaultSymbols = new Hashtable();

        /**
         * look up table translating OID values into their common symbols following the convention in RFC 2253
         */
        public static readonly Hashtable RFC2253Symbols = new Hashtable();

        /**
         * look up table translating OID values into their common symbols following the convention in RFC 1779
         */
        public static readonly Hashtable RFC1779Symbols = new Hashtable();

        /**
        * look up table translating common symbols into their OIDS.
        */
        public static readonly Hashtable DefaultLookup = new Hashtable();
#endif

        static X509Name()
        {
            DefaultSymbols.Add(C, "C");
            DefaultSymbols.Add(O, "O");
            DefaultSymbols.Add(T, "T");
            DefaultSymbols.Add(OU, "OU");
            DefaultSymbols.Add(CN, "CN");
            DefaultSymbols.Add(L, "L");
            DefaultSymbols.Add(ST, "ST");
            DefaultSymbols.Add(SerialNumber, "SERIALNUMBER");
            DefaultSymbols.Add(EmailAddress, "E");
            DefaultSymbols.Add(DC, "DC");
            DefaultSymbols.Add(UID, "UID");
            DefaultSymbols.Add(Street, "STREET");
            DefaultSymbols.Add(Surname, "SURNAME");
            DefaultSymbols.Add(GivenName, "GIVENNAME");
            DefaultSymbols.Add(Initials, "INITIALS");
            DefaultSymbols.Add(Generation, "GENERATION");
            DefaultSymbols.Add(UnstructuredAddress, "unstructuredAddress");
            DefaultSymbols.Add(UnstructuredName, "unstructuredName");
            DefaultSymbols.Add(UniqueIdentifier, "UniqueIdentifier");
            DefaultSymbols.Add(DnQualifier, "DN");
            DefaultSymbols.Add(Pseudonym, "Pseudonym");
            DefaultSymbols.Add(PostalAddress, "PostalAddress");
            DefaultSymbols.Add(NameAtBirth, "NameAtBirth");
            DefaultSymbols.Add(CountryOfCitizenship, "CountryOfCitizenship");
            DefaultSymbols.Add(CountryOfResidence, "CountryOfResidence");
            DefaultSymbols.Add(Gender, "Gender");
            DefaultSymbols.Add(PlaceOfBirth, "PlaceOfBirth");
            DefaultSymbols.Add(DateOfBirth, "DateOfBirth");
            DefaultSymbols.Add(PostalCode, "PostalCode");
            DefaultSymbols.Add(BusinessCategory, "BusinessCategory");
            DefaultSymbols.Add(TelephoneNumber, "TelephoneNumber");

            RFC2253Symbols.Add(C, "C");
            RFC2253Symbols.Add(O, "O");
            RFC2253Symbols.Add(OU, "OU");
            RFC2253Symbols.Add(CN, "CN");
            RFC2253Symbols.Add(L, "L");
            RFC2253Symbols.Add(ST, "ST");
            RFC2253Symbols.Add(Street, "STREET");
            RFC2253Symbols.Add(DC, "DC");
            RFC2253Symbols.Add(UID, "UID");

            RFC1779Symbols.Add(C, "C");
            RFC1779Symbols.Add(O, "O");
            RFC1779Symbols.Add(OU, "OU");
            RFC1779Symbols.Add(CN, "CN");
            RFC1779Symbols.Add(L, "L");
            RFC1779Symbols.Add(ST, "ST");
            RFC1779Symbols.Add(Street, "STREET");

            DefaultLookup.Add("c", C);
            DefaultLookup.Add("o", O);
            DefaultLookup.Add("t", T);
            DefaultLookup.Add("ou", OU);
            DefaultLookup.Add("cn", CN);
            DefaultLookup.Add("l", L);
            DefaultLookup.Add("st", ST);
            DefaultLookup.Add("serialnumber", SerialNumber);
            DefaultLookup.Add("street", Street);
            DefaultLookup.Add("emailaddress", E);
            DefaultLookup.Add("dc", DC);
            DefaultLookup.Add("e", E);
            DefaultLookup.Add("uid", UID);
            DefaultLookup.Add("surname", Surname);
            DefaultLookup.Add("givenname", GivenName);
            DefaultLookup.Add("initials", Initials);
            DefaultLookup.Add("generation", Generation);
            DefaultLookup.Add("unstructuredaddress", UnstructuredAddress);
            DefaultLookup.Add("unstructuredname", UnstructuredName);
            DefaultLookup.Add("uniqueidentifier", UniqueIdentifier);
            DefaultLookup.Add("dn", DnQualifier);
            DefaultLookup.Add("pseudonym", Pseudonym);
            DefaultLookup.Add("postaladdress", PostalAddress);
            DefaultLookup.Add("nameofbirth", NameAtBirth);
            DefaultLookup.Add("countryofcitizenship", CountryOfCitizenship);
            DefaultLookup.Add("countryofresidence", CountryOfResidence);
            DefaultLookup.Add("gender", Gender);
            DefaultLookup.Add("placeofbirth", PlaceOfBirth);
            DefaultLookup.Add("dateofbirth", DateOfBirth);
            DefaultLookup.Add("postalcode", PostalCode);
            DefaultLookup.Add("businesscategory", BusinessCategory);
            DefaultLookup.Add("telephonenumber", TelephoneNumber);
        }

        private readonly IList ordering = Platform.CreateArrayList();
        private readonly X509NameEntryConverter converter;

        private readonly IList values = Platform.CreateArrayList();
        private readonly IList added = Platform.CreateArrayList();
        private Asn1Sequence seq;

        /**
         * Return a X509Name based on the passed in tagged object.
         * 
         * @param obj tag object holding name.
         * @param explicitly true if explicitly tagged false otherwise.
         * @return the X509Name
         */
        public static X509Name GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static X509Name GetInstance(
            object obj)
        {
            if (obj == null || obj is X509Name)
                return (X509Name)obj;

            if (obj != null)
                return new X509Name(Asn1Sequence.GetInstance(obj));

            throw new ArgumentException("null object in factory", "obj");
        }

        protected X509Name()
        {
        }

        /**
         * Constructor from Asn1Sequence
         * 
         * the principal will be a list of constructed sets, each containing an (OID, string) pair.
         */
        protected X509Name(
            Asn1Sequence seq)
        {
            this.seq = seq;

            foreach (Asn1Encodable asn1Obj in seq)
            {
                var asn1Set = Asn1Set.GetInstance(asn1Obj.ToAsn1Object());

                for (var i = 0; i < asn1Set.Count; i++)
                {
                    var s = Asn1Sequence.GetInstance(asn1Set[i].ToAsn1Object());

                    if (s.Count != 2)
                        throw new ArgumentException("badly sized pair");

                    ordering.Add(DerObjectIdentifier.GetInstance(s[0].ToAsn1Object()));

                    var derValue = s[1].ToAsn1Object();
                    if (derValue is IAsn1String && !(derValue is DerUniversalString))
                    {
                        var v = ((IAsn1String)derValue).GetString();
                        if (Platform.StartsWith(v, "#")) v = "\\" + v;

                        values.Add(v);
                    }
                    else
                    {
                        values.Add("#" + Hex.ToHexString(derValue.GetEncoded()));
                    }

                    added.Add(i != 0);
                }
            }
        }

        /**
         * Constructor from a table of attributes with ordering.
         * <p>
         *     it's is assumed the table contains OID/string pairs, and the contents
         *     of the table are copied into an internal table as part of the
         *     construction process. The ordering ArrayList should contain the OIDs
         *     in the order they are meant to be encoded or printed in ToString.
         * </p>
         */
        public X509Name(
            IList ordering,
            IDictionary attributes)
            : this(ordering, attributes, new X509DefaultEntryConverter())
        {
        }

        /**
         * Constructor from a table of attributes with ordering.
         * <p>
         *     it's is assumed the table contains OID/string pairs, and the contents
         *     of the table are copied into an internal table as part of the
         *     construction process. The ordering ArrayList should contain the OIDs
         *     in the order they are meant to be encoded or printed in ToString.
         * </p>
         * <p>
         *     The passed in converter will be used to convert the strings into their
         *     ASN.1 counterparts.
         * </p>
         */
        public X509Name(
            IList ordering,
            IDictionary attributes,
            X509NameEntryConverter converter)
        {
            this.converter = converter;

            foreach (DerObjectIdentifier oid in ordering)
            {
                var attribute = attributes[oid];
                if (attribute == null)
                    throw new ArgumentException("No attribute for object id - " + oid +
                                                " - passed to distinguished name");

                this.ordering.Add(oid);
                added.Add(false);
                values.Add(attribute); // copy the hash table
            }
        }

        /**
        * Takes two vectors one of the oids and the other of the values.
        */
        public X509Name(
            IList oids,
            IList values)
            : this(oids, values, new X509DefaultEntryConverter())
        {
        }

        /**
         * Takes two vectors one of the oids and the other of the values.
         * <p>
         *     The passed in converter will be used to convert the strings into their
         *     ASN.1 counterparts.
         * </p>
         */
        public X509Name(
            IList oids,
            IList values,
            X509NameEntryConverter converter)
        {
            this.converter = converter;

            if (oids.Count != values.Count) throw new ArgumentException("'oids' must be same length as 'values'.");

            for (var i = 0; i < oids.Count; i++)
            {
                ordering.Add(oids[i]);
                this.values.Add(values[i]);
                added.Add(false);
            }
        }

        /**
         * Takes an X509 dir name as a string of the format "C=AU, ST=Victoria", or
         * some such, converting it into an ordered set of name attributes.
         */
        public X509Name(
            string dirName)
            : this(DefaultReverse, DefaultLookup, dirName)
        {
        }

        /**
         * Takes an X509 dir name as a string of the format "C=AU, ST=Victoria", or
         * some such, converting it into an ordered set of name attributes with each
         * string value being converted to its associated ASN.1 type using the passed
         * in converter.
         */
        public X509Name(
            string dirName,
            X509NameEntryConverter converter)
            : this(DefaultReverse, DefaultLookup, dirName, converter)
        {
        }

        /**
         * Takes an X509 dir name as a string of the format "C=AU, ST=Victoria", or
         * some such, converting it into an ordered set of name attributes. If reverse
         * is true, create the encoded version of the sequence starting from the
         * last element in the string.
         */
        public X509Name(
            bool reverse,
            string dirName)
            : this(reverse, DefaultLookup, dirName)
        {
        }

        /**
         * Takes an X509 dir name as a string of the format "C=AU, ST=Victoria", or
         * some such, converting it into an ordered set of name attributes with each
         * string value being converted to its associated ASN.1 type using the passed
         * in converter. If reverse is true the ASN.1 sequence representing the DN will
         * be built by starting at the end of the string, rather than the start.
         */
        public X509Name(
            bool reverse,
            string dirName,
            X509NameEntryConverter converter)
            : this(reverse, DefaultLookup, dirName, converter)
        {
        }

        /**
         * Takes an X509 dir name as a string of the format "C=AU, ST=Victoria", or
         * some such, converting it into an ordered set of name attributes. lookUp
         * should provide a table of lookups, indexed by lowercase only strings and
         * yielding a DerObjectIdentifier, other than that OID. and numeric oids
         * will be processed automatically.
         * <br />
         * If reverse is true, create the encoded version of the sequence
         * starting from the last element in the string.
         * @param reverse true if we should start scanning from the end (RFC 2553).
         * @param lookUp table of names and their oids.
         * @param dirName the X.500 string to be parsed.
         */
        public X509Name(
            bool reverse,
            IDictionary lookUp,
            string dirName)
            : this(reverse, lookUp, dirName, new X509DefaultEntryConverter())
        {
        }

        private DerObjectIdentifier DecodeOid(
            string name,
            IDictionary lookUp)
        {
            if (Platform.StartsWith(Platform.ToUpperInvariant(name), "OID."))
                return new DerObjectIdentifier(name.Substring(4));
            if (name[0] >= '0' && name[0] <= '9') return new DerObjectIdentifier(name);

            var oid = (DerObjectIdentifier)lookUp[Platform.ToLowerInvariant(name)];
            if (oid == null)
                throw new ArgumentException("Unknown object id - " + name + " - passed to distinguished name");

            return oid;
        }

        /**
         * Takes an X509 dir name as a string of the format "C=AU, ST=Victoria", or
         * some such, converting it into an ordered set of name attributes. lookUp
         * should provide a table of lookups, indexed by lowercase only strings and
         * yielding a DerObjectIdentifier, other than that OID. and numeric oids
         * will be processed automatically. The passed in converter is used to convert the
         * string values to the right of each equals sign to their ASN.1 counterparts.
         * <br />
         * @param reverse true if we should start scanning from the end, false otherwise.
         * @param lookUp table of names and oids.
         * @param dirName the string dirName
         * @param converter the converter to convert string values into their ASN.1 equivalents
         */
        public X509Name(
            bool reverse,
            IDictionary lookUp,
            string dirName,
            X509NameEntryConverter converter)
        {
            this.converter = converter;
            var nTok = new X509NameTokenizer(dirName);

            while (nTok.HasMoreTokens())
            {
                var token = nTok.NextToken();
                var index = token.IndexOf('=');

                if (index == -1) throw new ArgumentException("badly formated directory string");

                var name = token.Substring(0, index);
                var value = token.Substring(index + 1);
                var oid = DecodeOid(name, lookUp);

                if (value.IndexOf('+') > 0)
                {
                    var vTok = new X509NameTokenizer(value, '+');
                    var v = vTok.NextToken();

                    ordering.Add(oid);
                    values.Add(v);
                    added.Add(false);

                    while (vTok.HasMoreTokens())
                    {
                        var sv = vTok.NextToken();
                        var ndx = sv.IndexOf('=');

                        var nm = sv.Substring(0, ndx);
                        var vl = sv.Substring(ndx + 1);
                        ordering.Add(DecodeOid(nm, lookUp));
                        values.Add(vl);
                        added.Add(true);
                    }
                }
                else
                {
                    ordering.Add(oid);
                    values.Add(value);
                    added.Add(false);
                }
            }

            if (reverse)
            {
//				this.ordering.Reverse();
//				this.values.Reverse();
//				this.added.Reverse();
                var o = Platform.CreateArrayList();
                var v = Platform.CreateArrayList();
                var a = Platform.CreateArrayList();
                var count = 1;

                for (var i = 0; i < ordering.Count; i++)
                {
                    if (!(bool)added[i]) count = 0;

                    var index = count++;

                    o.Insert(index, ordering[i]);
                    v.Insert(index, values[i]);
                    a.Insert(index, added[i]);
                }

                ordering = o;
                values = v;
                added = a;
            }
        }

        /**
        * return an IList of the oids in the name, in the order they were found.
        */
        public IList GetOidList()
        {
            return Platform.CreateArrayList(ordering);
        }

        /**
         * return an IList of the values found in the name, in the order they
         * were found.
         */
        public IList GetValueList()
        {
            return GetValueList(null);
        }

        /**
         * return an IList of the values found in the name, in the order they
         * were found, with the DN label corresponding to passed in oid.
         */
        public IList GetValueList(DerObjectIdentifier oid)
        {
            var v = Platform.CreateArrayList();
            for (var i = 0; i != values.Count; i++)
                if (null == oid || oid.Equals(ordering[i]))
                {
                    var val = (string)values[i];

                    if (Platform.StartsWith(val, "\\#")) val = val.Substring(1);

                    v.Add(val);
                }

            return v;
        }

        public override Asn1Object ToAsn1Object()
        {
            if (seq == null)
            {
                var vec = new Asn1EncodableVector();
                var sVec = new Asn1EncodableVector();
                DerObjectIdentifier lstOid = null;

                for (var i = 0; i != ordering.Count; i++)
                {
                    var oid = (DerObjectIdentifier)ordering[i];
                    var str = (string)values[i];

                    if (lstOid == null
                        || (bool)added[i])
                    {
                    }
                    else
                    {
                        vec.Add(new DerSet(sVec));
                        sVec = new Asn1EncodableVector();
                    }

                    sVec.Add(
                        new DerSequence(
                            oid,
                            converter.GetConvertedValue(oid, str)));

                    lstOid = oid;
                }

                vec.Add(new DerSet(sVec));

                seq = new DerSequence(vec);
            }

            return seq;
        }

        /// <param name="other">The X509Name object to test equivalency against.</param>
        /// <param name="inOrder">
        ///     If true, the order of elements must be the same,
        ///     as well as the values associated with each element.
        /// </param>
        public bool Equivalent(
            X509Name other,
            bool inOrder)
        {
            if (!inOrder)
                return Equivalent(other);

            if (other == null)
                return false;

            if (other == this)
                return true;

            var orderingSize = ordering.Count;

            if (orderingSize != other.ordering.Count)
                return false;

            for (var i = 0; i < orderingSize; i++)
            {
                var oid = (DerObjectIdentifier)ordering[i];
                var oOid = (DerObjectIdentifier)other.ordering[i];

                if (!oid.Equals(oOid))
                    return false;

                var val = (string)values[i];
                var oVal = (string)other.values[i];

                if (!equivalentStrings(val, oVal))
                    return false;
            }

            return true;
        }

        /**
         * test for equivalence - note: case is ignored.
         */
        public bool Equivalent(
            X509Name other)
        {
            if (other == null)
                return false;

            if (other == this)
                return true;

            var orderingSize = ordering.Count;

            if (orderingSize != other.ordering.Count) return false;

            var indexes = new bool[orderingSize];
            int start, end, delta;

            if (ordering[0].Equals(other.ordering[0])) // guess forward
            {
                start = 0;
                end = orderingSize;
                delta = 1;
            }
            else // guess reversed - most common problem
            {
                start = orderingSize - 1;
                end = -1;
                delta = -1;
            }

            for (var i = start; i != end; i += delta)
            {
                var found = false;
                var oid = (DerObjectIdentifier)ordering[i];
                var value = (string)values[i];

                for (var j = 0; j < orderingSize; j++)
                {
                    if (indexes[j]) continue;

                    var oOid = (DerObjectIdentifier)other.ordering[j];

                    if (oid.Equals(oOid))
                    {
                        var oValue = (string)other.values[j];

                        if (equivalentStrings(value, oValue))
                        {
                            indexes[j] = true;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found) return false;
            }

            return true;
        }

        private static bool equivalentStrings(
            string s1,
            string s2)
        {
            var v1 = canonicalize(s1);
            var v2 = canonicalize(s2);

            if (!v1.Equals(v2))
            {
                v1 = stripInternalSpaces(v1);
                v2 = stripInternalSpaces(v2);

                if (!v1.Equals(v2)) return false;
            }

            return true;
        }

        private static string canonicalize(
            string s)
        {
            var v = Platform.ToLowerInvariant(s).Trim();

            if (Platform.StartsWith(v, "#"))
            {
                var obj = decodeObject(v);

                if (obj is IAsn1String) v = Platform.ToLowerInvariant(((IAsn1String)obj).GetString()).Trim();
            }

            return v;
        }

        private static Asn1Object decodeObject(
            string v)
        {
            try
            {
                return Asn1Object.FromByteArray(Hex.Decode(v.Substring(1)));
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("unknown encoding in name: " + e.Message, e);
            }
        }

        private static string stripInternalSpaces(
            string str)
        {
            var res = new StringBuilder();

            if (str.Length != 0)
            {
                var c1 = str[0];

                res.Append(c1);

                for (var k = 1; k < str.Length; k++)
                {
                    var c2 = str[k];
                    if (!(c1 == ' ' && c2 == ' ')) res.Append(c2);
                    c1 = c2;
                }
            }

            return res.ToString();
        }

        private void AppendValue(
            StringBuilder buf,
            IDictionary oidSymbols,
            DerObjectIdentifier oid,
            string val)
        {
            var sym = (string)oidSymbols[oid];

            if (sym != null)
                buf.Append(sym);
            else
                buf.Append(oid.Id);

            buf.Append('=');

            var index = buf.Length;

            buf.Append(val);

            var end = buf.Length;

            if (Platform.StartsWith(val, "\\#")) index += 2;

            while (index != end)
            {
                if (buf[index] == ','
                    || buf[index] == '"'
                    || buf[index] == '\\'
                    || buf[index] == '+'
                    || buf[index] == '='
                    || buf[index] == '<'
                    || buf[index] == '>'
                    || buf[index] == ';')
                {
                    buf.Insert(index++, "\\");
                    end++;
                }

                index++;
            }
        }

        /**
         * convert the structure to a string - if reverse is true the
         * oids and values are listed out starting with the last element
         * in the sequence (ala RFC 2253), otherwise the string will begin
         * with the first element of the structure. If no string definition
         * for the oid is found in oidSymbols the string value of the oid is
         * added. Two standard symbol tables are provided DefaultSymbols, and
         * RFC2253Symbols as part of this class.
         * 
         * @param reverse if true start at the end of the sequence and work back.
         * @param oidSymbols look up table strings for oids.
         */
        public string ToString(
            bool reverse,
            IDictionary oidSymbols)
        {
#if SILVERLIGHT || PORTABLE || NETFX_CORE
            List<object> components = new List<object>();
#else
            var components = new ArrayList();
#endif

            StringBuilder ava = null;

            for (var i = 0; i < ordering.Count; i++)
                if ((bool)added[i])
                {
                    ava.Append('+');
                    AppendValue(ava, oidSymbols,
                        (DerObjectIdentifier)ordering[i],
                        (string)values[i]);
                }
                else
                {
                    ava = new StringBuilder();
                    AppendValue(ava, oidSymbols,
                        (DerObjectIdentifier)ordering[i],
                        (string)values[i]);
                    components.Add(ava);
                }

            if (reverse) components.Reverse();

            var buf = new StringBuilder();

            if (components.Count > 0)
            {
                buf.Append(components[0]);

                for (var i = 1; i < components.Count; ++i)
                {
                    buf.Append(',');
                    buf.Append(components[i]);
                }
            }

            return buf.ToString();
        }

        public override string ToString()
        {
            return ToString(DefaultReverse, DefaultSymbols);
        }
    }
}

#endif