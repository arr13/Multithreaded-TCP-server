using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server
{
    public class XMLSerialization
    {
        public static void Export<T>(List<T> cardList, string aFileName)
        {
            XmlSerializer xmlSerializer =
            new XmlSerializer(typeof(List<T>));
            TextWriter writer = new StreamWriter(aFileName);
            using (writer)
            {
                xmlSerializer.Serialize(writer, cardList);
            }
        }
    }
}
