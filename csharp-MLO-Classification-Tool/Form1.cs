using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csharp_MLO_Classification_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        const int percentSplit = 66;
        public async Task<double> classifyTest(weka.classifiers.Classifier cl, string file)
        {
            
            double rate = 0;
            try
            {
                weka.core.Instances insts;

                if (file.EndsWith(".csv"))
                {
                    weka.core.converters.CSVLoader csvLoader = new weka.core.converters.CSVLoader();
                    csvLoader.setSource(new java.io.File(file));
                    insts = csvLoader.getDataSet();
                    insts.setClassIndex(insts.numAttributes() - 1);
                }
                else
                {
                    insts = new weka.core.Instances(new java.io.FileReader(file));
                    insts.setClassIndex(insts.numAttributes() - 1);
                }


                Console.WriteLine("Performing " + percentSplit + "% split evaluation.");

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
                ex.printStackTrace();
                rate = -1;
            }

            return rate;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string infoStr = "";
            label1.Text = "";
            Dictionary<string, double> rateDic = new Dictionary<string, double>(); // A dictionary that contains algorithm and its result.
            OpenFileDialog file = new OpenFileDialog();
            file.Title = "Open CSV or ARFF";
            file.InitialDirectory = "C:\\";
            file.Filter = "Attribute-Relation File Format |*.arff|Comma-separated values |*.csv";
            if (file.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = file.FileName;
                label2.Text = "STATUS = STARTED";
                await Task.Delay(50);
            }
            await Task.Delay(50);// For show file location in textbox
            rateDic.Add("J48", await classifyTest(new weka.classifiers.trees.J48(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Naive Bayes", await classifyTest(new weka.classifiers.bayes.NaiveBayes(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("1-IBk", await classifyTest(new weka.classifiers.lazy.IBk(1), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("3-IBk", await classifyTest(new weka.classifiers.lazy.IBk(3), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("5-IBk", await classifyTest(new weka.classifiers.lazy.IBk(5), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("7-IBk", await classifyTest(new weka.classifiers.lazy.IBk(7), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("9-IBk", await classifyTest(new weka.classifiers.lazy.IBk(9), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Random Forest", await classifyTest(new weka.classifiers.trees.RandomForest(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Random Tree", await classifyTest(new weka.classifiers.trees.RandomTree(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("REPTree", await classifyTest(new weka.classifiers.trees.REPTree(), file.FileName));
            await Task.Delay(50);
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            rateDic.Add("LMT", await classifyTest(new weka.classifiers.trees.LMT(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Multilayer Perceptron", await classifyTest(new weka.classifiers.functions.MultilayerPerceptron(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("SMO", await classifyTest(new weka.classifiers.functions.SMO(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Logistic Regression", await classifyTest(new weka.classifiers.functions.Logistic(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            rateDic.Add("Linear Regression", await classifyTest(new weka.classifiers.functions.LinearRegression(), file.FileName));
            label2.Text = "STATUS = Working on " + rateDic.Keys.ElementAt(rateDic.Keys.Count() - 1);
            await Task.Delay(50);
            var ordered = rateDic.OrderByDescending(x => x.Value);
            label2.Text = "STATUS = Working on  ORDERING";
            await Task.Delay(50);
            int counter = 1;
            foreach (var item in ordered)
            {
                if (item.Value == -1)
                {
                    infoStr += counter + ") " + item.Key + " is unavaible for this" + Environment.NewLine;
                    counter++;
                }
                else
                {
                    infoStr += counter + ") " + item.Key + " with %" + item.Value + " success rate" + Environment.NewLine;
                    counter++;
                }
            }
            label2.Text = "STATUS = FINISHED";
            await Task.Delay(50);
            label1.Text = infoStr;
            label1.Visible = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Dock = DockStyle.Fill;
        }
    }
}
