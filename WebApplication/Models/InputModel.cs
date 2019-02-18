using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WebApplication.Models
{
    public class InputModel 
    {
        public List <ItemModel> Items { get; set; }
    }

    public class ItemModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
    }

}