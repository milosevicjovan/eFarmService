using eFarmDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eFarmService.Models
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int ProducerId { get; set; }
        public string Producer { get; set; }

        //public Producer Producer { get; set; }

        /* 
        
        REMOVED WHEN MULTIPLE DEVICES PER PRODUCER WERE ADDED
        
        public int DeviceId { get; set; }
        public string DeviceType { get; set; }
        
        */
    }
}