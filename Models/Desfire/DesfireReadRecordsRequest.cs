namespace uFCoderApi.Models.Desfire
{
    public class DesfireReadRecordsRequest
    {
        public byte AesKeyNr { get; set; }
        public uint Aid { get; set; }
        public byte AidKeyNr { get; set; }
        public byte FileId { get; set; }
        public ushort Offset { get; set; }
        public ushort NumberOfRecords { get; set; }
        public ushort RecordSize { get; set; }
        public byte CommunicationSettings { get; set; }
    }
    public class DesfireReadRecordsResponse
    {
        public string Status { get; set; }
        public string Data { get; set; }
        public ushort CardStatus { get; set; }
        public ushort ExecTime { get; set; }
    }
}
