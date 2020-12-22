using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using eFarmDataAccess;
using eFarmService.Models;
using System.Data.Entity;
using System.Web.Http.Description;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace eFarmService
{
    /*
        This class provides method for basic authentication that we use for authorization device's requests
    */
    public class eFarmSecurity
    {

        public static bool Login(string username, string password)                                                 
        {
            using (DeviceDataEntities entities = new DeviceDataEntities())
            {
                return entities.Users.Any(user => user.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)
                                            && user.PasswordHash == password);
            }
        }
    }
}