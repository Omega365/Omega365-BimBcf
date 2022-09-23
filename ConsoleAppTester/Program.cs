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
            // C:\\Users\\Nicolai.Stensland\\Documents\\BIM_Documents\\o365\\SBIM_BCF-test.bcf
            // C:\\Users\\Nicolai.Stensland\\Documents\\BIM_Documents\\o365\\22-09-02 Fundament bygg nord.bcf
            // C:\\Users\\Nicolai.Stensland\\Documents\\BIM_Documents\\o365\\NYKO_ARK_TEST.bcf

            byte[] fileBytes = File.ReadAllBytes("C:\\Users\\Nicolai.Stensland\\Documents\\BIM_Documents\\o365\\NYKO_ARK_TEST.bcf");
            Stream stream = new MemoryStream(fileBytes);

            var bcf = BCF.Deserialize(stream);

            Console.WriteLine("Done");
            Console.ReadLine();

        }
    }
}
