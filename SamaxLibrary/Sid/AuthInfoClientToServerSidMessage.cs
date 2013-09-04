namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class represents a SID_AUTH_INFO message sent from the client to the server.
    /// It is the first message that is sent (except for the initial protocol byte) when logging
    /// onto Battle.Net and contains various information related to the authentication of the
    /// client.
    /// </summary>
    public class AuthInfoClientToServerSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="AuthInfoClientToServerSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.AuthInfo;

        /// <summary>
        /// Gets the protocol ID.
        /// </summary>
        /// TODO: Check if this value is 0 always.
        public Int32 ProtocolID { get; private set; }

        /// <summary>
        /// Gets the platform ID.
        /// </summary>
        public PlatformID PlatformID { get; private set; }

        /// <summary>
        /// Gets the product ID.
        /// </summary>
        public ProductID ProductID { get; private set; }

        /// <summary>
        /// Gets the version of the client.
        /// </summary>
        public Int32 Version { get; private set; }

        /// <summary>
        /// Gets the product language.
        /// </summary>
        public string ProductLanguage { get; private set; }

        /// <summary>
        /// Gets the local IPv4 address of the client.
        /// </summary>
        /// <remarks>
        /// <para>The address is in network byte order (big-endian). That is, the first byte of the
        /// array is the most significant byte, typically 192 in local area networks.</para>
        /// <para>This can supposedly be set to 0.0.0.0.</para>
        /// </remarks>
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "StyleCop bug not allowing IPv4 to be recognized properly.")]
        public byte[] LocalIpAddress { get; private set; }

        /// <summary>
        /// Gets the difference between UTC and local time in minutes.
        /// </summary>
        public Int32 TimeZoneBiasInMinutes { get; private set; }

        /// <summary>
        /// Gets the locale ID of the client's system.
        /// </summary>
        /// <remarks>The locale ID can be retrieved using the <c>GetLocaleInfo</c> function of the
        /// Windows API</remarks>
        public Int32 LocaleID { get; private set; }

        /// <summary>
        /// Gets the language ID of the client's system.
        /// </summary>
        /// <remarks>The language ID can be retrieved using the <c>GetUserDefaultLangID</c>
        /// function of the Windows API.</remarks>
        public Int32 LanguageID { get; private set; }

        /// <summary>
        /// Gets the country abbreviation of the client's system.
        /// </summary>
        /// <remarks>The country abbreviation can be retrieved using the <c>GetLocaleInfo</c>
        /// function of the Windows API.</remarks>
        public string CountryAbbreviation { get; private set; }

        /// <summary>
        /// Gets the country name of the client's system.
        /// </summary>
        /// <remarks>The country name can be retrieved using the <c>GetLocaleInfo</c> function of
        /// the Windows API.</remarks>
        public string Country { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthInfoClientToServerSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid client-to-server SID_AUTH_INFO message.</exception>
        public AuthInfoClientToServerSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.ProtocolID = parser.ReadInt32();
                if (this.ProtocolID != 0)
                {
                    throw new ArgumentException(
                        String.Format("The protocol ID ({0}) was not 0.", this.ProtocolID));
                }

                //// TODO: Validation for most of these is required
                //// Also consider turning some of the types into enumerations

                this.PlatformID = parser.ReadDwordStringAsEnum<PlatformID>();
                this.ProductID = parser.ReadDwordStringAsEnum<ProductID>();
                this.Version = parser.ReadInt32();
                this.ProductLanguage = parser.ReadDwordString();
                this.LocalIpAddress = parser.ReadByteArray(4); // TODO: Magic number (but obvious)
                this.TimeZoneBiasInMinutes = parser.ReadInt32();
                this.LocaleID = parser.ReadInt32();
                this.LanguageID = parser.ReadInt32();
                this.CountryAbbreviation = parser.ReadAsciiString();
                this.Country = parser.ReadAsciiString();
            }
            catch (SidByteParserException ex)
            {
                throw new ArgumentException(
                    String.Format("The bytes could not be parsed successfully: {0}", ex.Message),
                    ex);
            }

            if (parser.HasBytesLeft)
            {
                throw new ArgumentException("There were unexpected bytes at the end of the message.");
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="AuthInfoClientToServerSidMessage"/> class from
        /// high-level data.
        /// </summary>
        /// <param name="productID">The product ID.</param>
        /// <param name="version">The version of the client.</param>
        /// <param name="localIPAddress">An array of 4 bytes representing the local IP address.
        /// The address is in network byte order, so the first byte is the most significant byte.
        /// </param>
        /// <returns>An instance of the <see cref="AuthInfoClientToServerSidMessage"/> class with the
        /// specified data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="localIPAddress"/> is
        /// <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="productID"/> is not a constant of
        /// the <see cref="ProductID"/> enumeration, or <paramref name="localIPAddress"/> is not of
        /// length 4.</exception>
        public static AuthInfoClientToServerSidMessage CreateFromHighLevelData(
            ProductID productID,
            Int32 version,
            byte[] localIPAddress)
        {
            if (localIPAddress == null)
            {
                throw new ArgumentNullException("localIPAddress");
            }

            if (!Enum.IsDefined(typeof(ProductID), productID))
            {
                throw new ArgumentException(
                    String.Format(
                        "The specified product ID ({0}) is not a constant of the product ID enumeration.",
                        productID));
            }

            //// TODO: Validate the version

            if (localIPAddress.Length != 4)
            {
                throw new ArgumentException(
                    String.Format(
                        "The byte array representing the IP address is of invalid length ({0}).",
                        localIPAddress.Length));
            }

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            RegionInfo regionInfo = RegionInfo.CurrentRegion;

            Int32 protocolID = 0;
            PlatformID platformID = PlatformID.Ix86;

            //// TODO: Get the actual langauge in a manner like this
            ////string productLanguage = cultureInfo.TwoLetterISOLanguageName.ToLower() +
                ////regionInfo.TwoLetterISORegionName;
            string productLanguage = "enUS";

            Int32 timeZoneBiasInMinutes = (Int32)(DateTime.UtcNow - DateTime.Now).TotalMinutes;
            Int32 localeID = cultureInfo.LCID;
            Int32 languageID = cultureInfo.LCID;
            string countryAbbreviation = regionInfo.ThreeLetterISORegionName;
            string country = regionInfo.EnglishName;

            SidByteWriter writer = new SidByteWriter();
            writer.AppendInt32(protocolID);
            writer.AppendEnumAsDwordString(platformID);
            writer.AppendEnumAsDwordString(productID);
            writer.AppendInt32(version);
            writer.AppendDwordString(productLanguage);
            writer.AppendByteArray(localIPAddress);
            writer.AppendInt32(timeZoneBiasInMinutes);
            writer.AppendInt32(localeID);
            writer.AppendInt32(languageID);
            writer.AppendAsciiString(countryAbbreviation);
            writer.AppendAsciiString(country);

            byte[] dataBytes = writer.Bytes;
            byte[] messageBytes = SidMessage.GetMessageBytes(dataBytes, MessageType);

            return new AuthInfoClientToServerSidMessage(messageBytes);
        }
    }
}
