using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TaskManagement.Repositories.TaskDetailsData;

namespace TaskManagement.Helper
{
    public class Common
    {
        private readonly IConfiguration _configuration;

        public Common(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GetNewTaskID()
        {
            //TaskDataRepository taskDataRepository = new TaskDataRepository(_configuration);
            //var lastCreatedID = taskDataRepository.GetLastCreatedTaskID();
            
            Random r = new Random();
            string NewID = "T" + r.Next(1000, 2000);
            return NewID;
            //using (var rng = new RNGCryptoServiceProvider())
            //{
            //    var bit_count = (inputString * 6);
            //    var byte_count = ((bit_count + 7) / 8); // rounded up
            //    var bytes = new byte[byte_count];
            //    rng.GetBytes(bytes);
            //    return Convert.ToBase64String(bytes);
            //}
        }


    }
}
