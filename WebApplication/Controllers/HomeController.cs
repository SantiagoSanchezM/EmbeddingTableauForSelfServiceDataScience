using System;
using System.Configuration;
using System.Web.Mvc;
using System.Drawing.Imaging;
using System.Data;
using WebApplication.Extensions;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        
        public string GetTrustedTicket()
        {
            // SSO using Trusted Tickets
            string error = "";
            TrustedAuthenticationExtensions trustedAuthenticationExtension = new TrustedAuthenticationExtensions();
            
            return trustedAuthenticationExtension.GetTableauTicket(ConfigurationManager.AppSettings["TableauServer"], User.Identity.GetUserEmail().Split('@')[0], User.Identity.GetUserEmail().Split('@')[1].Split('.')[0], ref error);
        }


        public ActionResult TableauViz(string vizName)
        {
            Session["vizName"] = vizName;

            if(vizName == "MyOrders/MyOrders")
            {
                return View("InputViz");
            }

            return View();
        }

        public ActionResult MyDataScienceDesk()
        {

            // Dynamicly load the workbooks the user has permissions to see using the REST API
            var restAPI = new DataPaintersExtensions(User.Identity.GetUserEmail().Split('@')[1].Split('.')[0]);
            DataTable userWorkbooks = restAPI.getUserWorkbooks(User.Identity.GetUserEmail().Split('@')[0]);
            ViewBag.Images = new string[userWorkbooks.Rows.Count];
            ViewBag.WorkbookPath = new string[userWorkbooks.Rows.Count];
            ViewBag.WorkbookName = new string[userWorkbooks.Rows.Count];
            int i = 0;

            foreach (DataRow r in userWorkbooks.Rows)
            {
                // Attach a workbook image to the results provided by the API                
                var workbookIcon = restAPI.getWorkbookImage(r["id"].ToString());
                var workbookName = restAPI.getWorkbookFirstSheet(r["id"].ToString());

                using (var streak = new System.IO.MemoryStream())
                {
                    workbookIcon.Save(streak, ImageFormat.Png);
                    string imageBase64Data = System.Convert.ToBase64String(streak.ToArray());
                    string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                    ViewBag.Images[i] = imageDataURL;
                }

                ViewBag.WorkbookPath[i] = r["name"].ToString().Replace(" ", "") + "/" + workbookName.Replace(" ", "");
                ViewBag.WorkbookName[i] = workbookName;

                i++;
            }

            return View();
        }

        public ActionResult MyVizzes()
        {
            
            // Dynamicly load the workbooks the user has permissions to see using the REST API
            var restAPI = new DataPaintersExtensions(User.Identity.GetUserEmail().Split('@')[1].Split('.')[0]);
            DataTable userWorkbooks = restAPI.getUserWorkbooks(User.Identity.GetUserEmail().Split('@')[0]); 
            ViewBag.Images = new string[userWorkbooks.Rows.Count];
            ViewBag.WorkbookPath = new string[userWorkbooks.Rows.Count];
            ViewBag.WorkbookName = new string[userWorkbooks.Rows.Count];
            int i = 0;

            foreach (DataRow r in userWorkbooks.Rows)
            {
                // Attach a workbook image to the results provided by the API                
                var workbookIcon = restAPI.getWorkbookImage(r["id"].ToString());
                var workbookName = restAPI.getWorkbookFirstSheet(r["id"].ToString());

                using (var streak = new System.IO.MemoryStream())
                {
                    workbookIcon.Save(streak, ImageFormat.Png);
                    string imageBase64Data = System.Convert.ToBase64String(streak.ToArray());
                    string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                    ViewBag.Images[i] = imageDataURL;
                }

                ViewBag.WorkbookPath[i] = r["name"].ToString().Replace(" ", "") + "/" + workbookName.Replace(" ", "");
                ViewBag.WorkbookName[i] = workbookName;

                i++;
            }

            return View();
        }

        public JsonResult RunDataScienceModel(string inputArray)
        {
            try
            {
                var inputDataModel = JsonConvert.DeserializeObject<Models.InputModel>(inputArray);

                // Receive the variables from the Web Application
                string userVariables = "\"" + inputDataModel.Items.Find(x => x.ItemName == "bedrooms").Quantity +
                    "\" \"" + inputDataModel.Items.Find(x => x.ItemName == "bathrooms").Quantity +
                    "\" \"" + inputDataModel.Items.Find(x => x.ItemName == "sqftLiving").Quantity +
                    "\" \"" + inputDataModel.Items.Find(x => x.ItemName == "sqftLot").Quantity +
                    "\" \"" + inputDataModel.Items.Find(x => x.ItemName == "zipCode").Quantity;

                // Variables need to include a datetime and a username, to be used for Row Level Security
                userVariables += "\" \"" + DateTime.Now.ToString() + "\" \"" + User.Identity.GetUserEmail().Split('@')[0] + "\"";

                // Call the Model and pass the user variables
                RunPythonCommandLine(ConfigurationManager.AppSettings["YOUR_PY_FILE_PATH"], userVariables);

                return Json("");
            }
            catch (Exception e)
            {
                Response.AddHeader("Error: ", e.Message);
                return Json("There was an error running the prediction. Please try again later.");
            }
        }

        // This method will start a command line in the background and execute a python script
        // cmd indicate the path to the python script, args are an array of user variables
        public string RunPythonCommandLine(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = ConfigurationManager.AppSettings["YOUR_PYTHON_EXE_PATH"];
            start.Arguments = string.Format("\"{0}\" {1}", cmd, args);
            start.UseShellExecute = false;          // Do not use OS shell
            start.CreateNoWindow = true;            // We don't need new window
            start.RedirectStandardOutput = true;    // Any output, generated by application will be redirected back
            start.RedirectStandardError = true;     // Any error in standard output will be redirected back (for example exceptions)
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string stderr = process.StandardError.ReadToEnd();  // Here are the exceptions from our Python script
                    string result = reader.ReadToEnd();                 // Here is the result of StdOut(for example: print "test")
                    return result;
                }
            }
        }

        public JsonResult TableauWriteBack (string inputArray) 
        {
            try
            {
                var inputDataModel = JsonConvert.DeserializeObject<Models.InputModel>(inputArray);
                var inputTime = DateTime.Now;
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    foreach (var item in inputDataModel.Items)
                    {
                        string sql = "INSERT INTO ItemInput(Item, InputDate, Quantity) VALUES(@param1,@param2,@param3)";
                        SqlCommand cmd = new SqlCommand(sql, connection);
                        cmd.Parameters.Add("@param1", SqlDbType.VarChar, 255).Value = item.ItemName;
                        cmd.Parameters.Add("@param2", SqlDbType.DateTime).Value = inputTime;
                        cmd.Parameters.Add("@param3", SqlDbType.Int).Value = item.Quantity;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json("");
            }
            catch (Exception e)
            {
                Response.AddHeader("Error: ", e.Message);
                return Json("");
            }
        }
    }
}