using Flurl.Http;
using System.Security.Cryptography.X509Certificates;

namespace apple_wallet
{
    public class AppleWalletConfiguration
    {
        public string WWDRCertificateBase64 { get; set; }
        public string PassTypeIdentifier { get; set; }
        public string PassbookCertificateBase64 { get; set; }
        public string PassbookPassword { get; set; }
        public string IconUrl { get; set; }
        public string LogoUrl { get; set; }

        public X509Certificate2 AppleWWDRCACertificate() =>
            new(Convert.FromBase64String(WWDRCertificateBase64));

        public X509Certificate2 PassbookCertificate() =>
            new(Convert.FromBase64String(PassbookCertificateBase64), PassbookPassword);

        public async Task<byte[]> GetLogo() => await LogoUrl.GetBytesAsync();

        public async Task<byte[]> GetIcon() => await IconUrl.GetBytesAsync();
    }
}
