using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

namespace ScanHelperMergePDF
{
    public partial class FormMain : Form
    {

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            textBoxInput.Text = Ini.ReadSetting("SelectedInputPath");
            textBoxOutput.Text = Ini.ReadSetting("SelectedOutputPath");

            // jeśli oba foldery istnieją i wynikowy jest pusty
            buttonStart.Enabled = Globals.DirectoriesReady(textBoxInput.Text, textBoxOutput.Text);

            toolStripStatusLabelMain.Text = "Gotowy!";
        }

        private void ButtonInput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbdOpen = new FolderBrowserDialog
            {
                Description = "Wskaż folder z plikami PDF do połączenia",
                ShowNewFolderButton = false,
                RootFolder = Environment.SpecialFolder.MyComputer,
                SelectedPath = Ini.ReadSetting("SelectedInputPath")
            };

            if (fbdOpen.ShowDialog() == DialogResult.OK)
            {
                Ini.SaveSetting("SelectedInputPath", fbdOpen.SelectedPath);

                textBoxInput.Text = fbdOpen.SelectedPath;

                // jeśli oba foldery istnieją i wynikowy jest pusty
                buttonStart.Enabled = Globals.DirectoriesReady(textBoxInput.Text, textBoxOutput.Text);
            }
        }

        private void ButtonOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbdOpen = new FolderBrowserDialog
            {
                Description = "Wskaż folder wynikowy",
                ShowNewFolderButton = true,
                RootFolder = Environment.SpecialFolder.MyComputer,
                SelectedPath = Ini.ReadSetting("SelectedOutputPath")
            };

            if (fbdOpen.ShowDialog() == DialogResult.OK)
            {
                Ini.SaveSetting("SelectedOutputPath", fbdOpen.SelectedPath);

                textBoxOutput.Text = fbdOpen.SelectedPath;

                // jeśli oba foldery istnieją i wynikowy jest pusty
                buttonStart.Enabled = Globals.DirectoriesReady(textBoxInput.Text, textBoxOutput.Text);
            }
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            buttonInput.Enabled = false;
            buttonOutput.Enabled = false;

            backgroundWorkerMerge.RunWorkerAsync();
        }

        private void BackgroundWorkerMerge_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // wczytaj słownik rodzajów dokumentów
            KdokRodzCollection kdokRodzCollection = new KdokRodzCollection("KdokRodz.txt");

            // wybierz wszystkie prefiksy
            List<string> prefixDict = kdokRodzCollection.Values.Select(p => p.Prefix).ToList();

            // wybierz wszystkie katalogi włącznie z podkatalogami
            string[] directories = Directory.GetDirectories(textBoxInput.Text, "*", SearchOption.AllDirectories);

            Array.Sort(directories, new NaturalStringComparer()); 

            int dirCounter = 0;

            foreach (string directory in directories)
            {
                dirCounter++;

                toolStripStatusLabelMain.Text = $"Przetwarzanie folderu: {dirCounter} / {directories.Length} - {directory}";

                string[] fileNames = Directory.GetFiles(directory, "*.pdf", SearchOption.TopDirectoryOnly);

                Array.Sort(fileNames, new NaturalStringComparer()); 

                ScanFileCollection scanFileCollection = new ScanFileCollection();

                for (int i = 0; i < fileNames.Length; i++)
                {
                    ScanFile scanFile = new ScanFile
                    {
                        IdFile = i+1,
                        PathAndFileName = fileNames[i],
                        Path = Path.GetDirectoryName(fileNames[i]),
                        FileName = Path.GetFileName(fileNames[i]),
                        FileExt = Path.GetExtension(fileNames[i]),
                        PdfFile = File.ReadAllBytes(fileNames[i])
                    };

                    scanFile.Prefix = prefixDict.First(p => scanFile.FileName.Like(p));

                    scanFile.PrefixDesc = kdokRodzCollection.Values.Where(p => scanFile.FileName.Like(p.Prefix)).Select(p => p.Opis).First();

                    scanFileCollection.Add(i + 1, scanFile);
                }

                // wybierz ze słownika te wartości, które podlegają połączeniu
                List<string> prefixToMerge = kdokRodzCollection.Values.Where(d => d.Scal).Select(p => p.Prefix).ToList();

                // wybierz rodzaje prefiksów z plików
                List<string> prefixFromFiles = scanFileCollection.Values.Where(s => !string.IsNullOrEmpty(s.Prefix)).Select(p => p.Prefix).Distinct().ToList();

                prefixToMerge = prefixToMerge.Intersect(prefixFromFiles).ToList();

                List<ScanFile> outputPdfFiles = new List<ScanFile>();

                foreach (string prefix in prefixToMerge)
                {
                    ScanFile mergedScanFile = new ScanFile();

                    using MemoryStream outputStream = new MemoryStream();

                    using (PdfDocument outputPdf = new PdfDocument(new PdfWriter(outputStream)))
                    {
                        PdfDocumentInfo info = outputPdf.GetDocumentInfo();

                        List<ScanFile> scanFilesForPrefix = scanFileCollection.Values.Where(s => s.Prefix == prefix)
                            .OrderBy(scan => scan.FileName, new NaturalStringComparer())
                            .ToList();

                        //  przypisanie atrybutów pierwszego pliku do pliku wynikowego
                        mergedScanFile.IdFile = scanFilesForPrefix[0].IdFile;
                        mergedScanFile.FileName = scanFilesForPrefix[0].FileName;
                        mergedScanFile.Path = scanFilesForPrefix[0].Path;
                        mergedScanFile.Prefix = scanFilesForPrefix[0].Prefix;

                        PdfMerger pdfMerger = new PdfMerger(outputPdf);

                        foreach (ScanFile scanFile in scanFilesForPrefix)
                        {
                            scanFileCollection[scanFile.IdFile].Merged = true;

                            using MemoryStream sourceStream = new MemoryStream(scanFile.PdfFile);
                            using PdfDocument inputPdf = new PdfDocument(new PdfReader(sourceStream));

                            pdfMerger.Merge(inputPdf, 1, inputPdf.GetNumberOfPages());
                        }

                        info.SetCreator("GISNET ScanHelper");
                        info.SetKeywords(scanFilesForPrefix[0].PrefixDesc);
                    }

                    mergedScanFile.PdfFile = outputStream.ToArray();

                    outputPdfFiles.Add(mergedScanFile);
                }

                // lista plików, które nie zostały połączone w jeden
                List<ScanFile> scanFilesWithoutMerge = scanFileCollection.Values.Where(scan => scan.Merged == false).ToList();

                foreach (ScanFile scanFile in scanFilesWithoutMerge)
                {
                    outputPdfFiles.Add(scanFile);
                }

                List<ScanFile> outputPdfFilesOrdered = outputPdfFiles.OrderBy(scan => scan.FileName, new NaturalStringComparer()).ToList();


                foreach (ScanFile pdfFile in outputPdfFilesOrdered)
                {
                    int idRodzDok = kdokRodzCollection.Values.Where(p => pdfFile.FileName.Like(p.Prefix)).Select(p => p.IdRodzDok).First();

                    pdfFile.TypeCounter = ++kdokRodzCollection[idRodzDok].Count;
                }


                foreach (ScanFile pdfFile in outputPdfFilesOrdered)
                {
                    string pdfMergeFolder = pdfFile.Path.Replace(textBoxInput.Text, textBoxOutput.Text);

                    Directory.CreateDirectory(pdfMergeFolder);     //  utwórz folder wynikowy

                    string fileName = Path.Combine(pdfMergeFolder, pdfFile.FileName);

                    File.WriteAllBytes(fileName, pdfFile.PdfFile);
                }

                backgroundWorkerMerge.ReportProgress(dirCounter * 100 / directories.Length);               
            }
        }

        private void BackgroundWorkerMerge_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBarMain.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerMerge_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            buttonStart.Enabled = true;
            buttonInput.Enabled = true;
            buttonOutput.Enabled = true;

            toolStripStatusLabelMain.Text = "Koniec przetwarzania!";
        }
    }
}
