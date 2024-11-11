using System.Web;
using System.Net.Sockets;
using System.Text;

class SaltoHelper
{

    readonly char STX = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x02 })[0];
    readonly char ETX = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x03 })[0];
    readonly char cSEP = '³';
    readonly char ACK = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x06 })[0];
    readonly char NAK = System.Text.Encoding.ASCII.GetChars(new byte[] { 0x06 })[0];
    readonly char[] ENQ = System.Text.Encoding.Default.GetChars(new byte[] { 0x05 });


    TcpClient client;

    public bool getEncoderStatus(string Server_IP, int port)
    {

        try
        {

            //---create a TCPClient object at the IP and port no.---
            client = new TcpClient(Server_IP, port);
            NetworkStream nwStream = client.GetStream();

            byte[] bytesToSend = Encoding.Default.GetBytes(ENQ);

            nwStream.Write(bytesToSend, 0, bytesToSend.Length); //---send the text---
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            string response = Encoding.Default.GetString(bytesToRead, 0, bytesRead);
            char[] char_response = response.ToCharArray();
            client.Close();
            if (char_response[0] == ACK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {

            return false;
        }
        finally
        {
            client.Close();
        }
    }

    public Response Key_Encode(string Server_IP, int port, string encoderID, string Room_Number, bool isMainKey, DateTime StartDate, DateTime EndDate, string SourceSystem, string Authorisations_granted, string Authorisations_denied)
    {
        try
        {

            //future use
            string Second_room = "";               // Second room to be opened by the card.
            string Third_room = "";                // Third room to be opened by the card.
            string Fourth_room = "";               // Fourth room to be opened by the card.
                                                   //string Authorisations_granted = "";    //Authorisations granted to guest
                                                   //string Authorisations_denied = "";     //Authorisations denied to guest.
            string startDate = StartDate.ToString("hhmmddMMyy"); //Starting date and time of the card.
            string endDate = EndDate.ToString("hhmmddMMyy");  //Expiring date and time of the card.
            string user = SourceSystem;                  //Data of the operator who makes the request. Max. 24 characters.
            string information1 = "";              //Information to be written on track #1.
            string information2 = "";              //Information to be written on track #2.
            string information3 = "";              //Information to be written on track #3.
            string card_serial_number = "1";        //Whether the PC interface is to return the card’s serial number or not.
            string Authorisation_code = "";        //Authorisation code assigned to the room guest (max 64 characters)

            string operation = isMainKey ? "CN" : "CC";
            if (user.Length > 24)
            {
                user = user.Substring(0, 24);
            }

            string Message = cSEP + operation + cSEP + encoderID + cSEP + "E" + cSEP + Room_Number + cSEP + Second_room + cSEP
                                    + Third_room + cSEP + Fourth_room + cSEP + Authorisations_granted + cSEP + Authorisations_denied
                                    + cSEP + startDate + cSEP + endDate + cSEP + user +
                                    +cSEP + information1 + cSEP + information2 + cSEP + information3 + cSEP + card_serial_number + cSEP + Authorisation_code
                                    + cSEP + "" + ETX;


            char LRC = CalculateLRC(Message);

            Message = STX + Message + LRC;

            client = new TcpClient(Server_IP, port);

            NetworkStream nwStream = client.GetStream();

            byte[] bytesToSend = Encoding.Default.GetBytes(Message);

            nwStream.Write(bytesToSend, 0, bytesToSend.Length);//---send the text---

            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);


            string temp_response = Encoding.Default.GetString(bytesToRead, 0, bytesRead);
            char[] char_response = temp_response.ToCharArray();

            if (char_response.Length > 0)
            {
                if ((char_response[0] == ACK))
                {
                    do
                    {
                        byte[] bytesToRead1 = new byte[client.ReceiveBufferSize];
                        int bytesRead1 = nwStream.Read(bytesToRead1, 0, client.ReceiveBufferSize);
                        temp_response = Encoding.Default.GetString(bytesToRead1, 0, bytesRead1);
                        Console.WriteLine(temp_response);
                        if (!string.IsNullOrEmpty(temp_response))
                        {
                            //Console.WriteLine(temp_response);
                            char[] temp_char_response = temp_response.ToCharArray();
                            break;
                        }

                    } while (true);

                    if (char_response.Length > 0)
                    {
                        if (char_response[0] == ACK)
                        {
                            string[] split = temp_response.Split(cSEP);

                            if (split[1] == "CN" || split[1] == "CC")
                            {
                                return new Response()
                                {
                                    Status = true,
                                    ResponseMessage = "Success"
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
                                    ResponseMessage = err
                                };
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

    public Response Key_Encode_Binary(string Server_IP, int port, string CardNumber, int Cardtype, string Card_Memory_Sector, string Room_Number, bool isMainKey, DateTime StartDate, DateTime EndDate, string SourceSystem, string Authorisations_granted, string Authorisations_denied)
    {
        try
        {


            //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"{Server_IP}, {port},{CardNumber},{Room_Number}");
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

            string startDate = StartDate.ToString("hhmmddMMyy"); //Starting date and time of the card.
            string endDate = EndDate.ToString("hhmmddMMyy");  //Expiring date and time of the card.
            string user = SourceSystem; //SourceSystem == null ? "Mirror" : SourceSystem;                  //Data of the operator who makes the request. Max. 24 characters.
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

            // CardNumber = "E0B19B87";

            // CardNumber += "000000";
            // +"000000"


            string Message = cSEP + operation + cSEP + CardNumber + cSEP + Card_structure + cSEP + Room_Number + cSEP + Second_room + cSEP
                                    + Third_room + cSEP + Fourth_room + cSEP + Authorisations_granted + cSEP + Authorisations_denied
                                    + cSEP + startDate + cSEP + endDate + cSEP + user + cSEP + cSEP + cSEP + cSEP + cSEP + cSEP + ETX;


            //information1 + cSEP + information2 
            //+ cSEP + information3 + cSEP + Authorisation_code
            //+ cSEP + "" + ETX;
            //string Message = cSEP + operation + cSEP + CardNumber + cSEP + Card_structure + cSEP + Room_Number + cSEP + Second_room + cSEP
            //                        + Third_room + cSEP + Fourth_room + cSEP + Authorisations_granted + cSEP + Authorisations_denied
            //                        + cSEP + startDate + cSEP + endDate + cSEP + user + cSEP + information1 + cSEP + information2 
            //                        + cSEP + information3 + cSEP + Authorisation_code
            //                        + cSEP + "" + ETX;

            ///System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"Request Message : {Message}\n");

            char LRC = CalculateLRC(Message);

            Message = STX + Message + LRC;

        
        //Message = "STX | CNB | E7E912E0 | 1, 14, 15 | 101 | ETX LRC";


            //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"Request Message with LRC : {Message} \n");

            client = new TcpClient(Server_IP, port);

            NetworkStream nwStream = client.GetStream();

            byte[] bytesToSend = Encoding.Default.GetBytes(Message);

            nwStream.Write(bytesToSend, 0, bytesToSend.Length);//---send the text---

            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);


            //string temp_response = "³CNB³8F45A5C4³1,17,0,00C003000048EF48FF3FFCFFFFB710B7,17,1,FF818C001000B049FFFFFFFFFFFFFFFF,17,2,FFCB0EC581894EB4A8D4A278F0FDE5AA,16,0,D1C3006C0040000000000000D7FFFFFF,16,1,0030CF30BF9149BCFC27F30752D922F3,16,2,2BDE34E9E9FA4C8B709AA32D1CFF156A,15,0,9E3C8C71B121B266564DF53ABF02B769,15,1,FDB48A0AFFFFFFFFFFFFFFFFFFFFFFFF,15,2,FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF,14,0,FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF,14,1,FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF,14,2,00000000000000FFFFFFFFFFFFFFFFFF³";
            //string temp_response = "ACK STX|CNB|8F45A5C4| 1,14, 0, 9A9D8F7E56F77DF1CB73B948A7B9C8D2, 14, 1, 8F2378B1CF6D90F61CB37AB61A00BAA1,15, 0, 1BB733A6D9012BD5EE4A905D51A7FF82,15, 1, CC9377A6F812D94667DAA8FB38F823DD,15, 2, 1790261512757657ADBDDD8A8BCFEDDC|ETX LRC";
            string temp_response = Encoding.Default.GetString(bytesToRead, 0, bytesRead);
            //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"Response : {temp_response} \n");


            if (!string.IsNullOrEmpty(temp_response))
            {
                //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"Response not empty \n");
                char[] char_response = temp_response.ToCharArray();

                if (char_response.Length > 0)
                {
                    //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"Response length {char_response.Length} \n");
                    if ((char_response[0] == ACK))
                    {
                        //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"Salto Response ACK \n");

                        if (char_response.Length > 0)
                        {
                            if (char_response[0] == ACK)
                            {
                                if (bytesRead == 1)
                                {
                                    bytesToRead = new byte[client.ReceiveBufferSize];
                                    bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                                    temp_response = Encoding.Default.GetString(bytesToRead, 0, bytesRead);
                                }

                                string[] split = temp_response.Split('|');
                                //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"split the response \n");

                                if (split.Length > 0)
                                {
                                    if (split[1] == "CNB" || split[1] == "CCB")
                                    {
                                        foreach (var item in split)
                                        {
                                            //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"{item} \n");
                                        }

                                        return new Response()
                                        {
                                            Status = true,
                                            ResponseMessage = "Success",
                                            CardNumber = split[2],
                                            BinaryImage = split[3]
                                        };
                                    }
                                    else
                                    {
                                        //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log_Salto.txt"), $"{split[1]} \n");
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
                                            CardNumber = split[2]
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


}