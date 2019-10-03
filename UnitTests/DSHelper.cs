using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;

namespace Worker
{
    /// <summary>
    /// This class implement common and general implementation.
    /// </summary>
    internal class DSHelper
    {
        /// <summary>
        /// This method read bytes content from resource
        /// </summary>
        /// <param name="fileName">resource path</param>
        /// <returns>return bytes array</returns>
        internal static byte[] ReadContent(string fileName)
        {
            byte[] buff = null;
            var assembly = Assembly.GetExecutingAssembly();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName);
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    long numBytes = new FileInfo(path).Length;
                    buff = br.ReadBytes((int)numBytes);
                }
            }

            return buff;
        }        
        /// <summary>
        /// This method printing pretty json format
        /// </summary>
        /// <param name="obj">any object to be written as string</param>
        internal static void PrintPrettyJSON(Object obj)
        {
            Console.WriteLine("Result:");
            string json = JsonConvert.SerializeObject(obj);
            string jsonFormatted = JValue.Parse(json).ToString(Formatting.Indented);
            Console.WriteLine(jsonFormatted);
        }
    }
}
