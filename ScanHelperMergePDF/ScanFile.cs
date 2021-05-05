using System.Collections.Generic;

namespace ScanHelperMergePDF
{
    public class ScanFile
    {
        public int IdFile { get; set; }
        public string PathAndFileName { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileExt { get; set; }
        public string Prefix { get; set; }
        public string PrefixDesc { get; set; }
        public int TypeCounter { get; set; }
        public bool Merged { get; set; }
        public byte[] PdfFile { get; set; }
    }

    public class ScanFileCollection : Dictionary<int, ScanFile>
    {

    }
}
