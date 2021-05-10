using OPSCompute;
using PDMHelpers;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace M1Test
{
    public partial class M1TestForm : Form
    {
        private const string OPSCOMPUTE_KEY = "SOFTWARE\\OTS";
        private const long C_RES_OK = 1;
        private const long C_RES_NOK = -1;
        private long m_lRslt = C_RES_OK;

        ITraceable logger;
        ILoggerManager loggerManager;
        OPSComputeM1 computeM1;

        private string EntradaRegistroSeleccionada
        {
            get
            {
                return entradasRegistro.SelectedItem?.ToString() ?? string.Empty;
            }
        }
        private string TextoEntrada
        {
            get
            {
                return textoEntrada.Text;
            }
        }

        public M1TestForm()
        {
            InitializeComponent();
            InitializeComputePerformanceLabel();

            loggerManager = new LoggerManager();
            logger = loggerManager.CreateTracer(this.GetType());
            //computeM1 = new OPSComputeM1(loggerManager);

            computeM1 = OPSComputeM1.GetInstance(loggerManager);

            LoadEntradasRegistroCombo();
            
        }

        private bool IsTextoEntradaValid()
        {
            return (TextoEntrada.Length > 0);
        }
        private bool IsEntradaRegistroSelectionValid()
        {
            return !string.IsNullOrEmpty(EntradaRegistroSeleccionada);
        }

        private void LoadEntradasRegistroCombo()
        {
            RegistryService registryService = new RegistryService();
            entradasRegistro.Items.AddRange(registryService.ReadSubKeys(OPSCOMPUTE_KEY));
        }

        private int Compute(ILoggerManager loggerManager)
        {
            Stopwatch performanceWatcher = new Stopwatch();

            string strInFDDFSA = @"<m1 id=""4"" dst=""4""><m>FDDFSA</m><d>120000131217</d><u>109</u><o>1</o><ad>4</ad><g>60001</g><cdl>1</cdl></m1>";
            string strIn6403CZB = @"<m1 id=""50"" dst=""4""><m>6403CZB</m><g>60002</g><d>200000020217</d><u>111</u><o>1</o><ad>4</ad><cdl>1</cdl></m1>";
            string strIn0123456789 = @"<m1 id=""50"" dst=""4""><m>0123456789</m><g>60001</g><d>203000020217</d><u>107</u><o>1</o><ad>4</ad><cdl>1</cdl></m1>";

            string strMessage = string.Empty;
            string strMessageM50 = string.Empty;

            performanceWatcher.Start();

            int returnValue = 0;
            returnValue = computeM1.fnM1(TextoEntrada, $@"{OPSCOMPUTE_KEY}\{EntradaRegistroSeleccionada}", ref strMessage, ref strMessageM50, 1024, true, false);
            //returnValue = computeM1.fnM1(TextoEntrada, $@"{EntradaRegistroSeleccionada}", ref strMessage, ref strMessageM50, 1024, true, false);

            performanceWatcher.Stop();
            SetComputePerformanceResults(performanceWatcher);
            SetComputePerformanceResultsColor(returnValue);

            if (returnValue == C_RES_OK)
            {
                LoadTreeViewFromMessage(m1Output, strMessage);
                LoadTreeViewFromMessage(m50Output, strMessageM50);
            }
            else {
                MessageBox.Show("Error processing Input message");
            }

            return returnValue;
        }

        private void LoadTreeViewFromMessage(TreeView treeview, string message)
        {
            XmlDocument outputXml = new XmlDocument();
            outputXml.LoadXml(message);

            treeview.Nodes.Clear();
            treeview.Nodes.Add(new TreeNode(outputXml.DocumentElement.Name));
            TreeNode tNode = treeview.Nodes[0];

            AddNode(outputXml.DocumentElement, tNode);

            treeview.ExpandAll();
            treeview.Nodes[0].EnsureVisible();
        }
        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    inTreeNode.Nodes.Add(new TreeNode(xNode.Name));
                    tNode = inTreeNode.Nodes[i];
                    AddNode(xNode, tNode);
                }
            }
            else
            {
                // Here you need to pull the data from the XmlNode based on the
                // type of node, whether attribute values are required, and so forth.
                inTreeNode.Text = (inXmlNode.OuterXml).Trim();
            }
        }



        private void InitializeComputePerformanceLabel() {
            lblState.Text = string.Empty;
        }

        private void SetComputePerformanceResults(Stopwatch watcher)
        {
            lblState.Text = $"Computed in {watcher.Elapsed.ToString()}";
            lblState.ForeColor = System.Drawing.Color.Green;

        }

        private void SetComputePerformanceResultsColor(long result)
        {
            lblState.ForeColor = (C_RES_OK == result) ? System.Drawing.Color.Green : System.Drawing.Color.Red;

        }



        private void btnCompute_Click(object sender, System.EventArgs e)
        {
            if (!IsEntradaRegistroSelectionValid())
            {
                MessageBox.Show("Register entry selection must can't be null");
                return;
            }

            if (!IsTextoEntradaValid())
            {
                MessageBox.Show("Input message must can't be null");
                return;
            }
            
            this.Cursor = Cursors.WaitCursor;

            int returnValue = Compute(loggerManager);

            logger.Write(PDMHelpers.TraceLevel.Info, $"El valor devuelto por la funcion es {returnValue}");
            MessageBox.Show($"El valor devuelto por la funcion es {returnValue}");

            if (returnValue != C_RES_OK)
            {
                MessageBox.Show("Error processing Input message");
            }

            logger.Write(PDMHelpers.TraceLevel.Info, $"==== APP FINISH =====");

            this.Cursor = Cursors.Default;
           
        }

    }
}
