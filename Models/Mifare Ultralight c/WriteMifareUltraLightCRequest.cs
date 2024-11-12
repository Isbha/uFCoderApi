namespace uFCoderApi.Models
{
    public class WriteMifareUltraLightCRequest
    {
        public string Data { get; set; }         // Data to write (in hexadecimal format)
        public int PageNumber { get; set; }      // The page number to write to (0-based)
        public string AuthMode { get; set; }     // Authentication mode (e.g., "NO_AUTH", "RKA_AUTH", "PK_AUTH")
        public string KeyIndex { get; set; }     // Key index (as a string, applicable for RKA_AUTH)
        public string Key { get; set; }          // Key in hexadecimal format (applicable for PK_AUTH)
    }
}