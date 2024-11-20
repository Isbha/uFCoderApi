namespace uFCoderApi.Models
{
    public class WriteDesfireRequest
    {
     public byte AesKeyNr { get; set; }
    public uint Aid { get; set; }
    public byte AidKeyNr { get; set; }
    public byte FileId { get; set; }
    public ushort Offset { get; set; }
    public ushort DataLength { get; set; }
    public byte CommunicationSettings { get; set; }
    public byte[] Data { get; set; }
    }
}
