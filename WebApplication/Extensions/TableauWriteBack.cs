using System;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace WebApplication.Extensions
{
    public class TableauWriteBack
    {

        public bool queryTableauData(string workbook, string dimensionDetail, string dimensionValue, DateTime dimensionDate, string updatedBy)
        {

            SqlConnection myConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            try
            {
                myConnection.Open();

                // Check if a record with the provided parameter already exists
                string query = "SELECT COUNT(*) FROM TableauWriteBack";
                query += " WHERE Workbook = @Workbook AND DimensionDetail = @DimensionDetail AND DimensionValue = @DimensionValue and DimensionDate = @DimensionDate";

                SqlCommand myCommand = new SqlCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@Workbook", workbook);
                myCommand.Parameters.AddWithValue("@DimensionDetail", dimensionDetail);
                myCommand.Parameters.AddWithValue("@DimensionValue", dimensionValue);
                myCommand.Parameters.AddWithValue("@DimensionDate", dimensionDate);
                myCommand.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                myCommand.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                return ((Int32)myCommand.ExecuteScalar()) != 0;
            }
            catch (Exception e)
            {
                Console.Write(e.Message + " " + e.StackTrace);
            }
            
            myConnection.Close();
            return false;
        }

        public void saveTableauData(string workbook, string dimensionDetail, string dimensionValue, DateTime dimensionDate, string updatedBy)
        {

            SqlConnection myConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            try
            {
                myConnection.Open();

                string query = "INSERT INTO TableauWriteBack (Workbook, DimensionDetail, DimensionValue, DimensionDate, LastUpdate,  UpdatedBy)";
                query += " VALUES (@Workbook, @DimensionDetail, @DimensionValue, @DimensionDate, @LastUpdate,  @UpdatedBy)";

                SqlCommand myCommand = new SqlCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@Workbook", workbook);
                myCommand.Parameters.AddWithValue("@DimensionDetail", dimensionDetail);
                myCommand.Parameters.AddWithValue("@DimensionValue", dimensionValue);
                myCommand.Parameters.AddWithValue("@DimensionDate", dimensionDate);
                myCommand.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                myCommand.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.Write(e.Message + " " + e.StackTrace);
            }

            myConnection.Close();
        }

        public void updateTableauData(string workbook, string dimensionDetail, string dimensionValue, DateTime dimensionDate, string updatedBy)
        {

            SqlConnection myConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            try
            {
                myConnection.Open();

                string query = "UPDATE TableauWriteBack";
                query += " SET Workbook = @Workbook, DimensionDetail = @DimensionDetail, DimensionValue = @DimensionValue, DimensionDate = @DimensionDate, LastUpdate = @LastUpdate,  UpdatedBy = @UpdatedBy)";
                query += " WHERE Workbook = @Workbook AND DimensionDetail = @DimensionDetail AND DimensionValue = @DimensionValue and DimensionDate = @DimensionDate";

                SqlCommand myCommand = new SqlCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@Workbook", workbook);
                myCommand.Parameters.AddWithValue("@DimensionDetail", dimensionDetail);
                myCommand.Parameters.AddWithValue("@DimensionValue", dimensionValue);
                myCommand.Parameters.AddWithValue("@DimensionDate", dimensionDate);
                myCommand.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                myCommand.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.Write(e.Message + " " + e.StackTrace);
            }

            myConnection.Close();
        }

        public void deleteTableauData(string workbook, string dimensionDetail, string dimensionValue, DateTime dimensionDate)
        {

            SqlConnection myConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            try
            {
                myConnection.Open();

                string query = "DELETE FROM TableauWriteBack";
                query += " WHERE Workbook = @Workbook AND DimensionDetail = @DimensionDetail AND DimensionValue = @DimensionValue and DimensionDate = @DimensionDate";

                SqlCommand myCommand = new SqlCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@Workbook", workbook);
                myCommand.Parameters.AddWithValue("@DimensionDetail", dimensionDetail);
                myCommand.Parameters.AddWithValue("@DimensionValue", dimensionValue);
                myCommand.Parameters.AddWithValue("@DimensionDate", dimensionDate);

                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.Write(e.Message + " " + e.StackTrace);
            }

            myConnection.Close();
        }

        public void deleteAllTableauData(string workbook)
        {

            SqlConnection myConnection = new SqlConnection(WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            try
            {
                myConnection.Open();

                string query = "DELETE FROM TableauWriteBack";
                query += " WHERE Workbook = @Workbook";

                SqlCommand myCommand = new SqlCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@Workbook", workbook);

                myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.Write(e.Message + " " + e.StackTrace);
            }

            myConnection.Close();
        }

    }
}