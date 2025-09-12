namespace UFSTWSSecuritySample
{

    public class Settings
    {
        public string PathPKCS12 { get; set; }

        public string PKCS12Passphrase { get; set; }

        public string PathPEM { get; set; }

        public bool LogRequest { get; set; }

        public bool LogResponse { get; set; }
    }
}