﻿using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;
using uFCoderApi.Models;
using uFCoderMulti;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace uFCoderApi.Controllers
{   

    [Route("api/[controller]")]
    [ApiController]
    public class ReaderController : ControllerBase
    {

        
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

        [HttpPost("encodeAndWrite")]
        public IActionResult EncodeAndWrite([FromBody] EncodeAndWriteRequest request)
        {
            // Validate request
            if (request == null ||
                string.IsNullOrEmpty(request.Server_IP) ||
                request.Port <= 0 ||
                string.IsNullOrEmpty(request.CardNumber) ||
                string.IsNullOrEmpty(request.Room_Number) ||
                string.IsNullOrEmpty(request.SourceSystem) ||
                string.IsNullOrEmpty(request.Authorisations_granted) ||
                string.IsNullOrEmpty(request.Authorisations_denied))
            {
                return BadRequest("Invalid request parameters.");
            }

            SaltoHelper saltoHelper = new SaltoHelper();

            Response encodeResponse = saltoHelper.Key_Encode_Binary(
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

            // Check if encoding was successful
            if (!encodeResponse.Status)
            {
                return StatusCode(500, $"Error encoding key: {encodeResponse.ResponseMessage}");
            }

            WriteMifareRequest writeRequest = new WriteMifareRequest
            {
                Data = encodeResponse.BinaryImage, 
                BlockNumber = request.BlockNumber 
            };

            IActionResult writeResult = WriteToMifare(writeRequest);
            if (writeResult is BadRequestObjectResult badRequest)
            {
                return badRequest; 
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
    }
}