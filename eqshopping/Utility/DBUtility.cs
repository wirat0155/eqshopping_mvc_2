using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Utility
{
    public static class DBUtility
    {
        public static string ChooseDb(string docno)
        {
            string character;
            if (docno.StartsWith("CO"))
            {
                character = docno.Substring(2, 1);
            }
            else
            {
                character = docno.Substring(0, 1);
            }

            return "NM" + character;
        }
    }
}