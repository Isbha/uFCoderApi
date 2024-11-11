namespace uFCoderApi.Models
{
    public class EncodeAndWriteRequest
    {
        public string Server_IP { get; set; }
        public int Port { get; set; } 
        public string CardNumber { get; set; } 
        public int Cardtype { get; set; } 
        public string Card_Memory_Sector { get; set; } 
        public string Room_Number { get; set; } 
        public bool IsMainKey { get; set; } 
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; } 
        public string SourceSystem { get; set; } 
        public string Authorisations_granted { get; set; } 
        public string Authorisations_denied { get; set; } 
        public int BlockNumber { get; set; } 
    }
}