using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uFCoderMulti
{
    using System.Runtime.InteropServices;
    using UFR_HANDLE = System.UIntPtr;

    enum CARD_SAK
    {
        UNKNOWN = 0x00,
        MIFARE_CLASSIC_1k = 0x08,
        MF1ICS50 = 0x08,
        SLE66R35 = 0x88,
        MIFARE_CLASSIC_4k = 0x18,
        MF1ICS70 = 0x18,
        MIFARE_CLASSIC_MINI = 0x09,
        MF1ICS20 = 0x09,
    }

    enum DLOGIC_CARD_TYPE
    {
        DL_NO_CARD = 0x00,
        DL_MIFARE_ULTRALIGHT = 0x01,
        DL_MIFARE_ULTRALIGHT_EV1_11 = 0x02,
        DL_MIFARE_ULTRALIGHT_EV1_21 = 0x03,
        DL_MIFARE_ULTRALIGHT_C = 0x04,
        DL_NTAG_203 = 0x05,
        DL_NTAG_210 = 0x06,
        DL_NTAG_212 = 0x07,
        DL_NTAG_213 = 0x08,
        DL_NTAG_215 = 0x09,
        DL_NTAG_216 = 0x0A,
        DL_MIKRON_MIK640D = 0x0B,
        NFC_T2T_GENERIC = 0x0C,
        DL_NT3H_1101 = 0x0D,
        DL_NT3H_1201 = 0x0E,
        DL_NT3H_2111 = 0x0F,
        DL_NT3H_2211 = 0x10,

        DL_MIFARE_MINI = 0x20,
        DL_MIFARE_CLASSIC_1K = 0x21,
        DL_MIFARE_CLASSIC_4K = 0x22,
        DL_MIFARE_PLUS_S_2K_SL0 = 0x23,
        DL_MIFARE_PLUS_S_4K_SL0 = 0x24,
        DL_MIFARE_PLUS_X_2K_SL0 = 0x25,
        DL_MIFARE_PLUS_X_4K_SL0 = 0x26,
        DL_MIFARE_DESFIRE = 0x27,
        DL_MIFARE_DESFIRE_EV1_2K = 0x28,
        DL_MIFARE_DESFIRE_EV1_4K = 0x29,
        DL_MIFARE_DESFIRE_EV1_8K = 0x2A,
        DL_MIFARE_DESFIRE_EV2_2K = 0x2B,
        DL_MIFARE_DESFIRE_EV2_4K = 0x2C,
        DL_MIFARE_DESFIRE_EV2_8K = 0x2D,
        DL_MIFARE_PLUS_S_2K_SL1 = 0x2E,
        DL_MIFARE_PLUS_X_2K_SL1 = 0x2F,
        DL_MIFARE_PLUS_EV1_2K_SL1 = 0x30,
        DL_MIFARE_PLUS_X_2K_SL2 = 0x31,
        DL_MIFARE_PLUS_S_2K_SL3 = 0x32,
        DL_MIFARE_PLUS_X_2K_SL3 = 0x33,
        DL_MIFARE_PLUS_EV1_2K_SL3 = 0x34,
        DL_MIFARE_PLUS_S_4K_SL1 = 0x35,
        DL_MIFARE_PLUS_X_4K_SL1 = 0x36,
        DL_MIFARE_PLUS_EV1_4K_SL1 = 0x37,
        DL_MIFARE_PLUS_X_4K_SL2 = 0x38,
        DL_MIFARE_PLUS_S_4K_SL3 = 0x39,
        DL_MIFARE_PLUS_X_4K_SL3 = 0x3A,
        DL_MIFARE_PLUS_EV1_4K_SL3 = 0x3B,

        // Special card type
        DL_GENERIC_ISO14443_4 = 0x40,
        DL_GENERIC_ISO14443_4_TYPE_B = 0x41,
        DL_GENERIC_ISO14443_3_TYPE_B = 0x42,

        DL_UNKNOWN_ISO_14443_4 = 0x40
    }

    // MIFARE CLASSIC Authentication Modes:
    enum MIFARE_AUTHENTICATION
    {
        MIFARE_AUTHENT1A = 0x60,
        MIFARE_AUTHENT1B = 0x61,
    }
    // MIFARE PLUS AES Authentication Modes:
    enum MIFARE_PLUS_AES_AUTHENTICATION
    {
        MIFARE_PLUS_AES_AUTHENT1A = 0x80,
        MIFARE_PLUS_AES_AUTHENT1B = 0x81,
    };

    enum T2T_AUTHENTICATION
    {
        T2T_NO_PWD_AUTH = 0,
        T2T_RKA_PWD_AUTH = 1,
        T2T_PK_PWD_AUTH = 3,
        T2T_WITHOUT_PWD_AUTH = 0x60,
        T2T_WITH_PWD_AUTH = 0x61,
    };

    // API Status Codes Type:
    public enum DL_STATUS
    {
        UFR_OK = 0x00,

        UFR_COMMUNICATION_ERROR = 0x01,
        UFR_CHKSUM_ERROR = 0x02,
        UFR_READING_ERROR = 0x03,
        UFR_WRITING_ERROR = 0x04,
        UFR_BUFFER_OVERFLOW = 0x05,
        UFR_MAX_ADDRESS_EXCEEDED = 0x06,
        UFR_MAX_KEY_INDEX_EXCEEDED = 0x07,
        UFR_NO_CARD = 0x08,
        UFR_COMMAND_NOT_SUPPORTED = 0x09,
        UFR_FORBIDEN_DIRECT_WRITE_IN_SECTOR_TRAILER = 0x0A,
        UFR_ADDRESSED_BLOCK_IS_NOT_SECTOR_TRAILER = 0x0B,
        UFR_WRONG_ADDRESS_MODE = 0x0C,
        UFR_WRONG_ACCESS_BITS_VALUES = 0x0D,
        UFR_AUTH_ERROR = 0x0E,
        UFR_PARAMETERS_ERROR = 0x0F, // ToDo, tačka 5.
        UFR_MAX_SIZE_EXCEEDED = 0x10,

        UFR_WRITE_VERIFICATION_ERROR = 0x70,
        UFR_BUFFER_SIZE_EXCEEDED = 0x71,
        UFR_VALUE_BLOCK_INVALID = 0x72,
        UFR_VALUE_BLOCK_ADDR_INVALID = 0x73,
        UFR_VALUE_BLOCK_MANIPULATION_ERROR = 0x74,
        UFR_WRONG_UI_MODE = 0x75,
        UFR_KEYS_LOCKED = 0x76,
        UFR_KEYS_UNLOCKED = 0x77,
        UFR_WRONG_PASSWORD = 0x78,
        UFR_CAN_NOT_LOCK_DEVICE = 0x79,
        UFR_CAN_NOT_UNLOCK_DEVICE = 0x7A,
        UFR_DEVICE_EEPROM_BUSY = 0x7B,
        UFR_RTC_SET_ERROR = 0x7C,

        UFR_COMMUNICATION_BREAK = 0x50,
        UFR_NO_MEMORY_ERROR = 0x51,
        UFR_CAN_NOT_OPEN_READER = 0x52,
        UFR_READER_NOT_SUPPORTED = 0x53,
        UFR_READER_OPENING_ERROR = 0x54,
        UFR_READER_PORT_NOT_OPENED = 0x55,
        UFR_CANT_CLOSE_READER_PORT = 0x56,

        UFR_FT_STATUS_ERROR_1 = 0xA0,
        UFR_FT_STATUS_ERROR_2 = 0xA1,
        UFR_FT_STATUS_ERROR_3 = 0xA2,
        UFR_FT_STATUS_ERROR_4 = 0xA3,
        UFR_FT_STATUS_ERROR_5 = 0xA4,
        UFR_FT_STATUS_ERROR_6 = 0xA5,
        UFR_FT_STATUS_ERROR_7 = 0xA6,
        UFR_FT_STATUS_ERROR_8 = 0xA7,
        UFR_FT_STATUS_ERROR_9 = 0xA8,

        //NDEF error codes
        UFR_WRONG_NDEF_CARD_FORMAT = 0x80,
        UFR_NDEF_MESSAGE_NOT_FOUND = 0x81,
        UFR_NDEF_UNSUPPORTED_CARD_TYPE = 0x82,
        UFR_NDEF_CARD_FORMAT_ERROR = 0x83,
        UFR_MAD_NOT_ENABLED = 0x84,
        UFR_MAD_VERSION_NOT_SUPPORTED = 0x85,

        // multi units
        UFR_DEVICE_WRONG_HANDLE = 0x100,
        UFR_DEVICE_INDEX_OUT_OF_BOUND,
        UFR_DEVICE_ALREADY_OPENED,
        UFR_DEVICE_ALREADY_CLOSED,

        MAX_UFR_STATUS = 10000000,
        UNKNOWN_ERROR = 2147483647 // 0x7FFFFFFF
    };

    public static unsafe class uFCoder
    {
        //--------------------------------------------------------------------------------------------------
#if WIN64
        public const string DLL_NAME = "uFCoder-x86_64.dll"; // for x64 target
#else
        public const string DLL_NAME = "uFCoder-x86.dll"; // for x86 target
#endif
        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderOpen")]
        public static extern DL_STATUS ReaderOpen();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderOpenEx")]
        private static extern DL_STATUS ReaderOpenEx(UInt32 reader_type, [In] byte[] port_name, UInt32 port_interface, [In] byte[] arg);
        public static DL_STATUS ReaderOpenEx(UInt32 reader_type, string port_name, UInt32 port_interface, string arg)
        {

            byte[] port_name_p = Encoding.UTF8.GetBytes(port_name);
            byte[] port_name_param = new byte[port_name_p.Length + 1];
            Array.Copy(port_name_p, 0, port_name_param, 0, port_name_p.Length);
            port_name_param[port_name_p.Length] = 0;

            byte[] arg_p = Encoding.UTF8.GetBytes(arg);
            byte[] arg_param = new byte[arg_p.Length + 1];
            Array.Copy(arg_p, 0, arg_param, 0, arg_p.Length);
            arg_param[arg_p.Length] = 0;

            return ReaderOpenEx(reader_type, port_name_param, port_interface, arg_param);
        }
        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderClose")]
        public static extern DL_STATUS ReaderClose();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderReset")]
        public static extern DL_STATUS ReaderReset();

        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetCardIdEx")]
        public static extern DL_STATUS GetCardIdEx(byte* bSak,
                                              byte* bCardUID,
                                              byte* bUidSize);
        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReaderUISignal")]
        public static extern DL_STATUS ReaderUISignal(byte light_signal_mode, byte beep_signal_mode);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderType")]
        public static extern DL_STATUS GetReaderType(uint* lpulReaderType);


        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderSerialDescription")]
        public static extern DL_STATUS GetReaderSerialDescription(byte* pSerialDescription);

        //--------------------------------------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "get_ndef_record_count")]
        public static extern DL_STATUS get_ndef_record_count(byte* ndef_message_cnt, byte* ndef_record_cnt, byte* ndef_record_array, byte* empty_ndef_message_cnt);

        //---------------------------------------------------------------------

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDlogicCardType")]
        public static extern DL_STATUS GetDlogicCardType(byte* lpucCardType);


        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetCardIdExM")]
        public static extern DL_STATUS GetCardIdExM(UFR_HANDLE hndUFR,
                                                    byte* bCardType,
                                                    byte* bCardUID,
                                                    byte* bUidSize);


        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDllVersion")]
        public static extern uint GetDllVersion();

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderHardwareVersion")]
        public static extern DL_STATUS GetReaderHardwareVersion(byte* version_major, byte* version_minor);

        //---------------------------------------------------------------------
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetReaderFirmwareVersion")]
        public static extern DL_STATUS GetReaderFirmwareVersion(byte* version_major, byte* version_minor);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuildNumber")]
        public static extern DL_STATUS GetBuildNumber(byte* build);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "BlockRead")]
        public static extern DL_STATUS BlockRead(byte[] data,
                                             UInt16 block_address,
                                             byte auth_mode,
                                             byte key_index);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "BlockRead_PK")]
        public static extern DL_STATUS BlockRead_PK(byte[] data, byte block_address, byte auth_mode, byte[] key);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "BlockWrite_PK")]
        public static extern DL_STATUS BlockWrite_PK(byte[] data, byte block_address, byte auth_mode, byte[] key);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "BlockWrite")]
        public static extern DL_STATUS BlockWrite([In] byte[] data,
                                                    UInt16 block_address,
                                                    byte auth_mode,
                                                    byte key_index);
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "BlockWrite_PK")]
        public static extern DL_STATUS BlockWrite_PK(byte[] data,
                                                   UInt16 block_address,
                                                   byte auth_mode,
                                                   byte[] pk_key);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "BlockRead_PK")]
        public static extern DL_STATUS BlockRead_PK(byte[] data,
                                                    UInt16 block_address,
                                                    byte auth_mode,
                                                    byte[] pk_key);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, EntryPoint = "ULC_write_3des_key_no_auth")]
        public static extern DL_STATUS ULC_write_3des_key_no_auth(byte[] newKey);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "uFR_SAM_DesfireWriteRecordAesAuth")]
        public static extern DL_STATUS uFR_SAM_DesfireWriteRecordAesAuth(byte aes_key_nr,uint aid,byte aid_key_nr,byte file_id,ushort offset,
            ushort data_length,byte communication_settings, byte[] data,ref ushort card_status,ref ushort exec_time);


        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "uFR_SAM_DesfireReadRecordsAesAuth")]
        public static extern DL_STATUS uFR_SAM_DesfireReadRecordsAesAuth(byte aes_key_nr,uint aid,byte aid_key_nr,byte file_id,
            ushort offset,ushort number_of_records,ushort record_size,byte communication_settings,[Out] byte[] data,
            ref ushort card_status,ref ushort exec_time);
    }
}

