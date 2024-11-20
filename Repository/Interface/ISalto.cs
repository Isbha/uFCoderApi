namespace uFCoderApi.Repository.Interface
{
    public interface ISalto
    {
        Task<Response> Key_Encode_Binary(string Server_IP, int port, string CardNumber, int Cardtype, string Card_Memory_Sector, string Room_Number, bool isMainKey, DateTime StartDate, DateTime EndDate, string SourceSystem, string Authorisations_granted, string Authorisations_denied);

    }
}
