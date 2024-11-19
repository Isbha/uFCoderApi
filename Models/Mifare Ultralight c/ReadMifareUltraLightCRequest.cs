namespace uFCoderApi.Models
{
    public class ReadMifareUltraLightCRequest
    {
        public int PageNumber { get; set; }      // The page number to read (0-based)
        //public string AuthMode { get; set; }     // Authentication mode (e.g., "NO_AUTH", "RKA_AUTH", "PK_AUTH")
        public string Key { get; set; }          // Key in hexadecimal format (applicable for PK_AUTH)
    }
}
