using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Zegeger.Data;

namespace RefEdit
{
    public partial class Form1 : Form
    {
        XmlDocument refDoc;
        List<XmlNode> XMLReferences;
        Dictionary<string, List<XmlNode>> XMLValues;
        string fileName;
        bool editingXML = false;
        string selectedRef = "";
        string selectedValue = "";

        public Form1()
        {
            string profilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\AgentC\";
            traceLogger.StartUp(profilePath + @"Logs\RefEditLog.txt");
            traceLogger.Write("START");
            XMLReferences = new List<XmlNode>();
            XMLValues = new Dictionary<string, List<XmlNode>>();
            InitializeComponent();
        }

        private void BuildReferences()
        {
            refList.Items.Clear();
            valRef.Items.Clear();
            valRef.Items.Add("");
            foreach(XmlNode node in XMLReferences)
            {
                refList.Items.Add(node.Attributes["name"].Value);
                valRef.Items.Add(node.Attributes["name"].Value);
            }
            if (refList.Items.Contains(selectedRef))
            {
                refList.SelectedItem = selectedRef;
            }
            else
            {
                if (refList.Items.Count > 0)
                    refList.SelectedIndex = 0;
            }
        }

        private void LoadValues(string name)
        {
            valList.Items.Clear();
            foreach (XmlNode node in XMLValues[name])
            {
                valList.Items.Add(node.Attributes["name"].Value);
            }
            if (selectedValue != null)
            {
                if (valList.Items.Contains(selectedValue))
                {
                    valList.SelectedItem = selectedValue;
                }
                else
                {
                    if (valList.Items.Count > 0)
                        valList.SelectedIndex = 0;
                }
            }
            else
            {
                if (valList.Items.Count > 0)
                    valList.SelectedIndex = 0;
            }
        }

        private void ResetValueGUI()
        {
            valName.Text = "";
            valData.Text = "";
            valType.Text = "";
            valRef.Text = "";
            valRefData.Text = "";
            valIsArray.Checked = false;
            ShowValueBasedOnType();
        }

        private void EditValue(string profile, string value)
        {
            foreach (XmlNode node in XMLValues[profile])
            {
                if (node.Attributes["name"].Value == value)
                {
                    valName.Text = node.Attributes["name"].Value;
                    valData.Text = node.InnerXml;
                    valType.Text = "";
                    valRef.Text = "";
                    valIsArray.Checked = false;
                    if (node.Attributes["type"] != null)
                    {
                        valType.Text = node.Attributes["type"].Value;
                    }
                    if (node.Attributes["ref"] != null)
                    {
                         valRef.Text = node.Attributes["ref"].Value;
                    }
                    if (node.Attributes["array"] != null)
                    {
                        valIsArray.Checked = (node.Attributes["array"].Value.ToLower() == "true");
                    }
                    ShowValueBasedOnType();
                    valRef_SelectedValueChanged(null, null);
                    valRefData.Text = node.InnerXml;
                }
            }
        }

        private bool LoadXMLFile(string path)
        {
            fileName = path;
            return LoadXML(System.IO.File.ReadAllText(path));
        }

        private bool LoadXML(string text)
        {
            try
            {
                XmlDocument tmpDoc = new XmlDocument();
                try
                {
                    tmpDoc.LoadXml(text);
                }
                catch (XmlException)
                {
                    StatusText.Text = "Failed to parse XML";
                    return false;
                }
                StatusText.Text = "Loaded XML";
                refDoc = tmpDoc;
                XMLReferences.Clear();
                XMLValues.Clear();
                XmlElement root = refDoc.DocumentElement;
                XmlNode refs = root.FirstChild;
                foreach (XmlNode reference in refs.ChildNodes)
                {
                    if (reference.Name == "Reference")
                    {
                        string refName = reference.Attributes["name"].Value;
                        XMLReferences.Add(reference);
                        List<XmlNode> vals = new List<XmlNode>();
                        foreach (XmlNode value in reference.ChildNodes)
                        {
                            if (value.Name == "Value")
                            {
                                vals.Add(value);
                            }
                        }
                        XMLValues.Add(refName, vals);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
                return false;
            }
            return true;
        }

        private void updateXMLText()
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            refDoc.Save(stream);
            string sXML = Encoding.UTF8.GetString(stream.ToArray());
            XDocument xRefDoc = XDocument.Parse(sXML, LoadOptions.PreserveWhitespace);
            XMLText.Text = xRefDoc.ToString();
        }

        private void EnableRef()
        {
            refList.Enabled = true;
            refAdd.Enabled = true;
            refDelete.Enabled = true;
            refName.Enabled = true;
        }

        private void DisableRef()
        {
            refList.Enabled = false;
            refAdd.Enabled = false;
            refDelete.Enabled = false;
            refName.Enabled = false;
        }

        private void EnableVal()
        {
            valList.Enabled = true;
            valDelete.Enabled = true;
            valNew.Enabled = true;
            valAdd.Enabled = true;
            valDelete.Enabled = true;
            valName.Enabled = true;
            valType.Enabled = true;
            valData.Enabled = true;
            valIsArray.Enabled = true;
            valRefData.Enabled = true;
            ShowValueBasedOnType();
        }

        private void DisableVal()
        {
            valList.Enabled = false;
            valDelete.Enabled = false;
            valNew.Enabled = false;
            valAdd.Enabled = false;
            valDelete.Enabled = false;
            valName.Enabled = false;
            valType.Enabled = false;
            valData.Enabled = false;
            valIsArray.Enabled = false;
            valRefData.Enabled = false;
            valRef.Enabled = false;
        }

        private void ToggleEdit()
        {
            editingXML = !editingXML;
            if (editingXML)
            {
                XMLText.ReadOnly = false;
                EditXML.Hide();
                DoneXML.Show();
                RevertXML.Show();
                DisableVal();
                DisableRef();
            }
            else
            {
                XMLText.ReadOnly = true;
                EditXML.Show();
                DoneXML.Hide();
                RevertXML.Hide();
                EnableRef();
                EnableVal();
            }
        }

        private void ShowValueBasedOnType()
        {
            if (valType.Text == "Alias")
            {
                valRef.Enabled = true;
                valRefData.Show();
                valData.Hide();
                valLblData.Text = "Value Name";
                valIsArray.Checked = false;
                valIsArray.Enabled = false;
            }
            else
            {
                valRef.Enabled = false;
                valRef.Text = "";
                valRefData.Hide();
                valData.Show();
                valLblData.Text = "Value";
                valIsArray.Enabled = true;
            }
        }

        private void UpdateRefValList()
        {
            valRefData.Items.Clear();
            valRefData.Text = "";
            if (XMLValues.ContainsKey(valRef.Text))
            {
                foreach (XmlNode node in XMLValues[valRef.Text])
                    valRefData.Items.Add(node.Attributes["name"].Value);
                if (valRefData.Items.Count > 0)
                    valRefData.SelectedIndex = 0;
            }
        }

        private XmlAttribute CreateAttribute(string name, string value)
        {
            XmlAttribute valNode = refDoc.CreateAttribute(name);
            valNode.Value = value;
            return valNode;
        }

        private void refList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetValueGUI();
            valRef.Items.Clear();
            valRef.Items.Add("");
            selectedRef = refList.SelectedItem as string;
            foreach (XmlNode node in XMLReferences)
            {
                if (node.Attributes["name"].Value != refList.Text)
                {
                    valRef.Items.Add(node.Attributes["name"].Value);
                }
            }
            EnableVal();
            LoadValues(refList.Text);
        }

        private void refAdd_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(refName.Text) && !XMLValues.ContainsKey(refName.Text))
            {
                StatusText.Text = "Adding reference " + refName.Text;
                XmlNode newNode = refDoc.CreateElement("Reference");
                XmlAttribute name = refDoc.CreateAttribute("name");
                name.Value = refName.Text;
                newNode.Attributes.Append(name);
                XMLReferences.Add(newNode);
                refDoc.DocumentElement.FirstChild.AppendChild(newNode);
                XMLValues.Add(refName.Text, new List<XmlNode>());
                BuildReferences();
                refName.Text = "";
                updateXMLText();
            }
            else
            {
                StatusText.Text = "Failed to add reference " + refName.Text;
            }
        }

        private void refDelete_Click(object sender, EventArgs e)
        {
            string nodeName = refList.Text;
            XmlNode nodeToDelete = null;
            foreach (XmlNode node in XMLReferences)
            {
                if (node.Attributes["name"].Value == nodeName)
                {
                    nodeToDelete = node;
                    break;
                }
            }
            if (nodeToDelete != null)
            {
                StatusText.Text = "Deleting reference " + nodeName;
                XMLReferences.Remove(nodeToDelete);
                nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
                BuildReferences();
                updateXMLText();
            }
        }

        private void valList_SelectedValueChanged(object sender, EventArgs e)
        {
            selectedValue = valList.SelectedItem as string;
            EditValue(refList.Text, valList.Text);
        }

        private void valType_SelectedValueChanged(object sender, EventArgs e)
        {
            ShowValueBasedOnType();
        }

        private void valRef_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateRefValList();
        }

        private void valNew_Click(object sender, EventArgs e)
        {
            valList.SelectedItem = null;
            ResetValueGUI();
        }

        private void valDelete_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(valList.Text) && !String.IsNullOrEmpty(refList.Text))
            {
                string nodeName = valList.Text;
                XmlNode nodeToDelete = null;
                foreach (XmlNode node in XMLValues[refList.Text])
                {
                    if (node.Attributes["name"].Value == nodeName)
                    {
                        nodeToDelete = node;
                        break;
                    }
                }
                if (nodeToDelete != null)
                {
                    StatusText.Text = "Deleting value " + nodeName;
                    nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
                    XMLValues[refList.Text].Remove(nodeToDelete);
                    LoadValues(refList.Text);
                    updateXMLText();
                }
            }
            else
            {
                StatusText.Text = "Failed to delete value " + valList.Text;
            }
        }

        private void valAdd_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(refList.Text) && !String.IsNullOrEmpty(valName.Text))
            {
                bool isNewNode = true;
                XmlNode nodeToEdit = null;
                foreach (XmlNode node in XMLValues[refList.Text])
                {
                    if (node.Attributes["name"].Value == valName.Text)
                    {
                        nodeToEdit = node;
                        isNewNode = false;
                        break;
                    }
                }
                if (nodeToEdit == null)
                    nodeToEdit = refDoc.CreateElement("Value");
                nodeToEdit.Attributes.RemoveAll();
                nodeToEdit.Attributes.Append(CreateAttribute("name", valName.Text));
                if (valType.Text == "Alias")
                {
                    nodeToEdit.Attributes.Append(CreateAttribute("type", valType.Text));
                    nodeToEdit.Attributes.Append(CreateAttribute("ref", valRef.Text));
                    nodeToEdit.InnerXml = valRefData.Text;
                }
                else
                {
                    if (!String.IsNullOrEmpty(valType.Text))
                        nodeToEdit.Attributes.Append(CreateAttribute("type", valType.Text));
                    if (valIsArray.Checked)
                        nodeToEdit.Attributes.Append(CreateAttribute("array", "true"));
                    nodeToEdit.InnerXml = valData.Text;
                }
                if (isNewNode)
                {
                    StatusText.Text = "Adding value " + valName.Text;
                    foreach (XmlNode node in XMLReferences)
                    {
                        if (node.Attributes["name"].Value == refList.Text)
                        {
                            node.AppendChild(nodeToEdit);
                        }
                    }
                    XMLValues[refList.Text].Add(nodeToEdit);
                }
                else
                {
                    StatusText.Text = "Updating value " + valName.Text;
                }
                LoadValues(refList.Text);
                updateXMLText();
            }
            else
            {
                StatusText.Text = "Failed to add or update value " + valName.Text;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                openXMLFile(openFileDialog1.FileName);
            }
        }

        private void openXMLFile(string path)
        {
            if (LoadXMLFile(path))
            {
                folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(path);
                updateXMLText();
                BuildReferences();
                EnableRef();
                EditXML.Visible = true;
                saveToolStripMenuItem.Enabled = true;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(fileName))
                refDoc.Save(fileName);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void DoneXML_Click(object sender, EventArgs e)
        {
            if (LoadXML(XMLText.Text))
            {
                updateXMLText();
                BuildReferences();
                ToggleEdit();
            }
        }

        private void EditXML_Click(object sender, EventArgs e)
        {
            StatusText.Text = "Editing XML enabled";
            ToggleEdit();
        }

        private void RevertXML_Click(object sender, EventArgs e)
        {
            StatusText.Text = "Reverting XML";
            updateXMLText();
        }

        private void validateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                folderBrowserDialog1.Description = "Select a folder containing reference xml files";
                DialogResult res = folderBrowserDialog1.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    StatusText.Text = "Validating Reference files...";
                    DataReference RefData = new DataReference(folderBrowserDialog1.SelectedPath);
                    StatusText.Text = "Reference files validated.  View log at " + traceLogger.LogPath;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Validation of Reference files failed!";
                MessageBox.Show(ex.ToString());
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = saveFileDialog1.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                XmlDocument newDoc = new XmlDocument();
                XmlElement root = newDoc.CreateElement("refData");
                newDoc.AppendChild(root);
                XmlElement next = newDoc.CreateElement("References");
                root.AppendChild(next);
                newDoc.Save(saveFileDialog1.FileName);
                openXMLFile(saveFileDialog1.FileName);
            }
        }
    }
}
