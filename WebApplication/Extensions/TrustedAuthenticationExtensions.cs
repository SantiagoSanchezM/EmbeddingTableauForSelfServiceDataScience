using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;


namespace WebApplication.Extensions
{
    public class TrustedAuthenticationExtensions
    {

        private String _tabserver = "";
        private String _tabpath = "";
        private Boolean _tabssl = false;


        public String TabServer
        {
            get { return _tabserver; }
            set { _tabserver = value; }
        }


        public String TabPath
        {
            get { return _tabpath; }
            set { _tabpath = value; }
        }


        public Boolean TabSSL
        {
            get { return (Boolean)_tabssl; }
            set { _tabssl = (Boolean)value; }
        }


        // this function does the heavy lifting of validating a user
        public string GetTableauTicket(string tabserver, string tabuser, string tabsite, ref string errMsg)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            // the client_ip parameter isn't necessary to send in the POST unless you have
            // wgserver.extended_trusted_ip_checking enabled (it's disabled by default)
            // string postData = "username=" + tabuser + "&client_ip=" + Page.Request.UserHostAddress;

            string postData = "username=" + tabuser + "&target_site=" + tabsite;
            byte[] data = enc.GetBytes(postData);

            try
            {
                //string http = _tabssl ? "https://" : "http://";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["TableauServer"] + "/trusted");

                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = data.Length;

                // Write the request
                Stream outStream = req.GetRequestStream();
                outStream.Write(data, 0, data.Length);
                outStream.Close();

                // Do the request to get the response
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                StreamReader inStream = new StreamReader(res.GetResponseStream(), enc);
                string resString = inStream.ReadToEnd();
                inStream.Close();

                return resString;
            }
            // if anything bad happens, copy the error string out and return a "-1" to indicate failure
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return "-1";
            }
        }

    }
}