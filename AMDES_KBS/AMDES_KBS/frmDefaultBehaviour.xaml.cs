﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AMDES_KBS.Entity;
using AMDES_KBS.Controllers;

namespace AMDES_KBS
{
    /// <summary>
    /// Interaction logic for frmDefaultBehaviour.xaml
    /// </summary>
    public partial class frmDefaultBehaviour : Window
    {
        Navigation navi;

        List<NaviChildCriteriaQuestion> naviChildQuestion;
        List<NaviChildCritAttribute> naviChildAttribute;
        List<Navigation> AllBehaviour;
        public frmDefaultBehaviour()
        {
            InitializeComponent();
            newForm();
            loadQuestionGroup();
         //   loadAllBehaviour();
        }

        private void loadAllBehaviour()
        {
            cboDiagnosisList.Items.Clear();
            AllBehaviour = DefaultBehaviorController.getAllDefaultBehavior();
            for (int i = 0; i < AllBehaviour.Count; i++)
            {
                Navigation behaviour = AllBehaviour[i];
                bool conclusive = behaviour.isConclusive();
                string s="";
                s = "Conclusive - " + conclusive.ToString().ToUpper();
                if (conclusive)
                {
                    foreach (int rID in behaviour.DiagnosesID)
                    {
                        Diagnosis dia = DiagnosisController.getDiagnosisByID(rID);
                        s += Environment.NewLine + "Diagnosis - " + dia.Header;
                    }
                }
                else
                {
                    QuestionGroup qg = QuestionController.getGroupByID(behaviour.DestGrpID);
                    s += Environment.NewLine + "Destination - " + qg.Header;
                }

                cboDiagnosisList.Items.Add(s);
            }
        }

        private void newForm()
        {
            navi = new Navigation();
            naviChildQuestion = new List<NaviChildCriteriaQuestion>();
            naviChildAttribute = new List<NaviChildCritAttribute>();
            cboDiagnosisList.SelectedIndex = -1;
            cboGroupList.SelectedIndex = -1;
            chkConclusive.IsChecked = false;
            stkpnlDiagnosis.Visibility = Visibility.Collapsed;
            stkpnlSectionDestination.Visibility = Visibility.Visible;
            lstDiagnosisList.ItemsSource = new List<Diagnosis>();
            //txtDescription.Text = "";
            reloadAttribute();
            reloadCriteria();
            loadAllBehaviour();
        }

        private void reloadCriteria()
        {
            //lstCriteriaList.ItemsSource = naviChildQuestion;  
            lstCriteriaList.Items.Clear();
            foreach (NaviChildCriteriaQuestion criteria in naviChildQuestion)
            {
                QuestionGroup qg = QuestionController.getGroupByID(criteria.CriteriaGrpID);
                lstCriteriaList.Items.Add(qg.Header + " - " + criteria.Ans);
            }
        }

        private void reloadAttribute()
        {
            lstAttributeList.Items.Clear();
            foreach (NaviChildCritAttribute attr in naviChildAttribute)
            {
                string s = "";
                if (attr.getAttributeTypeENUM() == NaviChildCritAttribute.AttributeCmpType.LessThan)
                {
                    s = "<";
                }
                else
                {
                    s = ">=";
                }
                lstAttributeList.Items.Add("AGE" + s + " " + attr.AttributeValue);
            }
        }

        private void loadQuestionGroup()
        {
            cboGroupList.ItemsSource = QuestionController.getAllQuestionGroup();
            cboDestination.ItemsSource = QuestionController.getAllQuestionGroup();
        }

        private void btnAddCriteria_Click(object sender, RoutedEventArgs e)
        {
            int sIdx = cboGroupList.SelectedIndex;
            bool criteriaResult=false;
            if (sIdx==-1)
            {
                return;
            }

            QuestionGroup qg = (QuestionGroup)cboGroupList.Items[sIdx];

            if (radY.IsChecked == true)
            {
                criteriaResult = true;
            }
            else
            {
                criteriaResult = false;
            }

            NaviChildCriteriaQuestion newCriteria = new NaviChildCriteriaQuestion();
            newCriteria.CriteriaGrpID = qg.GroupID;
            newCriteria.Ans = criteriaResult;
            naviChildQuestion.Add(newCriteria);
            reloadCriteria();
        }

        private void btnDeleteCriteria_Click(object sender, RoutedEventArgs e)
        {
            int sIdx = lstCriteriaList.SelectedIndex;
            if (sIdx == -1)
            {
                return;
            }
            naviChildQuestion.RemoveAt(sIdx);
            reloadCriteria();
        }

        private void btnModifyCriteria_Click(object sender, RoutedEventArgs e)
        {
            bool criteriaResult = false;

            int sIdx = lstCriteriaList.SelectedIndex;
            if (sIdx == -1)
            {
                return;
            }

            QuestionGroup qg = (QuestionGroup)cboGroupList.Items[sIdx];

            if (radY.IsChecked == true)
            {
                criteriaResult = true;
            }
            else
            {
                criteriaResult = false;
            }

            NaviChildCriteriaQuestion oldCriteria = naviChildQuestion[sIdx];
            oldCriteria.CriteriaGrpID = qg.GroupID;
            oldCriteria.Ans = criteriaResult;
            reloadCriteria();
        }

        private void btnAddAttribute_Click(object sender, RoutedEventArgs e)
        {
            NaviChildCritAttribute newAttribute = new NaviChildCritAttribute();
            newAttribute.AttributeName = "AGE";
            newAttribute.AttributeValue = txtAge.Text;
            if (radless.IsChecked == true)
            {
                newAttribute.setRuleType((int)NaviChildCritAttribute.AttributeCmpType.LessThan);
            }
            else
            {
                newAttribute.setRuleType((int)NaviChildCritAttribute.AttributeCmpType.MoreThanEqual);
            }

            naviChildAttribute.Add(newAttribute);
            reloadAttribute();
        }

        private void btnAddDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            List<Diagnosis> dList = (List<Diagnosis>)lstDiagnosisList.ItemsSource;
            frmDiagnosisAddingToPath cDiagnosis = new frmDiagnosisAddingToPath(dList);
            if (cDiagnosis.ShowDialog() == true)
            {
                lstDiagnosisList.ItemsSource = null;
                lstDiagnosisList.ItemsSource = cDiagnosis.getAddedDiagnosis();
            }
        }

        private void btnDeleteAttribute_Click(object sender, RoutedEventArgs e)
        {
            int sIdx = lstAttributeList.SelectedIndex;
            if (sIdx == -1)
            {
                return;
            }
            naviChildAttribute.RemoveAt(sIdx);
            reloadAttribute();
        }

        private void btnModifyAttribute_Click(object sender, RoutedEventArgs e)
        {
            int sIdx = lstAttributeList.SelectedIndex;
            if (sIdx == -1)
            {
                return;
            }
            NaviChildCritAttribute oldAttribute = naviChildAttribute[sIdx];
            oldAttribute.AttributeName = "AGE";
            oldAttribute.AttributeValue = txtAge.Text;
            if (radless.IsChecked == true)
            {
                oldAttribute.setRuleType((int)NaviChildCritAttribute.AttributeCmpType.LessThan);
            }
            else
            {
                oldAttribute.setRuleType((int)NaviChildCritAttribute.AttributeCmpType.MoreThanEqual);
            }

            reloadAttribute();
        }

        private void loadExistingCriteria(NaviChildCriteriaQuestion criteria)
        {
            int sIdx = -1;
            for (int i = 0; i < cboGroupList.Items.Count; i++)
            {
                QuestionGroup qg = (QuestionGroup)cboGroupList.Items[i];
                if (criteria.CriteriaGrpID == qg.GroupID)
                {
                    sIdx = i;
                    break;
                }
            }
            cboGroupList.SelectedIndex = sIdx;
            radY.IsChecked = criteria.Ans;
            radN.IsChecked = !criteria.Ans;
        }

        private void loadExistingAttribute(NaviChildCritAttribute attribute)
        {
            if (attribute.getAttributeTypeENUM() == NaviChildCritAttribute.AttributeCmpType.MoreThanEqual)
            {
                radMoreEqual.IsChecked = true;
            }
            else
            {
                radless.IsChecked = true;
            }
            txtAge.Text = attribute.AttributeValue;
        }

        private void lstCriteriaList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sIdx = lstCriteriaList.SelectedIndex;
            if (sIdx==-1)
            {
                return;
            }
            NaviChildCriteriaQuestion existingCriteria = naviChildQuestion[sIdx];
            loadExistingCriteria(existingCriteria);
        }

        private void lstAttributeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sIdx = lstAttributeList.SelectedIndex;
            if (sIdx == -1)
            {
                return;
            }
            NaviChildCritAttribute existingAttribute = naviChildAttribute[sIdx];
            loadExistingAttribute(existingAttribute);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            newForm();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (saveBehaviour())
            {
                MessageBox.Show("Saved!");
                newForm();
            }
        }

        private bool saveBehaviour()
        {
            if (chkConclusive.IsChecked == true)
            {
                List<int> RIDList = new List<int>();
                if (navi.DiagnosesID.Count > 0)
                {
                    foreach (int diaID in navi.DiagnosesID)
                    {
                        RIDList.Add(diaID);
                    }
                }

                for (int i = 0; i < RIDList.Count; i++)
                {
                    navi.removeDiagnosisID(RIDList[i]);
                }

                for (int i = 0; i < lstDiagnosisList.Items.Count; i++)
                {
                    Diagnosis dia = (Diagnosis)lstDiagnosisList.Items[i];
                    navi.addDiagnosisID(dia.RID);
                }
            }
            else
            {
                int sIdx = cboDestination.SelectedIndex;
                if (sIdx==-1)
                {
                    MessageBox.Show("Please Select ur destination section!");
                    return false;
                }

                QuestionGroup qg = (QuestionGroup)cboDestination.Items[sIdx];
                navi.DestGrpID = qg.GroupID;
            }


            navi.clearCriteriaQuestions();
            navi.clearCriteriaAttributes();

            foreach (NaviChildCriteriaQuestion naviCriteria in naviChildQuestion)
            {
                navi.addNavCriteriaQuestion(naviCriteria);
            }

            foreach (NaviChildCritAttribute naviAttr in naviChildAttribute)
            {
                navi.addNavCriteriaAttribute(naviAttr);
            }

            try
            {
                DefaultBehaviorController.updateRules(navi);
            }
            catch (Exception ex)
            {
                return false;
//                throw;
            }
            return true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int sIdx = cboDiagnosisList.SelectedIndex;
            if (sIdx == -1)
            {
                return;
            }
            //Navigation sNavi = (Navigation)cboDiagnosisList.Items[sIdx];
            DefaultBehaviorController.deleteDefaultBehavior(int.Parse(navi.NavID));
            MessageBox.Show("Behaviour Deleted!");
            newForm();
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cboDiagnosisList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sIdx = cboDiagnosisList.SelectedIndex;
            if (sIdx == -1)
            {
                return;
            }
            navi = AllBehaviour[sIdx];
            LoadExistingBehaviour();
        }

        private void LoadExistingBehaviour()
        {
            bool isConclusive = navi.isConclusive();
            chkConclusive.IsChecked = isConclusive;
            if (isConclusive)
            {
                stkpnlDiagnosisRule.Visibility = Visibility.Visible;
                stkpnlSectionDestination.Visibility = Visibility.Collapsed;
                lstDiagnosisList.ItemsSource = null;
                List<Diagnosis> diaListExist = new List<Diagnosis>();
                List<Diagnosis> diaList = DiagnosisController.getAllDiagnosis();
                foreach (int rID in navi.DiagnosesID)
                {
                    for (int i = 0; i < diaList.Count; i++)
                    {
                        Diagnosis dia = diaList[i];
                        if (dia.RID == rID)
                        {
                            diaListExist.Add(dia);
                            break;
                        }
                    }
                }
                lstDiagnosisList.ItemsSource = diaListExist;
            }
            else
            {
                stkpnlDiagnosisRule.Visibility = Visibility.Collapsed;
                stkpnlSectionDestination.Visibility = Visibility.Visible;
                int sIdx = -1;
                for (int i = 0; i < cboDestination.Items.Count; i++)
                {
                    QuestionGroup qgItem = (QuestionGroup)cboDestination.Items[i];
                    if (qgItem.GroupID==navi.DestGrpID)
                    {
                        sIdx = i;
                        break;
                    }
                }
                cboDestination.SelectedIndex = sIdx;
            }

            this.naviChildQuestion = new List<NaviChildCriteriaQuestion>();
            this.naviChildAttribute = new List<NaviChildCritAttribute>();

            foreach (NaviChildCriteriaQuestion naviCriteria in navi.ChildCriteriaQuestion)
            {
                naviChildQuestion.Add(naviCriteria);
            }

            foreach (NaviChildCritAttribute naviAttr in navi.ChildCriteriaAttributes)
            {
                naviChildAttribute.Add(naviAttr);
            }

            reloadCriteria();
            reloadAttribute();
        }

        private void chkConclusive_Checked(object sender, RoutedEventArgs e)
        {
            stkpnlSectionDestination.Visibility = Visibility.Collapsed;
            stkpnlDiagnosis.Visibility = Visibility.Visible;
        }

        private void chkConclusive_Unchecked(object sender, RoutedEventArgs e)
        {
            stkpnlSectionDestination.Visibility = Visibility.Visible;
            stkpnlDiagnosis.Visibility = Visibility.Collapsed;
        }
    }
}
