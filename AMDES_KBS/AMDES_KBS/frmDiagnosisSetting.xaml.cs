﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AMDES_KBS.Controllers;
using AMDES_KBS.Entity;


namespace AMDES_KBS
{
    /// <summary>
    /// Interaction logic for frmDiagnosisSetting.xaml
    /// </summary>
    public partial class frmDiagnosisSetting : Page
    {

        int currIdx = -1;
        List<NaviChildCritAttribute> naviChildAttribute;

        public frmDiagnosisSetting()
        {
            InitializeComponent();
            loadALLDiagnosis();
            loadALLQuestionGroup();
            lstGroupList.ItemsSource = null;
            lstGroupList.ItemsSource = new List<QuestionGroup>();
        }

        private void loadALLQuestionGroup()
        {
            cboGroupList.ItemsSource = null;
            cboGroupList.ItemsSource = QuestionController.getAllQuestionGroup();
            cboGroupList.SelectedIndex = 0;
        }

        private void loadALLDiagnosis()
        {
            lstGroupList.ItemsSource = null;
            lstGroupList.ItemsSource = new List<QuestionGroup>();
            lstAttributeList.ItemsSource = null;
            naviChildAttribute = new List<NaviChildCritAttribute>();
            reloadAttribute(naviChildAttribute);
            lstDiagnosisList.ItemsSource = DiagnosisController.getAllDiagnosis();
            lstDiagnosisList.SelectedIndex = currIdx;

        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            Diagnosis newDiagnosis = new Diagnosis();
            newDiagnosis.RID = DiagnosisController.getNextDiagnosisID();
            newDiagnosis.Header = txtHeader.Text.Trim();
            newDiagnosis.Comment = txtComment.Text.Replace(Environment.NewLine, "~~");
            newDiagnosis.LinkDesc = txtLinkDesc.Text.Trim();
            newDiagnosis.Link = txtLink.Text.Trim();
            newDiagnosis.RetrieveSym = chkSym.IsChecked.Value;
            newDiagnosis.RetrievalIDList = getSymptonsForDiagnosis();

            if (chkHavAttribute.IsChecked==true)
            {
                foreach (NaviChildCritAttribute attr in naviChildAttribute)
                {
                    CmpAttribute cAttr = new CmpAttribute(attr.AttributeName, attr.getAttributeTypeENUM(), int.Parse(attr.AttributeValue));
                    newDiagnosis.addAttribute(cAttr);
                }                
            }

            if (!SaveDiagnosis(newDiagnosis))
            {
                return;
            }
            currIdx = lstDiagnosisList.Items.Count;
            loadALLDiagnosis();
        }

        private void lstDiagnosisList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sidx = lstDiagnosisList.SelectedIndex;
            if (sidx == -1)
            {
                txtHeader.Text = "";
                txtComment.Text = "";
                txtLink.Text = "";



                return;
            }

            cboGroupList.SelectedIndex = -1;
            lstGroupList.ItemsSource = null;
            lstSymptomsList.ItemsSource = null;

            Diagnosis sDiagnosis = (Diagnosis)lstDiagnosisList.Items.GetItemAt(sidx);
            loadData(sDiagnosis);
            currIdx = sidx;
        }

        private void loadData(Diagnosis d)
        {
            txtHeader.Text = d.Header;
            txtComment.Text = d.Comment.Replace("~~", Environment.NewLine);
           
            chkSym.IsChecked = d.RetrieveSym;
            chkLink.IsChecked = d.Link.Length > 0;

            naviChildAttribute = new List<NaviChildCritAttribute>();

            foreach (CmpAttribute cAttr in d.getAttributes())
            {
                NaviChildCritAttribute attr = new NaviChildCritAttribute();
                attr.AttributeName = cAttr.Key;
                attr.AttributeValue = cAttr.Value.ToString();
                attr.setRuleType(cAttr.getCompareType());

                naviChildAttribute.Add(attr);
            }

            if (naviChildAttribute.Count > 0)
            {
                chkHavAttribute.IsChecked = true;
            }
            else
            {
                chkHavAttribute.IsChecked = false;
            }

            reloadAttribute(naviChildAttribute);

            if (chkLink.IsChecked == true)
            {
                txtLink.Text = d.Link;
                txtLinkDesc.Text = d.LinkDesc;
                chkLink_Checked(this, new RoutedEventArgs());
            }
            else
            {
                txtLink.Text = "";
                txtLinkDesc.Text = "";
                chkLink_Unchecked(this, new RoutedEventArgs());
            }

            if (chkSym.IsChecked == true)
                chkSym_Checked(this, new RoutedEventArgs());
            else
                chkSym_Unchecked(this, new RoutedEventArgs());

            chkRes.IsChecked = d.IsResource;

            lstGroupList.ItemsSource = null;
            List<QuestionGroup> qgGrpList = new List<QuestionGroup>();

            if (chkSym.IsChecked==false)
            {
                stkpnlSymtomsSection.Visibility = Visibility.Collapsed;
            }
            else
            {
                stkpnlSymtomsSection.Visibility = Visibility.Visible;
                for (int i = 0; i < d.RetrievalIDList.Count; i++)
                {
                    qgGrpList.Add(QuestionController.getGroupByID(d.RetrievalIDList[i]));
                }
                //chkAge.IsChecked = d.AgeBelow65;
            }

            lstGroupList.ItemsSource = qgGrpList;
        }

        private void loadSymtpomQuestionGroup(Diagnosis d)
        {
            lstGroupList.Items.Clear();
            List<QuestionGroup> currQGLst = new List<QuestionGroup>();
            foreach (QuestionGroup qgItem in currQGLst)
            {
                lstGroupList.Items.Add(qgItem);
            }

        }

        private void btnDeleteComment_Click(object sender, RoutedEventArgs e)
        {
            int sidx = lstDiagnosisList.SelectedIndex;
            if (sidx == -1)
            {
                return;
            }
            Diagnosis sDiagnosis = (Diagnosis)lstDiagnosisList.Items.GetItemAt(sidx);
            DiagnosisController.deleteDiagnosis(sDiagnosis.RID);
            currIdx = -1;
            loadALLDiagnosis();
        }

        private void btnSaveComment_Click(object sender, RoutedEventArgs e)
        {
            int sidx = lstDiagnosisList.SelectedIndex;
            if (sidx == -1)
            {
                return;
            }
            Diagnosis sDiagnosis = (Diagnosis)lstDiagnosisList.Items.GetItemAt(sidx);
            sDiagnosis.Header = txtHeader.Text.Trim();
            sDiagnosis.Comment = txtComment.Text.Replace(Environment.NewLine, "~~");
            if (chkLink.IsChecked == true)
            {
                sDiagnosis.Link = txtLink.Text.Trim();
                sDiagnosis.LinkDesc = txtLinkDesc.Text.Trim();
            }
            else
            {
                sDiagnosis.Link = "";
                sDiagnosis.LinkDesc = "";
            }

            
            sDiagnosis.RetrieveSym = chkSym.IsChecked.Value;
            sDiagnosis.RetrievalIDList.Clear();
            if (sDiagnosis.RetrieveSym)
            {
                sDiagnosis.RetrievalIDList = getSymptonsForDiagnosis();
            }

            //sDiagnosis.AgeBelow65 = chkAge.IsChecked.Value;
            
            sDiagnosis.IsResource = chkRes.IsChecked.Value;

            sDiagnosis.getAttributes().Clear();            
            if (chkHavAttribute.IsChecked == true)
            {
                foreach (NaviChildCritAttribute attr in naviChildAttribute)
                {
                    CmpAttribute cAttr = new CmpAttribute(attr.AttributeName, attr.getAttributeTypeENUM(), int.Parse(attr.AttributeValue));
                    sDiagnosis.addAttribute(cAttr);
                }
            }

            if (!SaveDiagnosis(sDiagnosis))
            {
                return;
            }

            currIdx = sidx;
            loadALLDiagnosis();

        }

        private List<int> getSymptonsForDiagnosis()
        {
            List<QuestionGroup> currQGlist = (List<QuestionGroup>)lstGroupList.ItemsSource;
            List<int> z = new List<int>();

            if (currQGlist != null)
            {
                foreach (QuestionGroup g in currQGlist)
                {
                    z.Add(g.GroupID);
                }
            }

            return z;
        }

        private bool SaveDiagnosis(Diagnosis d)
        {
            if (d.Header.Trim().Length == 0)
            {
                MessageBox.Show("Please Key In Header!");
                txtHeader.Focus();
                return false;
            }

            if (d.RetrieveSym == false && d.Comment.Trim().Length == 0)
            {
                MessageBox.Show("Please Key In Comment!");
                txtComment.Focus();
                return false;
            }

            if (!IsValidUrl(d.Link.Trim()) && d.Link.Trim().Length > 0)
            {
                MessageBox.Show("Please Key In Valid URL!");
                txtLink.Text = "";
                txtLink.Focus();
                return false;
            }

            DiagnosisController.updateDiagnosis(d);
            MessageBox.Show("Diagnosis Saved!");
            return true;
        }

        private bool IsValidUrl(string urlString)
        {
            if (Regex.IsMatch(urlString, @"(http://|https://)?(www\.)?[A-Za-z0-9]+\.[a-z]{2,3}"))
            {
                return true;
            }

            return false;
            //Uri uri;
            //return Uri.TryCreate(urlString, UriKind.Absolute, out uri);
            //&& //(uri.Scheme == Uri.UriSchemeHttp
            //|| uri.Scheme == Uri.UriSchemeHttps
            //|| uri.Scheme == Uri.UriSchemeFtp
            //|| uri.Scheme == Uri.UriSchemeMailto
            ///*...*/);
        }

        private void btnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            int idx = cboGroupList.SelectedIndex;
            if (idx == -1)
            {
                return;
            }

            QuestionGroup qg = (QuestionGroup)cboGroupList.Items[idx];
            if (addSymptonsFromGroup(qg))
                AddSymptomtoDiagnosis(qg);
        }

        private bool addSymptonsFromGroup(QuestionGroup qg)
        {
            List<QuestionGroup> lstQG = (List<QuestionGroup>)lstGroupList.ItemsSource;

            if (lstQG == null)
            {
                return true;
            }

            foreach (QuestionGroup qgItem in lstQG)
            {
                if (qgItem.GroupID == qg.GroupID)
                {
                    return false;
                }
            }
            return true;
        }

        private void AddSymptomtoDiagnosis(QuestionGroup qg)
        {
            List<QuestionGroup> lstQG = (List<QuestionGroup>)lstGroupList.ItemsSource;
            if (lstQG == null)
            {
                lstQG = new List<QuestionGroup>();
            }

            lstQG.Add(qg);
            lstGroupList.ItemsSource = null;
            lstGroupList.ItemsSource = lstQG;
        }

        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            List<QuestionGroup> lstQG = (List<QuestionGroup>)lstGroupList.ItemsSource;
            int idx = lstGroupList.SelectedIndex;
            if (idx == -1)
            {
                return;
            }
            lstQG.RemoveAt(idx);
            lstGroupList.ItemsSource = null;
            lstGroupList.ItemsSource = lstQG;
        }

        private void cboGroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<String> SymptomsList = new List<String>();
            int idx = cboGroupList.SelectedIndex;
            if (idx == -1)
                return;
            QuestionGroup qg = (QuestionGroup)cboGroupList.Items[idx];
            if (qg.Symptom.Trim() != "")
            {
                SymptomsList.Add(qg.Symptom.Trim());
            }

            foreach (Question q in qg.Questions)
            {
                String symptom = q.Symptom.Trim();
                if (symptom.Length > 0)
                {
                    if (!SymptomsList.Contains(symptom))
                    {
                        SymptomsList.Add(symptom);
                    }
                }
            }

            lstSymptomsList.ItemsSource = null;
            lstSymptomsList.ItemsSource = SymptomsList;
        }

        private void lstGroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = lstGroupList.SelectedIndex;
            if (idx == -1)
            {
                return;
            }

            QuestionGroup selectedQG = (QuestionGroup)lstGroupList.Items[idx];
            for (int i = 0; i < cboGroupList.Items.Count; i++)
            {
                QuestionGroup qg = (QuestionGroup)cboGroupList.Items[i];
                if (qg.GroupID == selectedQG.GroupID)
                {
                    cboGroupList.SelectedIndex = i;
                    break;
                }
            }
        }

        private void chkSym_Checked(object sender, RoutedEventArgs e)
        {
            stkpnlSymtomsSection.Visibility = Visibility.Visible;
        }

        private void chkSym_Unchecked(object sender, RoutedEventArgs e)
        {
            stkpnlSymtomsSection.Visibility = Visibility.Collapsed;
        }

        private void chkLink_Checked(object sender, RoutedEventArgs e)
        {
            stkpnllinkDesc.Visibility = stkpnllinkURL.Visibility = Visibility.Visible;
        }

        private void chkLink_Unchecked(object sender, RoutedEventArgs e)
        {
            stkpnllinkDesc.Visibility = stkpnllinkURL.Visibility = Visibility.Collapsed;
        }

        //Hidden Function only to us
        private void btnImportSpecial_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog OD = new Microsoft.Win32.OpenFileDialog();
            List<String> DiagnosisList = new List<String>();
            if (OD.ShowDialog() == true)
            {
                string fileName = OD.FileName;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
                {
                    while (sr.Peek() >= 0)
                    {
                        String diagString = sr.ReadLine().Trim();
                        DiagnosisList.Add(diagString);
                    }
                }
            }

            int counter;
            try
            {
                counter = int.Parse(txtHeader.Text.ToString().Trim());
            }
            catch (Exception)
            {
                counter = 1;
            }


            foreach (String diagnosisStr in DiagnosisList)
            {
                Diagnosis d = new Diagnosis();
                d.RID = DiagnosisController.getNextDiagnosisID();
                d.Header = "R" + String.Format("{0:00}", counter);
                d.Comment = diagnosisStr;
                d.Link = "";
                DiagnosisController.updateDiagnosis(d);
                counter++;
            }

            this.loadALLDiagnosis();
        }

        private void txtHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtHeader.Text.ToString().Trim() == "@Enable Special Function")
            {
                btnImportSpecial.Visibility = Visibility.Visible;
            }
            else if (txtHeader.Text.ToString().Trim() == "@Disable Special Function")
            {
                btnImportSpecial.Visibility = Visibility.Collapsed;
            }
            else
            {

            }
        }

        private void btnOtherAttr_Click(object sender, RoutedEventArgs e)
        {
            frmAttributeAddingToPath cAttribute = new frmAttributeAddingToPath(naviChildAttribute);

            if (cAttribute.ShowDialog() == true)
            {
                naviChildAttribute = cAttribute.getOtherAttribute();
                reloadAttribute(naviChildAttribute);
            }
        }

        private void reloadAttribute(List<NaviChildCritAttribute> otherAttr)
        {
            lstAttributeList.Items.Clear();
            foreach (NaviChildCritAttribute attr in otherAttr)
            {
                string s = "=";
                if (attr.getAttributeTypeENUM() == AttributeCmpType.LessThanEqual)
                {
                    s = "<=";
                }
                else if (attr.getAttributeTypeENUM() == AttributeCmpType.LessThan)
                {
                    s = "<";
                }
                else if (attr.getAttributeTypeENUM() == AttributeCmpType.MoreThanEqual)
                {
                    s = ">=";
                }
                else if (attr.getAttributeTypeENUM() == AttributeCmpType.MoreThan)
                {
                    s = ">";
                }

                if (!App.isAttrCompareNumerical(attr.AttributeName))
                {
                    PatAttribute attrCat = PatAttributeController.searchPatientAttribute(attr.AttributeName);
                    lstAttributeList.Items.Add(attr.AttributeName + " " + s + " " + attrCat.CategoricalVals[int.Parse(attr.AttributeValue)]);
                }
                else
                {
                    lstAttributeList.Items.Add(attr.AttributeName + " " + s + " " + attr.AttributeValue);
                }
            }
        }

        private void chkHavAttribute_Checked(object sender, RoutedEventArgs e)
        {
            stkpnlAttribute.Visibility = Visibility.Visible;
        }

        private void chkHavAttribute_Unchecked(object sender, RoutedEventArgs e)
        {
            stkpnlAttribute.Visibility = Visibility.Collapsed;
        }

    }
}
