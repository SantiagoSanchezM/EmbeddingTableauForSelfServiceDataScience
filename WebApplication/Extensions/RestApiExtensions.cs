using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace MyPlanetFinancial.Extensions
{
    public class RestApiExtensions
    {

        private Exception _apiCallError;
        private Exception _userDefinedFieldError;
        private XmlDocument _parsed_response;
        private string _token;
        private string _site_id;
        private string _user_id;

        public Exception ApiCallError
        {
            get { return _apiCallError; }
            set { _apiCallError = value; }
        }

        public Exception UserDefinedFieldError
        {
            get { return _userDefinedFieldError; }
            set { _userDefinedFieldError = value; }
        }

        public XmlDocument Parsed_response
        {
            get { return _parsed_response; }
            set { _parsed_response = value; }
        }

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public string Site_Id
        {
            get { return _site_id; }
            set { _site_id = value; }
        }

        public string User_Id
        {
            get { return _user_id; }
            set { _user_id = value; }
        }

        public RestApiExtensions()
        {
            Parsed_response = new XmlDocument();
        }

        public void Check_Status(HttpWebResponse server_response, HttpStatusCode success_code)
        {
            if (server_response.StatusCode != success_code)
            {
                StreamReader inStream = new StreamReader(server_response.GetResponseStream(), new ASCIIEncoding());
                string resString = inStream.ReadToEnd();
                inStream.Close();
                Parsed_response.LoadXml(resString);

                //Create namespace manager
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(Parsed_response.NameTable);
                nsmgr.AddNamespace("t", "http://tableau.com/api");

                // Obtain the 3 xml tags from the response: error, summary, and detail tags
                XmlNodeList errorElement = Parsed_response.SelectNodes("t:error", nsmgr);
                XmlNodeList summaryElement = Parsed_response.SelectNodes(".//t:summary", nsmgr);
                XmlNodeList detailElement = Parsed_response.SelectNodes(".//t:detail", nsmgr);

                // Retrieve the error code, summary, and detail if the response contains them
                string error = errorElement[0].Attributes.GetNamedItem("code").InnerText;
                string summary = summaryElement[0].InnerText;
                string detail = detailElement[0].InnerText;

                ApiCallError = new Exception(String.Format("{0}: {1} - {2}", error, summary, detail));
                throw ApiCallError;
            }
        }

        public void Sign_In(string server, string username, string password, string site = "")
        {
            try
            {
                string url = server + "/api/2.4/auth/signin";

                // Create the XML that will go with the POST Request
                XmlDocument xmlRequest = new XmlDocument();
                XmlElement requestXML = xmlRequest.CreateElement("tsRequest");

                XmlElement credentialsXML = xmlRequest.CreateElement("credentials");
                credentialsXML.SetAttribute("name", username);
                credentialsXML.SetAttribute("password", password);

                XmlElement siteXML = xmlRequest.CreateElement("site");
                siteXML.SetAttribute("contentUrl", site);

                credentialsXML.AppendChild(siteXML);
                requestXML.AppendChild(credentialsXML);
                xmlRequest.AppendChild(requestXML);

                // Create the request
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/xml";
                request.Accept = "application/xml";

                byte[] data = Encoding.UTF8.GetBytes(xmlRequest.OuterXml.ToString());
                request.ContentLength = data.Length;

                // Write the request
                Stream outStream = request.GetRequestStream();
                outStream.Write(data, 0, data.Length);
                outStream.Close();

                // Do the request to get the response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Check_Status(response, HttpStatusCode.OK);

                StreamReader inStream = new StreamReader(response.GetResponseStream(), new ASCIIEncoding());
                string resString = inStream.ReadToEnd();
                inStream.Close();
                Parsed_response.LoadXml(resString);

                //Create namespace manager
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(Parsed_response.NameTable);
                nsmgr.AddNamespace("t", "http://tableau.com/api");

                // Obtain the 3 xml tags from the response: error, summary, and detail tags
                XmlNodeList tokenElement = Parsed_response.SelectNodes(".//t:credentials", nsmgr);
                XmlNodeList siteElement = Parsed_response.SelectNodes(".//t:site", nsmgr);
                XmlNodeList userElement = Parsed_response.SelectNodes(".//t:user", nsmgr);

                // Retrieve the error code, summary, and detail if the response contains them
                Token = tokenElement[0].Attributes.GetNamedItem("token").InnerText;
                Site_Id = siteElement[0].Attributes.GetNamedItem("id").InnerText;
                User_Id = userElement[0].Attributes.GetNamedItem("id").InnerText;

            }
            catch (Exception e)
            {
                // TODO: Log unexpected error
                
            }
        }

        public void Sign_Out(string server, string auth_token)
        {
            try
            {
                string url = server + "/api/2.4/auth/signout";

                ASCIIEncoding enc = new ASCIIEncoding();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                
                // Create the request
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("x-tableau-auth", Token);

                // Do the request to get the response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Check_Status(response, HttpStatusCode.NoContent);
                
            }
            catch (Exception e)
            {
                // TODO: Log unexpected error
            }
        }

    }
}