using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScanHelperMergePDF
{
    public class KdokRodz
    {
        public int IdRodzDok { get; set; }
        public string Opis { get; set; }
        public string Prefix { get; set; }
        public bool Scal { get; set; }  //  czy scalać dokumenty danego rodzaju
        public int Count { get; set; }  //  liczba dokumentów danego typu
    }

    public class KdokRodzCollection : Dictionary<int, KdokRodz>
    {
        public KdokRodzCollection(string fileName)
        {
            List<string> fileLines = File.ReadLines(fileName, Encoding.UTF8).ToList();

            foreach (string line in fileLines)
            {
                string[] lineSplit = line.Split(';', '\t');

                KdokRodz kdokRodz = new KdokRodz
                {
                    IdRodzDok = Convert.ToInt32(lineSplit[0]),
                    Opis = lineSplit[1],
                    Prefix = lineSplit[2],
                    Scal = lineSplit[3] == "tak"
                };

                Add(kdokRodz.IdRodzDok, kdokRodz);
            }
        }
    }
}
