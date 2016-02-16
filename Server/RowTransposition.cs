using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RowTransposition
    {
        readonly int[] key;

        public RowTransposition(int[] key)
        {
            this.key = key;
        }
        
        // encrypt credit card
        public string Encrypt(string plainText)
        {
            int columns = 0, rows = 0;
            Dictionary<int, int> rowsPositions = FillPositionsDictionary(plainText, ref columns, ref rows);
            char[,] matrix2 = new char[rows, columns];

            //Fill Matrix
            int charPosition = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (charPosition < plainText.Length)
                    {
                        matrix2[i, j] = plainText[charPosition];
                    }
                    else
                    {
                        matrix2[i, j] = '*';
                    }

                    charPosition++;
                }
            }

            string result = "";

            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    result += matrix2[j, rowsPositions[i + 1]];
                }

                result += " ";
            }

            return result;
        }

        // decrypt credit card
        public string Decrypt(string cipherText)
        {
            int columns = 0, rows = 0;
            Dictionary<int, int> rowsPositions = FillPositionsDictionary(cipherText, ref columns, ref rows);
            char[,] matrix = new char[rows, columns];

            //Fill Matrix
            int charPosition = 0;
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    matrix[j, rowsPositions[i + 1]] = cipherText[charPosition];
                    charPosition++;
                }
            }

            string result = "";

            foreach (char c in matrix)
            {
                if (c != '*' && c != ' ')
                {
                    result += c;
                }
            }

            return result;
        }

        private Dictionary<int, int> FillPositionsDictionary(string token, ref int columns, ref int rows)
        {
            var result = new Dictionary<int, int>();
            columns = key.Length;
            rows = (int)Math.Ceiling((double)token.Length / (double)columns);
            
            for (int i = 0; i < columns; i++)
            {
                result.Add(key[i], i);
            }

            return result;
        }
    }
}
