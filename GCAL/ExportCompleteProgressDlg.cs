﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

using GCAL.Base;

namespace GCAL
{
    public partial class ExportCompleteProgressDlg : Form
    {
        public ExportCompleteProgressDlg()
        {
            InitializeComponent();
        }

        public bool IsWorking = false;
        public bool CancelRequested = false;

        private void ExportCompleteProgressDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsWorking && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsWorking)
            {
                CancelRequested = true;
                backgroundWorker1.CancelAsync();
                button1.Enabled = false;
            }
            else
            {
                Close();
            }
        }

        public List<TLocation> SelectedLocations = new List<TLocation>();
        public int StartYear = 2020;
        public int EndYear = 2020;
        public string OutputDir = "";
        public bool includeSun = false;
        public bool includeCore = false;

        public void SetData(List<TLocation> locs, int start, int end, string dir, bool isun, bool icore)
        {
            SelectedLocations.AddRange(locs);
            StartYear = start;
            EndYear = end;
            OutputDir = dir;
            includeSun = isun;
            includeCore = icore;
        }

        public int WorkType = 0;

        public void Start(int i)
        {
            WorkType = i;

            if (WorkType == 1)
            {
                // calculate locs/years complete data
                label2.Text = OutputDir;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = SelectedLocations.Count;
                progressBar1.Value = 0;
                progressBar2.Minimum = StartYear;
                progressBar2.Maximum = EndYear;
                progressBar2.Value = StartYear;
                backgroundWorker1.RunWorkerAsync();
                button1.Text = "Cancel";
            }

        }

        private class FileRec
        {
            public string country;
            public string city;
            public int year;
            public string filename;
        }

        public string ToFilePart(string s)
        {
            byte[] array = Encoding.ASCII.GetBytes(s);
            char[] chars = Encoding.ASCII.GetChars(array);
            for (int i = 0; i < chars.Length; i++)
            {
                if (Char.IsLetterOrDigit(chars[i]))
                    chars[i] = Char.ToLower(chars[i]);
                else
                    chars[i] = '_';
            }
            return new String(chars);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            SetProgressValue locdel = new SetProgressValue(SetLocationsProgressValue);
            SetProgressValue yrdel = new SetProgressValue(SetYearsProgressvalue);

            List<FileRec> files = new List<FileRec>();
            HashSet<string> countries = new HashSet<string>();

            try
            {
                GCDisplaySettings.Push();

                GCDisplaySettings.setValue(GCDS.CAL_ARUN_TIME, 0);
                GCDisplaySettings.setValue(GCDS.CAL_ARUN_TITHI, 0);
                GCDisplaySettings.setValue(GCDS.CAL_AYANAMSHA, 0);
                GCDisplaySettings.setValue(GCDS.CAL_BRAHMA_MUHURTA, includeSun ? 1 : 0);
                GCDisplaySettings.setValue(GCDS.CAL_COREEVENTS, includeCore ? 1 : 0);
                GCDisplaySettings.setValue(GCDS.CAL_DST_CHANGE, 1);
                GCDisplaySettings.setValue(GCDS.CAL_EKADASI_PARANA, 1);
                GCDisplaySettings.setValue(GCDS.CAL_FEST_0, 1);
                GCDisplaySettings.setValue(GCDS.CAL_FEST_1, 1);
                GCDisplaySettings.setValue(GCDS.CAL_FEST_2, 1);
                GCDisplaySettings.setValue(GCDS.CAL_FEST_3, 1);
                GCDisplaySettings.setValue(GCDS.CAL_FEST_4, 1);
                GCDisplaySettings.setValue(GCDS.CAL_FEST_5, 1);
                GCDisplaySettings.setValue(GCDS.CAL_FEST_6, 1);
                GCDisplaySettings.setValue(GCDS.CAL_HEADER_MASA, 0);
                GCDisplaySettings.setValue(GCDS.CAL_HEADER_MONTH, 1);
                GCDisplaySettings.setValue(GCDS.CAL_JULIAN, 0);
                GCDisplaySettings.setValue(GCDS.CAL_KSAYA, 0);
                GCDisplaySettings.setValue(GCDS.CAL_MASA_CHANGE, 1);
                GCDisplaySettings.setValue(GCDS.CAL_MOON_LONG, 0);
                GCDisplaySettings.setValue(GCDS.CAL_MOON_RISE, 0);
                GCDisplaySettings.setValue(GCDS.CAL_MOON_SET, 0);
                GCDisplaySettings.setValue(GCDS.CAL_SANKRANTI, 0);
                GCDisplaySettings.setValue(GCDS.CAL_SUN_LONG, 0);
                GCDisplaySettings.setValue(GCDS.CAL_SUN_RISE, includeSun ? 1 : 0);
                GCDisplaySettings.setValue(GCDS.CAL_SUN_SANDHYA, includeSun ? 1 : 0);
                GCDisplaySettings.setValue(GCDS.CAL_VRDDHI, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_ABHIJIT_MUHURTA, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_ASCENDENT, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_CONJUNCTION, 1);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_GULIKALAM, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_MOON, 1);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_MOONRASI, 1);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_NAKSATRA, 1);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_RAHUKALAM, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_SANKRANTI, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_SUN, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_TITHI, 1);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_YAMAGHANTI, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_YOGA, 1);

                for (int locIndex = 0; locIndex < SelectedLocations.Count; locIndex++)
                {
                    TLocation loc = SelectedLocations[locIndex];
                    if (!countries.Contains(loc.Country.Name))
                        countries.Add(loc.Country.Name);
                    progressBar1.Invoke(locdel, locIndex);
                    for (int year = StartYear; year <= EndYear; year++)
                    {
                        if (backgroundWorker1.CancellationPending)
                            return;
                        if (CancelRequested)
                            return;

                        progressBar2.Invoke(yrdel, year);
//                        Thread.Sleep(1000);

                        TResultCalendar calendar = new TResultCalendar();
                        calendar.CalculateCalendar(loc.GetLocationRef(), new GregorianDateTime(year, 1, 1), GregorianDateTime.IsLeapYear(year) ? 366 : 365);
                        string content = calendar.formatText(GCDataFormat.HTML);
                        string filename = year.ToString() + "_" + ToFilePart(loc.CityName) + ".html";
                        File.WriteAllText(Path.Combine(OutputDir, filename), content);
                        files.Add(new FileRec()
                        {
                            filename = filename,
                            city = loc.CityName,
                            country = loc.Country.Name,
                            year = year
                        });
                    }

                    // write location index file
                    if (StartYear != EndYear)
                    {
                        File.WriteAllText(Path.Combine(OutputDir, ToFilePart(loc.CityName) + ".html"), GenerateYearIndex(loc, StartYear, EndYear));
                    }

                }

                for (int year = StartYear; year <= EndYear; year++)
                {
                    // write main index file
                    File.WriteAllText(Path.Combine(OutputDir, "y" + year.ToString() + ".html"), GenerateMainIndex(countries, year, year));
                }

                // write main index file
                File.WriteAllText(Path.Combine(OutputDir, "index.html"), GenerateMainIndex(countries, StartYear, EndYear));

                // write main years file
                //File.WriteAllText(Path.Combine(OutputDir, "years.html"), GenerateYearsOverview(StartYear, EndYear));

                progressBar1.Invoke(new SetProgressValue(SetDialogCompleted), 0);

                GCDisplaySettings.Pop();
            }
            catch(Exception ex)
            {
                Debugger.Log(0, "", "Error: " + ex.Message + "\n");
            }
        }

        public string GenerateMainIndex(HashSet<string> countries, int sy, int ey)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<html><head></title>Vaisnava calendars</title></head>");
            sb.Append("<body>");

            if (sy != ey)
            {
                sb.AppendLine("<h1>All Years</h1>");
                sb.AppendLine("<p>");
                for (int y = sy; y <= ey; y++)
                {
                    if (y > sy)
                        sb.AppendLine(" | ");
                    sb.AppendFormat("<a href=\"{1}\">{0}</a> ", y, "y" + y.ToString() + ".html");
                }
                sb.AppendLine("</p>");
                sb.AppendLine("<hr>");
            }

            foreach (string s in countries)
            {
                sb.AppendLine("<h1>" + s + "</h1>");
                sb.AppendLine("<hr>");
                foreach (TLocation loc in SelectedLocations)
                {
                    if (loc.Country.Name.Equals(s))
                    {
                        if (sy == ey)
                        {
                            sb.AppendFormat("<p><a href=\"{1}\">{0} {2}</a> {3} {4}</p>\n", loc.CityName,
                                sy.ToString() + "_" + ToFilePart(loc.CityName) + ".html", sy,
                                GCEarthData.GetTextLatitude(loc.Latitude), GCEarthData.GetTextLongitude(loc.Longitude));
                        }
                        else
                        {
                            sb.AppendFormat("<p><a href=\"{1}\">{0} {2}-{3}</a> {4} {5}</p>\n",
                                loc.CityName, ToFilePart(loc.CityName) + ".html", sy, ey,
                                GCEarthData.GetTextLatitude(loc.Latitude), GCEarthData.GetTextLongitude(loc.Longitude));
                        }
                    }
                }
            }

            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }

        public string GenerateYearIndex(TLocation loc, int sy, int ey)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<html><head></title>Vaisnava calendars</title></head>");
            sb.Append("<body>");

            sb.AppendLine("<h1>" + loc.CityName + " (" + loc.Country.Name + ")" + "</h1>");
            sb.AppendLine("<hr>");

            for (int y = sy; y <= ey; y++)
            {
                if (y > sy)
                    sb.AppendLine(" | ");
                sb.AppendFormat("<a href=\"{1}\">{0}</a> ", y, y.ToString() + "_" + ToFilePart(loc.CityName) + ".html", sy);

            }

            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }

        public string GenerateYearsOverview(int sy, int ey)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<html><head></title>Vaisnava calendars</title></head>");
            sb.Append("<body>");

            sb.AppendLine("<h1>All Years</h1>");
            sb.AppendLine("<hr>");
            sb.AppendLine("<p>");

            for (int y = sy; y <= ey; y++)
            {
                if (y > sy)
                    sb.AppendLine(" | ");
                sb.AppendFormat("<a href=\"{1}\">{0}</a> ", y, "y" + y.ToString() + ".html");
            }

            sb.AppendLine("</p>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }

        public delegate void SetProgressValue(int val);

        public void SetLocationsProgressValue(int l)
        {
            progressBar1.Value = l;
        }

        public void SetYearsProgressvalue(int i)
        {
            progressBar2.Value = i;
        }

        public void SetDialogCompleted(int i)
        {
            progressBar1.Value = progressBar1.Maximum;
            progressBar2.Visible = false;
            button1.Text = "Exit";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }


    }
}
