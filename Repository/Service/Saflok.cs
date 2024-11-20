using Fare;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using SamsoWebhost.Saflok.Models;
using SamsoWebhost.Saflok.Schema;
using System.Net.Security;
using System.Xml.Serialization;
using uFCoderApi.Repository.Interface;
namespace SamsoWebhost.Saflok
{
    public class Saflok: ISaflok
    {

        public async Task<dynamic> CreateKey(KeyCardRequest cardOperation, string Username, string password, string url)
        {
            System.IO.File.AppendAllLines(@"C:/temp/log.txt", new string[] { DateTime.Now.ToString() + "-" + " Key request  URL:  ", url });

            ServicePointManager.Expect100Continue = true;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //url = "http://sam-tes-1001/MessengerPMSWS.asmx";
            Xeger xeger = new Xeger("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

            string CtimeGUID_str = xeger.Generate();
            string CUserGUID_str = xeger.Generate();
            string CAuthGUID_str = xeger.Generate();
            Envelope envelope = new Envelope();
            envelope.Header = new Schema.Header()
            {

                AuthHeader = new Schema.AuthHeader()
                {
                    MessageID = new MessageID()
                    {
                        Text = CAuthGUID_str,
                    },
                    Security = new Security()
                    {
                        Timestamp = new Timestamp()
                        {
                            Created = DateTime.Now,
                            Expires = DateTime.Now.AddMinutes(4),

                        },
                        UsernameToken = new UsernameToken()
                        {
                            Created = DateTime.Now,
                            Username = Username,
                            Password = password,
                            Id = CUserGUID_str,
                            Xmlns = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"
                        }
                    },
                    From = new From
                    {
                        Text = "urn:KABA"
                    },
                    Action = new Schema.Action { Text = $"{url}/CreateNewBooking" },
                    To = new To() { Text = url },
                    H = "http://tempuri.org",
                    Xmlns = "http://tempuri.org",
                    Xsd = "http://www.w3.org/2001/XMLSchema",
                    Xsi = "http://www.w3.org/2001/XMLSchema-instance"
                }

            };
            envelope.Body = new Body()
            {
                CreateNewBooking = new CreateNewBooking
                {

                    ReservationID = cardOperation.ReservationNo,
                    SiteName = cardOperation.KeyEncoderProperty,
                    PMSTerminalID = cardOperation.TerminalID,
                    CheckIn = cardOperation.CheckinDateTime == null ? DateTime.Now : cardOperation.CheckinDateTime.Value,
                    CheckOut = cardOperation.CheckoutDateTime,
                    GuestName = new GuestName
                    {
                        Text = cardOperation.GuestName,
                    },
                    MainRoomNo = cardOperation.RoomNumber,
                    EncoderID = cardOperation.EncoderID,
                    KeyCount = 1,
                    KeySize = 0,
                    BGrantAccessPredefinedSuiteDoors = false,
                    VariableRoomList = new object(),
                    CommonAreaList = new CommonAreaList()
                    {
                        CCommonAreas = new List<Schema.CCommonAreas> {
                    new Schema.CCommonAreas { PassLevelNo = 1, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 2, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 3, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 4, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 5, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 6, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 7, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 8, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 9, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 10, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 11, EMode = "DefaultConfiguredAccess" },
                    new Schema.CCommonAreas { PassLevelNo = 12, EMode = "DefaultConfiguredAccess" },
                    }
                    }
                }


            };
            envelope.S = "http://schemas.xmlsoap.org/soap/envelope/";
            string xml = Serialize(envelope);


            // System.IO.File.AppendAllLines(@"C:/temp/log.txt", new string[] { DateTime.Now.ToString() + "-" + " Key request :  ",xml });



            XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            using (StringReader reader = new StringReader(xml))
            {
                try
                {
                    var test = (Envelope)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    // System.IO.File.AppendAllLines(@"C:/temp/Error.txt", new string[] { DateTime.Now.ToString() + "-" + " Error in deserialize :  ", ex.ToString() });

                }
            }
            soapEnvelopeDocument.LoadXml(xml);

            ServicePointManager.ServerCertificateValidationCallback = new
RemoteCertificateValidationCallback
(
   delegate { return true; }
);
            string action = "http://tempuri.org/CreateNewBooking";
            HttpWebRequest webRequest = CreateWebRequest(url, action);
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeDocument.Save(stream);
            }

            string soapResponse = "";
            try
            {
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResponse = rd.ReadToEnd();
                    }
                }
                if (soapResponse != null)
                {
                    //System.IO.File.AppendAllLines(@"C:/temp/log.txt", new string[] { DateTime.Now.ToString() + "-" + " Key response :  ", soapResponse });


                    XmlDocument responsexml = new XmlDocument();
                    responsexml.LoadXml(soapResponse);
                    var result = responsexml.LastChild?.LastChild?.LastChild?.LastChild?.InnerText;
                    if (result != null && result.Contains("SUCCESS"))
                    {
                        return new
                        {
                            result = true,
                            message = "SUCCESS"
                        };
                    }
                    else
                    {
                        // System.IO.File.AppendAllLines(@"C:/temp/Error.txt", new string[] { DateTime.Now.ToString() + "-" + " Key response :  ", soapResponse });
                        return new
                        {
                            result = false,
                            message = soapResponse
                        };
                    }

                }
                else
                {
                    return new
                    {
                        result = false,
                        message = "Error in request"
                    };
                }
            }
            catch (WebException ex)
            {
                //System.IO.File.AppendAllLines(@"C:/temp/log.txt", new string[] { DateTime.Now.ToString() + "-" + " Error :  ", ex.ToString() });


                using (System.IO.Stream s = ex.Response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        soapResponse = sr.ReadToEnd();
                    }
                }

                return new
                {
                    result = false,
                    message = "Exception:" + soapResponse
                };
            }

            return soapResponse;
        }

        private static HttpWebRequest CreateWebRequest(string url, string action)
        {
            ServicePointManager.ServerCertificateValidationCallback = new
                RemoteCertificateValidationCallback
                (
                delegate { return true; }
                );

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=utf-8";
            //webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static string Serialize<T>(T obj)
        {
            var writer = new StringWriter();
            Serialize<T>(obj, writer);
            var xml = writer.ToString();
            return xml;
        }

        public static void Serialize<T>(T obj, string filepath)
        {
            var writer = new StreamWriter(filepath);
            Serialize<T>(obj, writer);
        }

        public static void Serialize<T>(T obj, TextWriter writer)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);
            XmlSerializerNamespaces names = new XmlSerializerNamespaces();
            names.Add("", "");
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xmlWriter, obj, names);
        }

        public async Task<dynamic> AdditionalKey(KeyCardRequest cardOperation, string Username, string password, string url)
        {
            ServicePointManager.Expect100Continue = true;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //url = "http://sam-tes-1001/MessengerPMSWS.asmx";
            Xeger xeger = new Xeger("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

            string CtimeGUID_str = xeger.Generate();
            string CUserGUID_str = xeger.Generate();
            string CAuthGUID_str = xeger.Generate();
            Envelope envelope = new Envelope();
            envelope.Header = new Schema.Header()
            {

                AuthHeader = new Schema.AuthHeader()
                {
                    MessageID = new MessageID()
                    {
                        Text = CAuthGUID_str,
                    },
                    Security = new Security()
                    {
                        Timestamp = new Timestamp()
                        {
                            Created = DateTime.Now,
                            Expires = DateTime.Now.AddMinutes(4),

                        },
                        UsernameToken = new UsernameToken()
                        {
                            Created = DateTime.Now,
                            Username = Username,
                            Password = password,
                            Id = CUserGUID_str,
                            Xmlns = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"
                        }
                    },
                    From = new From
                    {
                        Text = "urn:KABA"
                    },
                    Action = new Schema.Action { Text = $"{url}/ChangeKeyAccess" },
                    To = new To() { Text = url },
                    H = "http://tempuri.org",
                    Xmlns = "http://tempuri.org",
                    Xsd = "http://www.w3.org/2001/XMLSchema",
                    Xsi = "http://www.w3.org/2001/XMLSchema-instance"
                }

            };
            envelope.Body = new Body()
            {
                ChangeKeyAccess = new ChangeKeyAccess
                {

                    ReservationID = cardOperation.ReservationNo,
                    SiteName = cardOperation.KeyEncoderProperty,
                    PMSTerminalID = cardOperation.TerminalID,
                    CheckIn = cardOperation.CheckinDateTime == null ? DateTime.Now : cardOperation.CheckinDateTime.Value,
                    CheckOut = cardOperation.CheckoutDateTime,
                    GuestName = new GuestName
                    {
                        Text = cardOperation.GuestName,
                    },
                    MainRoomNo = cardOperation.RoomNumber,
                    EncoderID = cardOperation.EncoderID,
                    KeyCount = 1,
                    KeySize = 0,
                    VariableRoomList = new object(),
                    //BGrantAccessPredefinedSuiteDoors = false,

                }


            };
            envelope.S = "http://schemas.xmlsoap.org/soap/envelope/";
            string xml = Serialize(envelope);
            XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            using (StringReader reader = new StringReader(xml))
            {
                try
                {
                    var test = (Envelope)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {

                }
            }
            soapEnvelopeDocument.LoadXml(xml);

            ServicePointManager.ServerCertificateValidationCallback = new
RemoteCertificateValidationCallback
(
delegate { return true; }
);
            string action = "http://tempuri.org/ChangeKeyAccess";
            HttpWebRequest webRequest = CreateWebRequest(url, action);
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeDocument.Save(stream);
            }

            string soapResponse = "";
            try
            {
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResponse = rd.ReadToEnd();
                    }
                }
                if (soapResponse != null)
                {
                    XmlDocument responsexml = new XmlDocument();
                    responsexml.LoadXml(soapResponse);
                    var result = responsexml.LastChild?.LastChild?.LastChild?.LastChild?.InnerText;
                    if (result != null && result.Contains("SUCCESS"))
                    {
                        return new
                        {
                            result = true,
                            message = "SUCCESS"
                        };
                    }

                }

                return new
                {
                    result = false,
                    message = "Error in request"
                };
            }
            catch (WebException ex)
            {
                using (System.IO.Stream s = ex.Response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        soapResponse = sr.ReadToEnd();
                    }
                }

                return new
                {
                    result = false,
                    message = "Exception:" + soapResponse
                };
            }

            return soapResponse;
        }

    }



}
