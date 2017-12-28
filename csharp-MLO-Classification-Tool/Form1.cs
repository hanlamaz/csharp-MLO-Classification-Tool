using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using weka.classifiers;
using weka.classifiers.evaluation;
using weka.core;
using weka.core.neighboursearch;

namespace csharp_MLO_Classification_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        weka.core.Instances insts;
        weka.core.Instances instsTest;
        public string testLoc;
        List<trainedData> myData = new List<trainedData>();
        Instance ins;
        const int percentSplit = 66;
        private async Task loadFileAndMakeElements(string location)
        {
            if (location.EndsWith(".csv"))
            {
                weka.core.converters.CSVLoader csvLoader = new weka.core.converters.CSVLoader();
                csvLoader.setSource(new java.io.File(location));
                insts = csvLoader.getDataSet();
                insts.setClassIndex(insts.numAttributes() - 1);
            }
            else
            {
                insts = new weka.core.Instances(new java.io.FileReader(location));
                insts.setClassIndex(insts.numAttributes() - 1);
            }
            flowLayoutPanel1.Controls.Clear();
            for (int i = 0; i < insts.numAttributes() - 1; i++)
            {
                if (insts.attribute(i).isNominal() == true)
                {
                    if (insts.attribute(i).numValues() > 0)
                    {
                        Label lbl = new Label();
                        lbl.Text = insts.attribute(i).name().Trim();
                        lbl.Enabled = true;
                        ComboBox cmbBox = new ComboBox();
                        cmbBox.Name = insts.attribute(i).name();
                        for (int m = 0; m < insts.attribute(i).numValues(); m++)
                        {
                            cmbBox.Items.Add(insts.attribute(i).value(m));
                        }
                        cmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
                        cmbBox.Enabled = true;
                        flowLayoutPanel1.Controls.Add(lbl);
                        flowLayoutPanel1.Controls.Add(cmbBox);
                    }
                    else
                    {

                    }

                }
                else if (insts.attribute(i).isNumeric() == true)
                {
                    Label lbl = new Label();
                    lbl.Text = insts.attribute(i).name().Trim();
                    TextBox txtBox = new TextBox();
                    txtBox.Name = insts.attribute(i).name();
                    txtBox.KeyPress += new KeyPressEventHandler(txtBox_Keypress);
                    flowLayoutPanel1.Controls.Add(lbl);
                    flowLayoutPanel1.Controls.Add(txtBox);
                }
            }
        }
        private void txtBox_Keypress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        public async Task<string> classifyTest(weka.classifiers.Classifier cl)
        {
            string a = "";
            double rate = 0;
            try
            {
                //instsTest = Instances.mergeInstances(ins,null);
                
                /*if (ins.classIndex() == -1)
                    ins.setClassIndex(insts.numAttributes() - 1);*/

                System.Console.WriteLine("Performing " + percentSplit + "% split evaluation.");

                weka.filters.Filter normalized = new weka.filters.unsupervised.attribute.Normalize();
                normalized.setInputFormat(insts);
                insts = weka.filters.Filter.useFilter(insts, normalized);

                //randomize the order of the instances in the dataset.
                weka.filters.Filter myRandom = new weka.filters.unsupervised.instance.Randomize();
                myRandom.setInputFormat(insts);
                insts = weka.filters.Filter.useFilter(insts, myRandom);

                //replace missing values
                weka.filters.Filter replaceMissingValues = new weka.filters.unsupervised.attribute.ReplaceMissingValues();
                replaceMissingValues.setInputFormat(insts);
                insts = weka.filters.Filter.useFilter(insts, replaceMissingValues);


                int trainSize = insts.numInstances() * percentSplit / 100;
                int testSize = insts.numInstances() - trainSize;
                weka.core.Instances train = new weka.core.Instances(insts, 0, trainSize);

                cl.buildClassifier(train);

               //double label = cl.classifyInstance(instsTest.instance(0));
                double label = cl.classifyInstance(ins);
                ins.setClassValue(label);
                //instsTest.instance(0).setClassValue(label);
                a = ins.toString(ins.numAttributes()-1);

                weka.core.SerializationHelper.write("mymodel.model", cl);
                int numCorrect = 0;
                for (int i = trainSize; i < insts.numInstances(); i++)
                {
                    weka.core.Instance currentInst = insts.instance(i);
                    double predictedClass = cl.classifyInstance(currentInst);
                    if (predictedClass == insts.instance(i).classValue())
                        numCorrect++;
                }

                rate = (double)((double)numCorrect / (double)testSize * 100.0);

            }
            catch (java.lang.Exception ex)
            {
                //ex.printStackTrace();
                rate = -1;
            }
            return rate.ToString() + ";" + a??"";
        }

        private async Task makeTests()
        {
            string infoStr = "";
            Dictionary<string, string> rateDic = new Dictionary<string, string>(); // A dictionary that contains algorithm and its result.
            await Task.Delay(50);// For show file location in textbox
            rateDic.Add("J48", await classifyTest(new weka.classifiers.trees.J48()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Naive Bayes", await classifyTest(new weka.classifiers.bayes.NaiveBayes()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("1-IBk", await classifyTest(new weka.classifiers.lazy.IBk(1)));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("3-IBk", await classifyTest(new weka.classifiers.lazy.IBk(3)));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("5-IBk", await classifyTest(new weka.classifiers.lazy.IBk(5)));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("7-IBk", await classifyTest(new weka.classifiers.lazy.IBk(7)));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("9-IBk", await classifyTest(new weka.classifiers.lazy.IBk(9)));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Random Forest", await classifyTest(new weka.classifiers.trees.RandomForest()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Random Tree", await classifyTest(new weka.classifiers.trees.RandomTree()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("REPTree", await classifyTest(new weka.classifiers.trees.REPTree()));
            await Task.Delay(50);
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            rateDic.Add("LMT", await classifyTest(new weka.classifiers.trees.LMT()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Multilayer Perceptron", await classifyTest(new weka.classifiers.functions.MultilayerPerceptron()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("SMO", await classifyTest(new weka.classifiers.functions.SMO()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Logistic Regression", await classifyTest(new weka.classifiers.functions.Logistic()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Linear Regression", await classifyTest(new weka.classifiers.functions.LinearRegression()));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            var ordered = rateDic.OrderByDescending(x => double.Parse(x.Value.Split(';')[0]));
            label2.Text = "STATUS = Working on  ORDERING";
            await Task.Delay(50);
            int counter = 1;
            foreach (var item in ordered)
            {
                if (item.Value.Split(';')[0].ToString() == "-1")
                {
                    infoStr += counter + ") " + item.Key + " is unavaible for this" + System.Environment.NewLine;
                    counter++;
                }
                else
                {
                    infoStr += counter + ") " + item.Key + " with %" + item.Value.Split(';')[0] + " success rate , and Test Result is : " + item.Value.Split(';')[1] + System.Environment.NewLine;
                    counter++;
                }
            }
            label2.Text = "STATUS = FINISHED";
            await Task.Delay(50);
            label1.Text = infoStr;
            label1.Visible = true;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "";     
            OpenFileDialog file = new OpenFileDialog();
            file.Title = "Open CSV or ARFF";
            file.InitialDirectory = "C:\\";
            file.Filter = "Attribute-Relation File Format |*.arff|Comma-separated values |*.csv";
            if (file.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = file.FileName;
                await Task.Delay(50);
            }
            button3.Enabled = true;
            await loadFileAndMakeElements(file.FileName);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Dock = DockStyle.Fill;
            button3.Enabled = false;
        }


        public async Task makeNewInstance()
        {
            int a = 0;
            ins = new DenseInstance(insts.numAttributes());
            ins.setDataset(insts);
            foreach (trainedData item in myData)
            {
                if (item.isNumber==false)
                {
                    weka.core.Attribute m = insts.attribute(item.name);
                    ins.setValue(m, item.value);
                    a++;
                }
                else
                {

                }
            }
        }
        public async Task getInsertedElements()
        {
            List<String> values = new List<String>();
            foreach (Control c in flowLayoutPanel1.Controls.OfType<Control>())
                if (string.IsNullOrEmpty(c.Name) && string.IsNullOrEmpty(c.Text))
                {
                    MessageBox.Show("Please fill all info");
                    break;
                }
                else
                {
                    if (string.IsNullOrEmpty(c.Text))
                    {
                        values.Add(c.Name);
                    }
                    else
                    {
                        var a = (c as ComboBox);
                        if (a!=null)
                        {
                            values.Add((c as ComboBox).SelectedItem.ToString());
                        }
                        else
                        {
                            values.Add(c.Text);
                        }
                        
                    }
                    
                }
            for (int i = 0; i < values.Count; i += 2)
            {
                trainedData trn = new trainedData();
                double price;
                bool isDouble = Double.TryParse(values[i + 1], out price);
                if (isDouble)
                {
                    trn.name = values[i];
                    trn.value = values[i + 1];
                    trn.isNumber = true;
                }
                else
                {
                    trn.name = values[i];
                    trn.value = values[i + 1];
                    trn.isNumber = false;
                }
                myData.Add(trn);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            label2.Text = "STATUS = STARTED";
            await Task.Delay(50);
            await getInsertedElements();
            await Task.Delay(50);
            await makeNewInstance();
            await Task.Delay(50);
            await makeTests();
        }
        public class trainedData
        {
            public string name { get; set; }
            public string value { get; set; }
            public bool isNumber { get; set; }
        }
    }
}
