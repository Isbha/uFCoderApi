namespace uFCoderApi.Models
{
    public class ReadMifareUltraCRequest
    {
        public int PageNumber { get; set; }      // The page number to read (0-based)
        public string AuthMode { get; set; }     // Authentication mode (e.g., "NO_AUTH", "RKA_AUTH", "PK_AUTH")
        public string KeyIndex { get; set; }     // Key index (as a string, applicable for RKA_AUTH)
        public string Key { get; set; }          // Key in hexadecimal format (applicable for PK_AUTH)
    }
}
