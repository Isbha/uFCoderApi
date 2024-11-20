using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Xml.Linq;
using uFCoderApi.Models;
using uFCoderApi.Models.Desfire;
using uFCoderMulti;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SamsoWebhost.Saflok;
using uFCoderApi.Repository.Interface;
using SamsoWebhost.Saflok.Models;

namespace uFCoderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReaderController : ControllerBase
    {

        private readonly ISaflok _saflokService;
        private readonly ISalto _saltoService;
        public ReaderController(ISaflok saflokService, ISalto saltoService)
        {
            _saflokService = saflokService;
            _saltoService = saltoService;
        }
        [HttpPost("openConnection")]
        public IActionResult OpenReader([FromBody] ReaderOpenRequest request)
        {
            // Validate request
            if (request == null ||
                string.IsNullOrEmpty(request.ReaderType) ||
                string.IsNullOrEmpty(request.PortName) ||
                string.IsNullOrEmpty(request.PortInterface))
            {
                return BadRequest("Invalid request parameters.");
            }

            DL_STATUS status = reader_open_ex(request);

            // Map status to response
            if (status != DL_STATUS.UFR_OK)
            {
                return StatusCode(500, $"Error opening reader: {status}");
            }

            return Ok("Reader opened successfully.");
        }
        [HttpPost("writeToMifare")]
        public IActionResult WriteToMifare([FromBody] WriteMifareRequest request)
        {

            DL_STATUS status;
            byte[] data = new byte[16];

            byte[] dataToWrite = Encoding.ASCII.GetBytes(request.Data);
            int dataSize = Math.Min(dataToWrite.Length, data.Length);
            Array.Copy(dataToWrite, data, dataSize);

            byte blockAddress = (byte)request.BlockNumber;
            byte auth_mode = (byte)MIFARE_AUTHENTICATION.MIFARE_AUTHENT1B;
            byte[] key = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            status = uFCoder.BlockWrite_PK(data, blockAddress, auth_mode, key);

            if (status != DL_STATUS.UFR_OK)
            {
                return StatusCode(500, $"Error writing data to MIFARE card: {status}");
            }

            return Ok("Data written successfully.");
        }
        [HttpPost("readFromMifare")]
        public IActionResult ReadFromMifare([FromBody] ReadMifareRequest request)
        {
            if (request == null || request.BlockNumber < 0)
            {
                return BadRequest("Invalid request parameters.");
            }

            DL_STATUS status;
            byte[] data = new byte[16];
            byte blockAddress = (byte)request.BlockNumber;
            byte auth_mode = (byte)MIFARE_AUTHENTICATION.MIFARE_AUTHENT1B;
            byte[] key = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            status = uFCoder.BlockRead_PK(data, blockAddress, auth_mode, key);

            if (status != DL_STATUS.UFR_OK)
            {

                return StatusCode(500, $"Error reading data from MIFARE card: {status}");
            }

            string readData = System.Text.Encoding.ASCII.GetString(data);
            readData = readData.Replace("\0", ""); // Remove null characters
            return Ok(new { Message = "Data read successfully.", Data = readData });
        }


        [HttpPost("writeToUltralightC")]
        public IActionResult WriteToUltralight([FromBody] WriteMifareUltraLightCRequest request)
        {
            if (request == null || request.PageNumber < 2 || request.PageNumber > 47 || string.IsNullOrEmpty(request.Data) || string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Invalid request parameters.");
            }


            if (request.Key.Length != 32)
            {
                return BadRequest("Invalid key format. Key must be 16 bytes (32 hexadecimal characters).");
            }

            DL_STATUS status;
            byte[] dataToWrite = Encoding.ASCII.GetBytes(request.Data);
            byte[] key = ConvertHexStringToByteArray(request.Key);
            byte pageAddress = (byte)request.PageNumber;

            // Writing data to the specified page on the Ultralight C card using 3DES key (for Mifare Plus AES)
            status = uFCoder.BlockWrite_PK(dataToWrite, pageAddress, (byte)MIFARE_PLUS_AES_AUTHENTICATION.MIFARE_PLUS_AES_AUTHENT1A, key);

            if (status != DL_STATUS.UFR_OK)
            {
                return StatusCode(500, $"Error writing data to the Ultralight C card: {status}");
            }

            return Ok("Data written successfully to the Ultralight C card.");
        }
        [HttpPost("readFromUltralightC")]
        public IActionResult ReadFromUltralight([FromBody] ReadMifareUltraLightCRequest request)
        {
            if (request == null || request.PageNumber < 0 || request.PageNumber > 43 || string.IsNullOrEmpty(request.Key))
            {
                return BadRequest("Invalid request parameters.");
            }

            if (request.Key.Length != 32)
            {
                return BadRequest("Invalid key format. Key must be 16 bytes (32 hexadecimal characters).");
            }
            byte[] showData = new byte[4];

            DL_STATUS status;
            byte[] key = ConvertHexStringToByteArray(request.Key);
            byte pageAddress = (byte)request.PageNumber;
            byte[] dataRead = new byte[4];

            status = uFCoder.BlockRead_PK(dataRead, pageAddress, (byte)MIFARE_PLUS_AES_AUTHENTICATION.MIFARE_PLUS_AES_AUTHENT1A, key);

            if (status != DL_STATUS.UFR_OK)
            {
                return StatusCode(500, $"Error reading data from the Ultralight C card: {status}");
            }
            string dataString = Encoding.ASCII.GetString(dataRead);

            return Ok($"Data read successfully: {dataString}");
        }
        [HttpPost("writeToDesfire")]
        public IActionResult writeToDesfire([FromBody] WriteDesfireRequest request)
        {
            try
            {
                // Call the DESFire write function
                var result = WriteDataToDesfire(request);

                if (result.Status == "Success")
                    return Ok(new { message = "Data written successfully", execTime = result.ExecutionTime });

                return BadRequest(new { message = "Failed to write data", details = result.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        [HttpPost("readFromDesfire")]
        public IActionResult readFromDesfire([FromBody] DesfireReadRecordsRequest request)
        {
            const int MAX_DATA_LENGTH = 1024;
            byte[] dataBuffer = new byte[MAX_DATA_LENGTH];
            ushort cardStatus = 0;
            ushort execTime = 0;

            try
            {
                DL_STATUS status = uFCoder.uFR_SAM_DesfireReadRecordsAesAuth(
                    request.AesKeyNr,
                    request.Aid,
                    request.AidKeyNr,
                    request.FileId,
                    request.Offset,
                    request.NumberOfRecords,
                    request.RecordSize,
                    request.CommunicationSettings,
                    dataBuffer,
                    ref cardStatus,
                    ref execTime
                );

                if (status == DL_STATUS.UFR_OK) // Assuming 0 indicates success
                {
                    string readData = System.Text.Encoding.ASCII.GetString(dataBuffer).TrimEnd('\0');
                    return Ok(new DesfireReadRecordsResponse
                    {
                        Status = "Success",
                        Data = readData,
                        CardStatus = cardStatus,
                        ExecTime = execTime
                    });
                }

                return BadRequest(new { Status = "Error", Code = status, CardStatus = cardStatus });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost("setUltralightCKey")]
        public IActionResult SetUltralightCKey()
        {
            string hardcodedNewKeyHex = "A1B2C3D4E5F60123456789ABCDEF1234";

            byte[] newKey = ConvertHexStringToByteArray(hardcodedNewKeyHex);


            DL_STATUS status = uFCoder.ULC_write_3des_key_no_auth(newKey);

            if (status != DL_STATUS.UFR_OK)
            {
                return StatusCode(500, $"Failed to set the new key: Error Code {status}");
            }

            return Ok("New key successfully set on the Ultralight C card.");
        }
        [HttpPost("SaflokWriteDataToUltralightC")]
        public async Task<IActionResult> SaflokWriteDataToUltralightC([FromBody] KeyCardRequest request, [FromQuery] string username, [FromQuery] string password, [FromQuery] string url)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request payload." });
            }

            try
            {
               
                var safLokResult = await _saflokService.CreateKey(request, username, password, url);

                if (!safLokResult.result)
                {
                    return BadRequest(new { message = safLokResult.message });
                }

             
                byte[] accessKeyData = Convert.FromBase64String(safLokResult.retAccessKey);
                byte[] keySetData = Convert.FromBase64String(safLokResult.retKeySet);

                byte[] first8Bytes = new byte[8];
                Array.Copy(keySetData, 0, first8Bytes, 0, 8);

                byte[] second8Bytes = new byte[8];
                Array.Copy(keySetData, 8, second8Bytes, 0, 8);

                byte nextByte = keySetData[16];
                byte lastByte = keySetData[17];

              
                string hardcodedNewKeyHex = "A1B2C3D4E5F60123456789ABCDEF1234";
                byte[] Key = ConvertHexStringToByteArray(hardcodedNewKeyHex);

                DL_STATUS status;

               
                for (byte pageAddress = 4; pageAddress <= 39; pageAddress++)
                {
                    int dataStartIndex = (pageAddress - 4) * 4; 
                    byte[] dataToWrite = accessKeyData.Skip(dataStartIndex).Take(4).ToArray();

                    status = uFCoder.BlockWrite_PK(dataToWrite, pageAddress, (byte)MIFARE_PLUS_AES_AUTHENTICATION.MIFARE_PLUS_AES_AUTHENT1A, Key);
                    if (status != DL_STATUS.UFR_OK)
                    {
                        return StatusCode(500, $"Error writing data to the Ultralight C card at page {pageAddress}: {status}");
                    }
                }

                for (byte pageAddress = 44; pageAddress <= 45; pageAddress++)
                {
                    status = uFCoder.BlockWrite_PK(first8Bytes, pageAddress, (byte)MIFARE_PLUS_AES_AUTHENTICATION.MIFARE_PLUS_AES_AUTHENT1A, Key);
                    if (status != DL_STATUS.UFR_OK)
                    {
                        return StatusCode(500, $"Error writing data to the Ultralight C card at page {pageAddress}: {status}");
                    }
                }

                for (byte pageAddress = 46; pageAddress <= 47; pageAddress++)
                {
                    status = uFCoder.BlockWrite_PK(second8Bytes, pageAddress, (byte)MIFARE_PLUS_AES_AUTHENTICATION.MIFARE_PLUS_AES_AUTHENT1A, Key);
                    if (status != DL_STATUS.UFR_OK)
                    {
                        return StatusCode(500, $"Error writing data to the Ultralight C card at page {pageAddress}: {status}");
                    }
                }

                status = uFCoder.BlockWrite_PK(new byte[] { nextByte }, 42, (byte)MIFARE_PLUS_AES_AUTHENTICATION.MIFARE_PLUS_AES_AUTHENT1A, Key);
                if (status != DL_STATUS.UFR_OK)
                {
                    return StatusCode(500, $"Error writing data to the Ultralight C card at page 42: {status}");
                }

                status = uFCoder.BlockWrite_PK(new byte[] { lastByte }, 43, (byte)MIFARE_PLUS_AES_AUTHENTICATION.MIFARE_PLUS_AES_AUTHENT1A, Key);
                if (status != DL_STATUS.UFR_OK)
                {
                    return StatusCode(500, $"Error writing data to the Ultralight C card at page 43: {status}");
                }

                return Ok(new
                {
                    message = "Key and data written successfully to the Ultralight C card.",
                    AccessKey = safLokResult.retAccessKey,
                    KeySet = safLokResult.retKeySet
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", details = ex.Message });
            }
        }

        [HttpPost("encodeAndWrite")]
        public async Task<IActionResult> EncodeAndWrite([FromBody] EncodeAndWriteRequest request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Server_IP) ||
                request.Port <= 0 ||
                string.IsNullOrEmpty(request.CardNumber) ||
                string.IsNullOrEmpty(request.Room_Number) ||
                string.IsNullOrEmpty(request.SourceSystem))
            {
                return BadRequest("Invalid request parameters.");
            }

            Response encodeResponse = await _saltoService.Key_Encode_Binary(
                request.Server_IP,
                request.Port,
                request.CardNumber,
                request.Cardtype,
                request.Card_Memory_Sector,
                request.Room_Number,
                request.IsMainKey,
                request.StartDate,
                request.EndDate,
                request.SourceSystem,
                request.Authorisations_granted,
                request.Authorisations_denied
            );

            if (!encodeResponse.Status)
            {
                return StatusCode(500, $"Error encoding key: {encodeResponse.ResponseMessage}");
            }
            string binaryImage = encodeResponse.BinaryImage;
            string[] entries = binaryImage.Split(',');

            for (int i = 1; i < entries.Length; i += 3)
            {

                if (i + 3 <= entries.Length)
                {
                    int sector = int.Parse(entries[i]);
                    int block = int.Parse(entries[i + 1]);
                    string data = entries[i + 2];


                    WriteMifareRequest writeRequest = new WriteMifareRequest
                    {
                        Data = data,
                        BlockNumber = (sector * 4) + block
                    };

                    // Write to Mifare
                    IActionResult writeResult = WriteToMifare(writeRequest);
                    if (writeResult is BadRequestObjectResult badRequest)
                    {
                        return badRequest;
                    }
                }
            }
            return Ok("Binary image written to card successfully.");
        }


        [HttpPost("closeConnection")]
        public IActionResult CloseReader()
        {
            // Call the method to close the reader
            uFCoder.ReaderClose();

            return Ok("Reader closed successfully.");
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private DL_STATUS reader_open_ex(ReaderOpenRequest request)
        {
            DL_STATUS status = 0;

            uint reader_type;
            byte[] reader_sn = new byte[8];
            byte fw_major_ver;
            byte fw_minor_ver;
            byte fw_build;
            byte hw_major;
            byte hw_minor;

            UInt32 reader_type_int = 0, port_interface_int = 0;

            try
            {
                reader_type_int = Convert.ToUInt32(request.ReaderType);
            }
            catch
            {
                return DL_STATUS.UFR_READER_OPENING_ERROR;
            }

            try
            {
                port_interface_int = (UInt32)request.PortInterface[0];
            }
            catch
            {
                return DL_STATUS.UFR_READER_OPENING_ERROR;
            }

            status = uFCoder.ReaderOpenEx(reader_type_int, request.PortName, port_interface_int, request.Arg);

            if (status != DL_STATUS.UFR_OK)
            {
                return status;
            }

            unsafe
            {
                fixed (byte* f_rdsn = reader_sn)
                    status = uFCoder.GetReaderSerialDescription(f_rdsn);
            }

            unsafe
            {
                status |= uFCoder.GetReaderType(&reader_type);
                status |= uFCoder.GetReaderHardwareVersion(&hw_major, &hw_minor);
                status |= uFCoder.GetReaderFirmwareVersion(&fw_major_ver, &fw_minor_ver);
                status |= uFCoder.GetBuildNumber(&fw_build);
            }

            return status;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (string.IsNullOrEmpty(hexString) || hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid hexadecimal string.");
            }

            byte[] byteArray = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return byteArray;
        }
        private WriteResult WriteDataToDesfire(WriteDesfireRequest request)
        {
            // Example of using the uFR_SAM_DesfireWriteRecordAesAuth function
            UInt16 cardStatus = 0;
            UInt16 execTime = 0;

            // Call the library function
            DL_STATUS status = uFCoder.uFR_SAM_DesfireWriteRecordAesAuth(
                request.AesKeyNr,
                request.Aid,
                request.AidKeyNr,
                request.FileId,
                request.Offset,
                request.DataLength,
                request.CommunicationSettings,
                request.Data,
                ref cardStatus,
                ref execTime
            );

            // Map the status to a result object
            return new WriteResult
            {
                Status = status == DL_STATUS.UFR_OK ? "Success" : status.ToString(),
                CardStatus = cardStatus,
                ExecutionTime = execTime
            };
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public static byte[] ToByteArray(string HexString)
        {

            int NumberChars = HexString.Length;
            byte[] bytes = new byte[NumberChars / 2];

            if (HexString.Length % 2 != 0)
            {
                return bytes;
            }

            for (int i = 0; i < NumberChars; i += 2)
            {
                try
                {
                    bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
                }
                catch (System.FormatException)
                {
                    break;
                }
            }

            return bytes;
        }

        private byte[] StringToByteArray(string str)
        {
            // Convert a hexadecimal string to byte array
            int len = str.Length;
            byte[] arr = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                arr[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }
            return arr;
        }
        public class WriteResult
        {
            public string Status { get; set; }
            public ushort CardStatus { get; set; }
            public ushort ExecutionTime { get; set; }
        }

    }
}


