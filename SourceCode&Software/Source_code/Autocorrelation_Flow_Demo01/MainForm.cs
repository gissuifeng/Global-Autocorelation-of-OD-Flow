using Autocorrelation_Flow_Demo01.Lib;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Autocorrelation_Flow_Demo01
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void labelControl1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.ClientSize = new System.Drawing.Size(766, 405);
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            //this.ClientSize = new System.Drawing.Size(766, 742);
            this.ClientSize = new System.Drawing.Size(766, 405);

            // OD data file
            string odFile = textEdit_inputDataFile.EditValue.ToString();
            // Spatial relationsip metrix file
            string metrixFile = textEdit_inputMetrixFile.EditValue.ToString();

            List<MetrixObj> metrixList = new List<MetrixObj>();
            List<ODObj> odList = new List<ODObj>();

            double meanValue = 0.0;
            double varianceValue = 0.0;
            double wcValue = 0.0;
            double wValue = 0.0;
            double nValue = 0.0;

            if (File.Exists(metrixFile))
            {
                using (StreamReader sr = new StreamReader(metrixFile))
                {
                    string line = sr.ReadLine();

                    while ((line = sr.ReadLine()) != null) //Read all metrix record and insert into list
                    {
                        int sourceId = Convert.ToInt32(line.Split(',')[0]);
                        int nearId = Convert.ToInt32(line.Split(',')[1]);

                        metrixList.Add(new MetrixObj(sourceId, nearId));
                    }


                    List<int> a1 = new List<int>();
                    foreach (var item in metrixList)  //取出所有OD邻接对中的sourceID并存储到新列表中
                    {
                        a1.Add(item.SourceID);
                    }

                    HashSet<int> hs = new HashSet<int>(a1);

                    foreach (var item in hs)    //取出所有不重复的自身到自身的OD对并存入列表中.
                    {
                        metrixList.Add(new MetrixObj(item, item));
                    }

                }
            }

            if (File.Exists(odFile))
            {
                using (StreamReader sr = new StreamReader(odFile))
                {
                    string line = sr.ReadLine();

                    while ((line = sr.ReadLine()) != null)  //Read all OD record and insert into list(Record_ID，O_ID，D_ID and variable of interest)
                    {
                        int fid = Convert.ToInt32(line.Split(',')[0]);
                        int oid = Convert.ToInt32(line.Split(',')[1]);
                        int did = Convert.ToInt32(line.Split(',')[2]);
                        int val = Convert.ToInt32(line.Split(',')[3]);

                        odList.Add(new ODObj(fid, oid, did, val));
                    }
                }
            }

            //求平均值
            meanValue = MeanVal(odList);
            Console.WriteLine("mean value: " + meanValue);
            //求方差值
            varianceValue = VarianceVal(odList);
            Console.WriteLine("variance value: " + varianceValue);

            double v4 = VarianceVal_Biquadrate(odList);

            double K = v4 / (varianceValue * varianceValue);

            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (var item in odList)
            {
                dic.Add(item.oid + "," + item.did, item.val);
            }

            WCValueSet1 wcset = WC_W_Val(odList, metrixList, meanValue);
            wcValue = wcset.wcValue;
            wValue = wcset.wValue;

            double I = wcValue / varianceValue / wValue;

            nValue = odList.Count;

            double EI = -1 / ((nValue - 1) * 1.0);

            double A = wValue;
            double B = wValue;
            double C = wcset.cValue;

            double V = (Math.Pow(nValue, 2) * B - nValue * C + 3 * Math.Pow(A, 2)) / (Math.Pow(A, 2) * (Math.Pow(nValue, 2) - 1));
            double V1 = (nValue * ((Math.Pow(nValue, 2) + 3 - 3 * nValue) * B + 3 * Math.Pow(A, 2) - nValue * C) - K * ((Math.Pow(nValue, 2) - nValue) * B + 6 * Math.Pow(A, 2) - 2 * nValue * C)) / ((nValue - 1) * (nValue - 2) * (nValue - 3) * Math.Pow(A, 2));
            double SV = Math.Pow(V1, 0.5);

            double Z = (I - EI) / SV;

            //textBox1.Text = Z.ToString();
            //textBox2.Text = I.ToString();
            //textBox3.Text = SV.ToString();
            //textBox4.Text = EI.ToString();

            val_I.Text = I.ToString("0.000000");
            val_Z.Text = Z.ToString("0.000000");
            val_SD.Text = SV.ToString("0.000000");
            val_EI.Text = EI.ToString("0.000000");

            string resultFilePath = textEdit_outputResultFile.EditValue.ToString();

            StreamWriter sw = new StreamWriter(resultFilePath);

            sw.WriteLine("------Result parameter------");
            sw.WriteLine(string.Format(@"Moran's I: {0}", I));
            sw.WriteLine(string.Format(@"Z Score: {0}", Z));
            sw.WriteLine(string.Format(@"Sandard Deviation: {0}", SV));
            sw.WriteLine(string.Format(@"Expected Moran's I: {0}", EI));

            string conString = "Unknow";
            if (Z >= 1.65)
            {
                pictureEdit1.Image = Image.FromFile(string.Format(@"{0}\resources\Clustered.png", Application.StartupPath));
                pictureEdit1.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;

                if (Z >= 1.65 && Z < 1.96)
                {
                    conString = string.Format("Given the z-score of {0}, there is a less than {1}% likelihood that this clustered pattern could be the result of random chance.", Z, 10);
                }
                if (Z >= 1.96 && Z < 2.58)
                {
                    conString = string.Format("Given the z-score of {0}, there is a less than {1}% likelihood that this clustered pattern could be the result of random chance.", Z, 5);
                }
                if (Z >= 2.58)
                {
                    conString = string.Format("Given the z-score of {0}, there is a less than {1}% likelihood that this clustered pattern could be the result of random chance.", Z, 1);
                }
            }
            else if (Z <= -1.65)
            {
                pictureEdit1.Image = Image.FromFile(string.Format(@"{0}\resources\Dispersed.png", Application.StartupPath));
                pictureEdit1.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;

                if (Z <= -1.65 && Z > -1.96)
                {
                    conString = string.Format("Given the z-score of {0}, there is a less than {1}% likelihood that this dispersed pattern could be the result of random chance.", Z, 10);
                }
                if (Z < -1.96 && Z > -2.58)
                {
                    conString = string.Format("Given the z-score of {0}, there is a less than {1}% likelihood that this dispersed pattern could be the result of random chance.", Z, 5);
                }
                if (Z <= 2.58)
                {
                    conString = string.Format("Given the z-score of {0}, there is a less than {1}% likelihood that this dispersed pattern could be the result of random chance.", Z, 1);
                }
            }
            else
            {
                pictureEdit1.Image = Image.FromFile(string.Format(@"{0}\resources\Random.png", Application.StartupPath));
                pictureEdit1.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;

                if (Z <= -1.65 && Z > -1.96)
                {
                    conString = string.Format("Given the z-score of {0}, there be the result of random", Z);
                }
     
            }

            sw.WriteLine(conString);

            sw.WriteLine("------Input parameter------");
            sw.WriteLine(string.Format(@"OD flow data: {0}", odFile));
            sw.WriteLine(string.Format(@"Spatial relationship metrix: {0}", metrixFile));
            sw.WriteLine(string.Format(@"Output message file: {0}", resultFilePath));

            

            sw.Close();

            XtraMessageBox.Show("Analysis task finished successfully!");

            this.ClientSize = new System.Drawing.Size(766, 710);

        }

        public WCValueSet1 WC_W_Val(List<ODObj> odList, List<MetrixObj> metrixList, double meanValue)
        {
            double wsum = 0.0;
            int ss = 0;
            double csum = 0.0;


            foreach (var od in odList)
            {
                double tempCSum = 0.0;

                List<MetrixObj> olist = metrixList.Where(metrix => metrix.SourceID == od.oid).ToList();
                List<MetrixObj> dlist = metrixList.Where(metrix => metrix.SourceID == od.did).ToList();


                foreach (var o in olist)
                {
                    foreach (var d in dlist)
                    {
                        foreach (var od1 in odList)
                        {

                            if (od1.oid == o.NearID && od1.did == d.NearID)
                            {
                                if (!(od.oid == od1.oid && od.did == od1.did))
                                {
                                    Console.WriteLine(od1.oid + ", " + od1.did + ", " + o.NearID + ", " + d.NearID);

                                    wsum += (od.val - meanValue) * (od1.val - meanValue);

                                    Console.WriteLine(string.Format($"({od.val}-{meanValue})*({od1.val}-{meanValue})"));

                                    ss++;

                                    tempCSum++;
                                }
                            }

                        }
                    }


                }

                Console.WriteLine("++++++++++++");
                csum += tempCSum * tempCSum;
                //MessageBox.Show("aa");
            }


            return new WCValueSet1(wsum, ss, csum);
        }

        public double MeanVal(List<ODObj> list)
        {
            int sum = 0;
            int count = list.Count;

            foreach (var item in list)
            {
                sum += item.val;
            }

            return sum / (count * 1.0);
        }

        public double VarianceVal(List<ODObj> list)
        {
            double meanVal = MeanVal(list);
            double count = list.Count;
            double sum = 0.0;

            foreach (var item in list)
            {
                sum += (item.val - meanVal) * (item.val - meanVal);
            }

            return sum / count;
        }

        public double VarianceVal_Biquadrate(List<ODObj> list)
        {
            double meanVal = MeanVal(list);
            double count = list.Count;
            double sum = 0.0;

            foreach (var item in list)
            {
                sum += Math.Pow((item.val - meanVal), 4);
            }

            return sum / count;
        }


        /// Browse input data file
        private void btn_openDataFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = string.Format(@"{0}\data",Application.StartupPath);
            dlg.Filter = "txt files (*.txt)|*.txt|csv files(*.csv)|*.csv";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;
            dlg.Title = "Open data file";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textEdit_inputDataFile.EditValue = dlg.FileName;
            }
        }

        /// File full name for input file of spatial relationship metrxi
        private void btn_openMetrixFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = string.Format(@"{0}\data", Application.StartupPath);
            dlg.Filter = "txt files (*.txt)|*.txt|csv files(*.csv)|*.csv";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;
            dlg.Title = "Open data file of spatial relationship metrix";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textEdit_inputMetrixFile.EditValue = dlg.FileName;
            }
        }

        /// File full name of analysis result
        private void btn_SaveOutputFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = string.Format(@"{0}\data", Application.StartupPath);
            dlg.Filter = "txt files (*.txt)|*.txt";
            dlg.RestoreDirectory = false;
            dlg.Title = "File of analysis result";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textEdit_outputResultFile.EditValue = dlg.FileName;
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
            
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
