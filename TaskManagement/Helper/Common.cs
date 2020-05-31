using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TaskManagement.Helper
{
    public static class Common
    {

        public static string GetNewTaskID()
        {
            //using (var rng = new RNGCryptoServiceProvider())
            //{
            //    var bit_count = (inputString * 6);
            //    var byte_count = ((bit_count + 7) / 8); // rounded up
            //    var bytes = new byte[byte_count];
            //    rng.GetBytes(bytes);
            //    return Convert.ToBase64String(bytes);
            //}
            return "T1234";
        }
    }
}
