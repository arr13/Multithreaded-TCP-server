using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    // class that contains all components of a credit card
    public class CreditCard
    {
        public string Number { get; set; }
        public List<string> CypheredNumbers { get; set; }
        private readonly List<int[]> keys;

        public CreditCard(string number)
        {
            this.Number = number;
            this.CypheredNumbers = new List<string>();

            keys = new List<int[]>();
            keys.Add(new int[] { 4, 3, 1, 2, 5 });
            keys.Add(new int[] { 1, 3, 5, 2, 4 });
            keys.Add(new int[] { 2, 4, 1, 3, 5 });
            keys.Add(new int[] { 5, 2, 1, 4, 3 });
            keys.Add(new int[] { 4, 2, 1, 3, 5 });
            keys.Add(new int[] { 1, 2, 3, 5, 4 });
            keys.Add(new int[] { 4, 5, 3, 2, 1 });
            keys.Add(new int[] { 5, 3, 2, 4, 1 });
            keys.Add(new int[] { 5, 4, 1, 2, 3 });
            keys.Add(new int[] { 5, 4, 3, 2, 1 });
            keys.Add(new int[] { 3, 1, 4, 2, 5 });
            keys.Add(new int[] { 1, 5, 4, 3, 2 });
        }

        public string Encrypt(string number)
        {
            if (this.CypheredNumbers.Count < 12)
            {
                RowTransposition rowTransposition = new RowTransposition(keys[this.CypheredNumbers.Count]);
                string temp = rowTransposition.Encrypt(number);
                this.CypheredNumbers.Add(temp);
                return temp;
            }
            else
            {
                return "cannotEncrypt";
            }
        }

        public static string Decrypt(string cyphered)
        {
            RowTransposition temp = new RowTransposition(new int[] { 4, 3, 1, 2, 5 });
            return temp.Decrypt(cyphered);
        }
    }
}
