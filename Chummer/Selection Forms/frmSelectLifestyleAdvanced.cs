/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
  using System;
using System.Collections.Generic;
  using System.Collections.Specialized;
using System.Windows.Forms;
using System.Xml;
 ﻿using Chummer.Backend.Equipment;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class frmSelectLifestyleAdvanced : Form
    {
        private readonly Character _objCharacter;

        private readonly XmlDocument _xmlDocument;

        private bool _blnSkipRefresh = true;

        #region Control Events
        public frmSelectLifestyleAdvanced(Character objCharacter, Lifestyle objLifestyle)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            SelectedLifestyle = objLifestyle;
            // Load the Lifestyles information.
            _xmlDocument = XmlManager.Load("lifestyles.xml");
        }

        private void frmSelectLifestyleAdvanced_FormClosing(object sender, FormClosingEventArgs e)
        {
            SelectedLifestyle.LifestyleQualities.CollectionChanged -= LifestyleQualitiesOnCollectionChanged;
            SelectedLifestyle.FreeGrids.CollectionChanged -= FreeGridsOnCollectionChanged;
            Dispose(true);
        }

        private void FreeGridsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                ResetLifestyleQualitiesTree();
                return;
            }
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Move)
                return;

            TreeNode nodFreeGridsRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_FreeMatrixGrids", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.NewItems)
                        {
                            TreeNode objNode = objFreeGrid.CreateTreeNode();
                            if (objNode == null)
                                return;
                            if (nodFreeGridsRoot == null)
                            {
                                nodFreeGridsRoot = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                                    Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids", GlobalOptions.Language)
                                };
                                treLifestyleQualities.Nodes.Add(nodFreeGridsRoot);
                                nodFreeGridsRoot.Expand();
                            }

                            TreeNodeCollection lstParentNodeChildren = nodFreeGridsRoot.Nodes;
                            int intNodesCount = lstParentNodeChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }

                            lstParentNodeChildren.Insert(intTargetIndex, objNode);
                            treLifestyleQualities.SelectedNode = objNode;
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treLifestyleQualities.FindNodeByTag(objFreeGrid);
                            if (objNode != null)
                            {
                                TreeNode objParent = objNode.Parent;
                                objNode.Remove();
                                if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                    objParent.Remove();
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        List<TreeNode> lstOldParents = new List<TreeNode>();
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treLifestyleQualities.FindNodeByTag(objFreeGrid);
                            if (objNode != null)
                            {
                                if (objNode.Parent != null)
                                    lstOldParents.Add(objNode.Parent);
                                objNode.Remove();
                            }
                        }
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.NewItems)
                        {
                            TreeNode objNode = objFreeGrid.CreateTreeNode();
                            if (objNode == null)
                                return;
                            if (nodFreeGridsRoot == null)
                            {
                                nodFreeGridsRoot = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                                    Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids", GlobalOptions.Language)
                                };
                                treLifestyleQualities.Nodes.Add(nodFreeGridsRoot);
                                nodFreeGridsRoot.Expand();
                            }

                            TreeNodeCollection lstParentNodeChildren = nodFreeGridsRoot.Nodes;
                            int intNodesCount = lstParentNodeChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }

                            lstParentNodeChildren.Insert(intTargetIndex, objNode);
                            treLifestyleQualities.SelectedNode = objNode;
                        }
                        foreach (TreeNode objOldParent in lstOldParents)
                        {
                            if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                objOldParent.Remove();
                        }
                        break;
                    }
            }
        }

        private void ResetLifestyleQualitiesTree()
        {
            TreeNode nodPositiveQualityRoot = null;
            TreeNode nodNegativeQualityRoot = null;
            TreeNode nodEntertainmentsRoot = null;
            TreeNode nodFreeGridsRoot = null;

            string strSelectedNode = treLifestyleQualities.SelectedNode?.Tag.ToString();

            treLifestyleQualities.Nodes.Clear();

            foreach (LifestyleQuality objQuality in SelectedLifestyle.LifestyleQualities)
            {
                TreeNode objNode = objQuality.CreateTreeNode();
                if (objNode == null)
                    continue;
                switch (objQuality.Type)
                {
                    case QualityType.Positive:
                        if (nodPositiveQualityRoot == null)
                        {
                            nodPositiveQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_PositiveQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_PositiveQualities", GlobalOptions.Language)
                            };
                            treLifestyleQualities.Nodes.Insert(0, nodPositiveQualityRoot);
                            nodPositiveQualityRoot.Expand();
                        }
                        nodPositiveQualityRoot.Nodes.Add(objNode);
                        break;
                    case QualityType.Negative:
                        if (nodNegativeQualityRoot == null)
                        {
                            nodNegativeQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_NegativeQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_NegativeQualities", GlobalOptions.Language)
                            };
                            treLifestyleQualities.Nodes.Insert(nodPositiveQualityRoot == null ? 0 : 1, nodNegativeQualityRoot);
                            nodNegativeQualityRoot.Expand();
                        }
                        nodNegativeQualityRoot.Nodes.Add(objNode);
                        break;
                    default:
                        if (nodEntertainmentsRoot == null)
                        {
                            nodEntertainmentsRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_Entertainments",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_Entertainments", GlobalOptions.Language)
                            };
                            treLifestyleQualities.Nodes.Insert((nodPositiveQualityRoot == null ? 0 : 1) + (nodNegativeQualityRoot == null ? 0 : 1), nodEntertainmentsRoot);
                            nodEntertainmentsRoot.Expand();
                        }
                        nodEntertainmentsRoot.Nodes.Add(objNode);
                        break;
                }
            }

            foreach (LifestyleQuality objFreeGrid in SelectedLifestyle.FreeGrids)
            {
                TreeNode objNode = objFreeGrid.CreateTreeNode();
                if (objNode == null)
                    return;
                if (nodFreeGridsRoot == null)
                {
                    nodFreeGridsRoot = new TreeNode
                    {
                        Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                        Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids", GlobalOptions.Language)
                    };
                    treLifestyleQualities.Nodes.Add(nodFreeGridsRoot);
                    nodFreeGridsRoot.Expand();
                }
                nodFreeGridsRoot.Nodes.Add(objNode);
            }

            treLifestyleQualities.SortCustomAlphabetically(strSelectedNode);
        }

        private void LifestyleQualitiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                ResetLifestyleQualitiesTree();
                return;
            }
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Move)
                return;

            TreeNode nodPositiveQualityRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_PositiveQualities", false);
            TreeNode nodNegativeQualityRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_NegativeQualities", false);
            TreeNode nodEntertainmentsRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_Entertainments", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.NewItems)
                    {
                        AddToTree(objQuality);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.OldItems)
                    {
                        TreeNode objNode = treLifestyleQualities.FindNodeByTag(objQuality);
                        if (objNode != null)
                        {
                            TreeNode objParent = objNode.Parent;
                            objNode.Remove();
                            if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                objParent.Remove();
                        }
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    List<TreeNode> lstOldParents = new List<TreeNode>();
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.OldItems)
                    {
                        TreeNode objNode = treLifestyleQualities.FindNodeByTag(objQuality);
                        if (objNode != null)
                        {
                            if (objNode.Parent != null)
                                lstOldParents.Add(objNode.Parent);
                            objNode.Remove();
                        }
                    }
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.NewItems)
                    {
                        AddToTree(objQuality);
                    }
                    foreach (TreeNode objOldParent in lstOldParents)
                    {
                        if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                            objOldParent.Remove();
                    }
                    break;
                }
            }

            void AddToTree(LifestyleQuality objQuality)
            {
                TreeNode objNode = objQuality.CreateTreeNode();
                if (objNode == null)
                    return;
                TreeNode objParentNode;
                switch (objQuality.Type)
                {
                    case QualityType.Positive:
                        if (nodPositiveQualityRoot == null)
                        {
                            nodPositiveQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_PositiveQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_PositiveQualities", GlobalOptions.Language)
                            };
                            treLifestyleQualities.Nodes.Insert(0, nodPositiveQualityRoot);
                            nodPositiveQualityRoot.Expand();
                        }
                        objParentNode = nodPositiveQualityRoot;
                        break;
                    case QualityType.Negative:
                        if (nodNegativeQualityRoot == null)
                        {
                            nodNegativeQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_NegativeQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_NegativeQualities", GlobalOptions.Language)
                            };
                            treLifestyleQualities.Nodes.Insert(nodPositiveQualityRoot == null ? 0 : 1, nodNegativeQualityRoot);
                            nodNegativeQualityRoot.Expand();
                        }
                        objParentNode = nodNegativeQualityRoot;
                        break;
                    default:
                        if (nodEntertainmentsRoot == null)
                        {
                            nodEntertainmentsRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_Entertainments",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_Entertainments", GlobalOptions.Language)
                            };
                            treLifestyleQualities.Nodes.Insert((nodPositiveQualityRoot == null ? 0 : 1) + (nodNegativeQualityRoot == null ? 0 : 1), nodEntertainmentsRoot);
                            nodEntertainmentsRoot.Expand();
                        }
                        objParentNode = nodEntertainmentsRoot;
                        break;
                }

                TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                int intNodesCount = lstParentNodeChildren.Count;
                int intTargetIndex = 0;
                for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                {
                    if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                    {
                        break;
                    }
                }

                lstParentNodeChildren.Insert(intTargetIndex, objNode);
                treLifestyleQualities.SelectedNode = objNode;
            }
        }

        private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            using (XmlNodeList xmlLifestyleList = _xmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Options.BookXPath() + "]"))
                if (xmlLifestyleList?.Count > 0)
                    foreach (XmlNode objXmlLifestyle in xmlLifestyleList)
                    {
                        string strLifestyleName = objXmlLifestyle["name"]?.InnerText;

                        if (!string.IsNullOrEmpty(strLifestyleName) &&
                            strLifestyleName != "ID ERROR. Re-add life style to fix" &&
                            (StyleType == LifestyleType.Advanced || objXmlLifestyle["slp"]?.InnerText == "remove") &&
                            !strLifestyleName.Contains("Hospitalized") &&
                            _objCharacter.Options.Books.Contains(objXmlLifestyle["source"]?.InnerText))
                        {
                            lstLifestyles.Add(new ListItem(strLifestyleName, objXmlLifestyle["translate"]?.InnerText ?? strLifestyleName));
                        }
                    }

            chkBonusLPRandomize.DoNegatableDatabinding("Checked",SelectedLifestyle, nameof(Lifestyle.AllowBonusLP));
            nudBonusLP.DoDatabinding("Value", SelectedLifestyle,nameof(Lifestyle.BonusLP));
            ResetLifestyleQualitiesTree();
            cboBaseLifestyle.BeginUpdate();
            cboBaseLifestyle.ValueMember = "Value";
            cboBaseLifestyle.DisplayMember = "Name";
            cboBaseLifestyle.DataSource = lstLifestyles;

            cboBaseLifestyle.SelectedValue = SelectedLifestyle.BaseLifestyle;
            txtLifestyleName.DoDatabinding("Text",SelectedLifestyle,nameof(Lifestyle.Name));
            nudRoommates.DoDatabinding("Value",SelectedLifestyle,nameof(Lifestyle.Roommates));
            nudPercentage.DoDatabinding("Value", SelectedLifestyle, nameof(Lifestyle.Percentage));
            nudArea.DoDatabinding("Value", SelectedLifestyle, nameof(Lifestyle.BindableArea));
            nudComforts.DoDatabinding("Value", SelectedLifestyle, nameof(Lifestyle.BindableComforts));
            nudSecurity.DoDatabinding("Value", SelectedLifestyle, nameof(Lifestyle.BindableSecurity));
            nudArea.DoDatabinding("Maximum", SelectedLifestyle, nameof(Lifestyle.AreaDelta));
            nudComforts.DoDatabinding("Maximum", SelectedLifestyle, nameof(Lifestyle.ComfortsDelta));
            nudSecurity.DoDatabinding("Maximum", SelectedLifestyle, nameof(Lifestyle.SecurityDelta));
            cboBaseLifestyle.DoDatabinding("SelectedValue",SelectedLifestyle,nameof(Lifestyle.BaseLifestyle));
            chkTrustFund.DoDatabinding("Checked", SelectedLifestyle, nameof(Lifestyle.TrustFund));
            chkTrustFund.DoDatabinding("Enabled",SelectedLifestyle,nameof(Lifestyle.IsTrustFundEligible));
            chkPrimaryTenant.DoDatabinding("Checked", SelectedLifestyle, nameof(Lifestyle.PrimaryTenant));
            lblCost.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.DisplayTotalMonthlyCost));
            lblArea.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.FormattedArea));
            lblComforts.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.FormattedComforts));
            lblSecurity.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.FormattedSecurity));
            lblAreaTotal.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.TotalArea));
            lblComfortTotal.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.TotalComforts));
            lblSecurityTotal.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.TotalSecurity));
            lblTotalLP.DoDatabinding("Text", SelectedLifestyle, nameof(Lifestyle.TotalLP));
            if (cboBaseLifestyle.SelectedIndex == -1)
                cboBaseLifestyle.SelectedIndex = 0;
            cboBaseLifestyle.EndUpdate();

            SelectedLifestyle.LifestyleQualities.CollectionChanged += LifestyleQualitiesOnCollectionChanged;
            SelectedLifestyle.FreeGrids.CollectionChanged += FreeGridsOnCollectionChanged;

            _blnSkipRefresh = false;
            RefreshSelectedLifestyle();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkTrustFund_Changed(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (chkTrustFund.Checked)
            {
                nudRoommates.Value = 0;
            }

            nudRoommates.Enabled = !chkTrustFund.Checked;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshSelectedLifestyle();
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (nudRoommates.Value == 0 && !chkPrimaryTenant.Checked)
            {
                chkPrimaryTenant.Checked = true;
            }
        }

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString(), SelectedLifestyle.LifestyleQualities);
                frmSelectLifestyleQuality.ShowDialog(this);

                // Don't do anything else if the form was canceled.
                if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                {
                    frmSelectLifestyleQuality.Close();
                    return;
                }
                blnAddAgain = frmSelectLifestyleQuality.AddAgain;

                XmlNode objXmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

                objQuality.Create(objXmlQuality, SelectedLifestyle, _objCharacter, QualitySource.Selected);
                objQuality.Free = frmSelectLifestyleQuality.FreeCost;
                frmSelectLifestyleQuality.Close();
                //objNode.ContextMenuStrip = cmsQuality;
                if (objQuality.InternalId.IsEmptyGuid())
                    continue;
                
                SelectedLifestyle.LifestyleQualities.Add(objQuality);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treLifestyleQualities.SelectedNode == null || treLifestyleQualities.SelectedNode.Level == 0 || treLifestyleQualities.SelectedNode.Parent.Name == "nodFreeMatrixGrids")
                return;

            if (!(treLifestyleQualities.SelectedNode.Tag is LifestyleQuality objQuality)) return;
            if (objQuality.Name == "Not a Home" && cboBaseLifestyle.SelectedValue?.ToString() == "Bolt Hole")
            {
                return;
            }
            SelectedLifestyle.LifestyleQualities.Remove(objQuality);
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality)
            {
                lblQualityLPLabel.Visible = true;
                lblQualityCostLabel.Visible = true;
                lblQualitySourceLabel.Visible = true;
                chkQualityContributesLP.Visible = true;
                chkQualityContributesLP.Enabled = !(objQuality.Free || objQuality.OriginSource == QualitySource.BuiltIn);

                _blnSkipRefresh = true;
                chkQualityContributesLP.Checked = objQuality.ContributesLP;
                _blnSkipRefresh = false;

                lblQualityLp.Text = objQuality.LP.ToString(GlobalOptions.CultureInfo);
                lblQualityCost.Text = objQuality.Cost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                objQuality.SetSourceDetail(lblSource);
                cmdDeleteQuality.Enabled = !(objQuality.Free || objQuality.OriginSource == QualitySource.BuiltIn);
            }
            else
            {
                lblQualityLPLabel.Visible = false;
                lblQualityCostLabel.Visible = false;
                lblQualitySourceLabel.Visible = false;
                chkQualityContributesLP.Visible = false;
                lblQualityLp.Text = string.Empty;
                lblQualityCost.Text = string.Empty;
                lblQualitySource.Text = string.Empty;
                lblQualitySource.SetToolTip(null);
                cmdDeleteQuality.Enabled = false;
            }
        }

        private void chkQualityContributesLP_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (!(treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality)) return;
            objQuality.ContributesLP = chkQualityContributesLP.Checked;
            lblQualityLp.Text = objQuality.LP.ToString();
        }

        private void chkTravelerBonusLPRandomize_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBonusLPRandomize.Checked)
            {
                nudBonusLP.Enabled = false;
                nudBonusLP.Value = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
            }
            else
            {
                nudBonusLP.Enabled = true;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Lifestyle that was created in the dialogue.
        /// </summary>
        public Lifestyle SelectedLifestyle { get; }

        /// <summary>
        /// Type of Lifestyle to create.
        /// </summary>
        public LifestyleType StyleType { get; set; } = LifestyleType.Advanced;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (SelectedLifestyle.TotalLP < 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectAdvancedLifestyle_OverLPLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_OverLPLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlLifestyle = _xmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
            if (objXmlLifestyle == null)
                return;
            SelectedLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            SelectedLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            SelectedLifestyle.Name = txtLifestyleName.Text;
            SelectedLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"]?.InnerText);
            SelectedLifestyle.Percentage = nudPercentage.Value;
            SelectedLifestyle.BaseLifestyle = strBaseLifestyle;
            SelectedLifestyle.Area = decimal.ToInt32(nudArea.Value);
            SelectedLifestyle.Comforts = decimal.ToInt32(nudComforts.Value);
            SelectedLifestyle.Security = decimal.ToInt32(nudSecurity.Value);
            SelectedLifestyle.TrustFund = chkTrustFund.Checked;
            SelectedLifestyle.Roommates = SelectedLifestyle.TrustFund ? 0 : decimal.ToInt32(nudRoommates.Value);
            SelectedLifestyle.PrimaryTenant = chkPrimaryTenant.Checked;
            SelectedLifestyle.BonusLP = decimal.ToInt32(nudBonusLP.Value);

            // Get the starting Nuyen information.
            SelectedLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText);
            SelectedLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            SelectedLifestyle.StyleType = StyleType;

            DialogResult = DialogResult.OK;
        }

        private void RefreshSelectedLifestyle()
        {
            if (_blnSkipRefresh)
                return;

            string strBaseLifestyle = cboBaseLifestyle.SelectedValue?.ToString() ?? string.Empty;
            SelectedLifestyle.BaseLifestyle = strBaseLifestyle;
            XmlNode xmlAspect = SelectedLifestyle.GetNode();
            if (xmlAspect != null)
            {
                string strSource = xmlAspect["source"]?.InnerText ?? string.Empty;
                string strPage = xmlAspect["altpage"]?.InnerText ?? xmlAspect["page"]?.InnerText ?? string.Empty;
                if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                {
                    string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                    lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
                }
                else
                {
                    lblSource.Text = string.Empty;
                    lblSource.SetToolTip(string.Empty);
                }
            }
            else
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }

            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

            // Characters with the Trust Fund Quality can have the lifestyle discounted.
            if (SelectedLifestyle.IsTrustFundEligible)
            {
                chkTrustFund.Visible = true;
                chkTrustFund.Checked = SelectedLifestyle.TrustFund;
            }
            else
            {
                chkTrustFund.Checked = false;
                chkTrustFund.Visible = false;
            }

            if (SelectedLifestyle.AllowBonusLP)
            {
                lblBonusLP.Visible = true;
                nudBonusLP.Visible = true;
                chkBonusLPRandomize.Visible = true;

                if (chkBonusLPRandomize.Checked)
                {
                    nudBonusLP.Enabled = false;
                    _blnSkipRefresh = true;
                    nudBonusLP.Value = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    _blnSkipRefresh = false;
                }
                else
                {
                    nudBonusLP.Enabled = true;
                }
            }
            else
            {
                lblBonusLP.Visible = false;
                nudBonusLP.Visible = false;
                nudBonusLP.Value = 0;
                chkBonusLPRandomize.Visible = false;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
