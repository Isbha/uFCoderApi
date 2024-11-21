using System.Web;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using uFCoderApi.Repository.Interface;

class Salto:ISalto
{

    readonly char STX = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x02 })[0];
    readonly char ETX = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x03 })[0];
    readonly char cSEP = '³';
    readonly char ACK = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x06 })[0];
    readonly char NAK = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x06 })[0];
    readonly char[] ENQ = System.Text.Encoding.Default.GetChars(new byte[] { 0x05 });


    TcpClient client;

    public async Task<Response> Key_Encode_Binary(string Server_IP, int port, string CardNumber, int Cardtype, string Card_Memory_Sector, string Room_Number, bool isMainKey, DateTime StartDate, DateTime EndDate, string SourceSystem, string Authorisations_granted, string Authorisations_denied)
    {
        try
        {



            string Card_structure = "";

            if (Cardtype == 1) // Mifare
            {
                Card_structure = "1," + Card_Memory_Sector;
            }
            if (Cardtype == 2) // HIDiCLASS
            {
                Card_structure = "2," + Card_Memory_Sector; //5,0,6,0,8,1
            }
            if (Cardtype == 3) // Desfire
            {
                Card_structure = "3," + Card_Memory_Sector; //1,1024,2,256
            }
            if (Cardtype == 4) // TagIt
            {
                Card_structure = "4," + Card_Memory_Sector;// 128
            }
            //future use
            string Second_room = "";               // Second room to be opened by the card.
            string Third_room = "";                // Third room to be opened by the card.
            string Fourth_room = "";               // Fourth room to be opened by the card.
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(1);
            string startDate = StartDate.ToString("hhmmddMMyy"); //Starting date and time of the card.
            string endDate = EndDate.ToString("hhmmddMMyy");  //Expiring date and time of the card.
            string user = "";//SourceSystem == null ? "Mirror" : SourceSystem;                  //Data of the operator who makes the request. Max. 24 characters.
            string information1 = "";              //Information to be written on track #1.
            string information2 = "";              //Information to be written on track #2.
            string information3 = "";              //Information to be written on track #3.

            string Authorisation_code = "";        //Authorisation code assigned to the room guest (max 64 characters)

            string operation = isMainKey ? "CNB" : "CCB";
            if (user.Length > 24)
            {
                user = user.Substring(0, 24);
            }

            // card number reverse 

            if (CardNumber.Length > 0)
            {
                int index = 0;
                StringBuilder sb = new StringBuilder();
                foreach (char c in CardNumber)
                {
                    sb.AppendFormat("{0}{1}", c, (index++ & 1) == 0 ? "" : " ");
                }
                var s = sb.ToString().Trim(':');
                string[] split = s.Trim().Split(' ');
                if (split.Count() > 0)
                {
                    var reversestring = "";
                    foreach (var code in split.Reverse<string>())
                    {
                        reversestring += code;
                    }

                    CardNumber = reversestring;
                }

            }

            string Message = cSEP + operation + cSEP + CardNumber + cSEP + Card_structure + cSEP + Room_Number + cSEP + Second_room + cSEP
                                    + Third_room + cSEP + Fourth_room + cSEP + Authorisations_granted + cSEP + Authorisations_denied
                                    + cSEP + startDate + cSEP + endDate + cSEP + user + cSEP + cSEP + cSEP + cSEP + cSEP + cSEP + ETX;

            char LRC = CalculateLRC(Message);

            Message = STX + Message + LRC;


            client = new TcpClient(Server_IP, port);

            NetworkStream nwStream = client.GetStream();


            var encoding = Encoding.GetEncoding("ISO-8859-1");
            byte[] bytesToSend = encoding.GetBytes(Message);


            nwStream.Write(bytesToSend, 0, bytesToSend.Length);//---send the text---

            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            string temp_response = encoding.GetString(bytesToRead, 0, bytesRead);

            if (!string.IsNullOrEmpty(temp_response))
            {

                char[] char_response = temp_response.ToCharArray();

                if (char_response.Length > 0)
                {

                    if ((char_response[0] == ACK))
                    {


                        if (char_response.Length > 0)
                        {
                            if (char_response[0] == ACK)
                            {
                                if (bytesRead == 1)
                                {
                                    bytesToRead = new byte[client.ReceiveBufferSize];
                                    bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                                    temp_response = encoding.GetString(bytesToRead, 0, bytesRead);
                                }
                                string[] split = temp_response.Split(cSEP);


                                if (split.Length > 0)
                                {
                                    if (split[1] == "CNB" || split[1] == "CCB")
                                    {


                                        return new Response()
                                        {
                                            Status = true,
                                            ResponseMessage = "Success",
                                            CardNumber = split[2],
                                            BinaryImage = split[3],
                                            CardType=Cardtype
                                        };
                                    }
                                    else
                                    {

                                        string err = "";
                                        switch (split[1])
                                        {
                                            case "ES":
                                                err = "Syntax error";
                                                break;
                                            case "NC":
                                                err = "No communication";
                                                break;
                                            case "NF":
                                                err = "No files";
                                                break;
                                            case "OV":
                                                err = "Overflow";
                                                break;
                                            case "EP":
                                                err = "Card error";
                                                break;
                                            case "EF":
                                                err = "Format error";
                                                break;
                                            case "TD":
                                                err = "Unknown room";
                                                break;
                                            case "ED":
                                                err = "Timeout error";
                                                break;
                                            case "EA":
                                                err = "Room is already checkout";
                                                break;
                                            case "OS":
                                                err = "Room is out of service";
                                                break;
                                            case "EO":
                                                err = "Guest card is being encoded by another station";
                                                break;
                                            case "EV":
                                                err = "Card validity error";
                                                break;
                                            case "EG":
                                                err = "General error.";
                                                break;
                                            default:
                                                err = "Undefined error";
                                                break;
                                        }

                                        return new Response()
                                        {
                                            Status = false,
                                            ResponseMessage = err,
                                            CardNumber = ""
                                        };
                                    }
                                }
                            }
                        }
                    }
                    return new Response()
                    {
                        Status = false,
                        ResponseMessage = "Negative ACK"
                    };

                }
                return new Response()
                {
                    Status = false,
                    ResponseMessage = "Response failed from encoder "
                };
            }
            else
            {
                return new Response()
                {
                    Status = false,
                    ResponseMessage = "Response null from encoder "
                };
            }


        }
        catch (Exception ex)
        {
            return new Response()
            {
                Status = false,
                ResponseMessage = ex.ToString()
            };
        }
        finally
        {
            client.Close();
        }
    }


    public static char CalculateLRC(string toEncode)
    {
        //byte[] bytes = Encoding.ASCII.GetBytes(toEncode);
        Char[] split = toEncode.ToCharArray();
        char LRC = split[0];
        for (int i = 1; i < split.Length; i++)
        {
            LRC ^= split[i];
        }
        return LRC;
    }

}

public class Response
{
    public bool Status { get; set; }
    // public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }

    public string CardNumber { get; set; }
    public string BinaryImage { get; set; }
    public int CardType { get; set; }

}