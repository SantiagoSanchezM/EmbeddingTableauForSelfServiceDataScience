using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using tableauREST9_v1;
using WebApplication.Extensions;

namespace WebApplication.Extensions
{

    public class DataPaintersExtensions
    {

        // setup the variables
        string tableauServerURL = ConfigurationManager.AppSettings["TableauServer"];
        string tableauServerAPIUser = ConfigurationManager.AppSettings["TableauAdminUser"];
        string tableauServerAPIPassword = ConfigurationManager.AppSettings["TableauAdminPassword"];
        string _tableauServerSite = "default";
        tableau_controller _tc;

        public tableau_controller TableauAPIController
        {
            get { return _tc; }
            set { _tc = value; }
        }

        public DataPaintersExtensions(string tableauServerSite)
        {
            _tc = new tableau_controller(tableauServerURL, tableauServerSite, tableauServerAPIUser, tableauServerAPIPassword);
            _tableauServerSite = tableauServerSite;
        }

        public DataTable getUserWorkbooks(string username)
        {
            return TableauAPIController.tsGetWorkbooksByUser(username);
        }

        public string getWorkbookFirstSheet(string workbookID)
        {
            var views = TableauAPIController.tsGetViewsFromWorkbook(workbookID);
            return views.Rows[0]["name"].ToString();
        }

        public Image getWorkbookImage(string workbookId)
        {
            string tableau_ticket = "";
            string tableau_site = "";
            ArrayList full_token = new ArrayList(3);

            // called the function here
            try
            {
                full_token = TableauAPIController.tsLogin();
            }
            catch (Exception e)
            {
                // TODO Auto-generated catch block
                Console.Write(e.Message + " " + e.StackTrace);
            }

            // get the token
            tableau_ticket = full_token[0].ToString();
            tableau_site = full_token[1].ToString();

            // now send the new header request
            string url = ConfigurationManager.AppSettings["TableauServer"] + "/api/2.0/sites/" + tableau_site + "/workbooks/" + workbookId + "/previewImage";
            WebClient client = new WebClient();

            client.Headers.Add("Content-Type", "text/xml");
            client.Headers.Add("X-Tableau-Auth", tableau_ticket);

            return new Bitmap(new MemoryStream(client.DownloadData(url)));
        }

    }

}