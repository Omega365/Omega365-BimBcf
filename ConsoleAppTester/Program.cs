using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.BCF;
using System.IO;

namespace ConsoleAppTester
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] fileBytes = File.ReadAllBytes("C:\\Users\\mathias.skavhaug\\Desktop\\filer\\bcf\\JFL_BCF_med_to_nivå_Tag.bcf");
            Stream stream = new MemoryStream(fileBytes);

            var bcf = BCF.Deserialize(stream);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
