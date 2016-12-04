using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTP_RC
{
    public static class Encryption
    {
        public static string MyXOR(string data, string password)
        {
            string key = password; //key = Password so we can easily switch out Password and hardcode the key, or rewrite it
            string returnValue = ""; //The value that is returned.

            int varY = 0;
            int varX = 0;

            int[] cipher = new int[data.Length];//Creates the cipher equal to data's length
            varX = 0;

            for (varY = 0; varY < data.Length; varY++) //Cycles through cipher and return value.
            {
                returnValue = returnValue + (char)((data[varY] ^ key[varX]));
                cipher[varY] = (data[varY] ^ key[varX]);
                varX++;

                if (varX >= key.Length)
                    varX = 0;
            }
            return returnValue;
        }
    }
}
