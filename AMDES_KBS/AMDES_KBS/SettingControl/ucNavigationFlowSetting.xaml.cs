﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AMDES_KBS.Controllers;
using AMDES_KBS.Entity;
using System;

namespace AMDES_KBS
{
    /// <summary>
    /// Interaction logic for ucNavigationSetting.xaml
    /// </summary>
    public partial class ucNavigationFlowSetting : UserControl
    {
        bool Result = true;
        int ageCompareResult = 0;

        bool isConclusive = false;
        NaviChildCriteriaQuestion naviChildCriteriaGroup;
        List<NaviChildCritAttribute> naviChildAttrGroupList;
        List<NaviChildCritAttribute> naviChildOtherAttrGroupList;

        public ucNavigationFlowSetting()
        {
            InitializeComponent();
            loadDistinct();
            lblCurrStep.Content = "Step " + 1;
            naviChildOtherAttrGroupList = new List<NaviChildCritAttribute>();
        }

        public ucNavigationFlowSetting(int step)
        {
            InitializeComponent();
            loadDistinct();
            lblCurrStep.Content = "Step " + step;
        }

        public ucNavigationFlowSetting(int step, Navigation navi)
        {
            InitializeComponent();
            loadDistinct();
            lblCurrStep.Content = "Step " + step;
            loadAnswer(step, navi);
        }

        public void setGroupBox(int grpID)
        {
            QuestionGroup qg = QuestionController.getGroupByID(grpID);
            for (int i = 0; i < cboGroupList.Items.Count; i++)
            {
                QuestionGroup g = (QuestionGroup)cboGroupList.Items[i];
                if (g.Equals(qg))
                {
                    cboGroupList.SelectedIndex = i;
                    break;
                }
            }
        }

        private void getAllCompareType()
        {
            Array values = Enum.GetValues(typeof(NaviChildCritAttribute.AttributeCmpType));

            foreach(Enum val in values )
            {
                cboCompareType.Items.Add(App.processEnumStringForDataBind(val.ToString()));
            }
            cboCompareType.SelectedIndex = 0;
        }



        public void getAnswer()
        {
            naviChildCriteriaGroup = new NaviChildCriteriaQuestion();
            naviChildAttrGroupList = new List<NaviChildCritAttribute>();
            int selectedIdx = cboGroupList.SelectedIndex;
            int currGrpID = ((QuestionGroup)cboGroupList.Items[selectedIdx]).GroupID;
            naviChildCriteriaGroup.CriteriaGrpID = currGrpID;
            naviChildCriteriaGroup.Ans = Result;

            //bool ageExists = false;

            if (this.chkRequireAge.IsChecked == true)
            {
                NaviChildCritAttribute ageAttr = new NaviChildCritAttribute();
                //ageExists = true;
                ageAttr.GroupID = currGrpID;
                ageAttr.AttributeValue = txtAge.Text;
                ageAttr.AttributeName = "AGE";

                ageAttr.setRuleType(cboCompareType.SelectedIndex);
                //if (radMoreEqual.IsChecked == true)
                //{
                //    ageAttr.setRuleType((int)NaviChildCritAttribute.AttributeCmpType.MoreThan);
                //}
                //else
                //{
                //    ageAttr.setRuleType((int)NaviChildCritAttribute.AttributeCmpType.LessThanEqual);
                //}
                naviChildAttrGroupList.Add(ageAttr);
            }

            if (this.chkOtherAttr.IsChecked==true)
            {
                if (naviChildOtherAttrGroupList.Count > 0)
                {
                    foreach (NaviChildCritAttribute attr in naviChildOtherAttrGroupList)
                    {
                        attr.GroupID = currGrpID;
                        naviChildAttrGroupList.Add(attr);
                    }
                }                
            }
        }

        public NaviChildCriteriaQuestion getCriteria()
        {
            return naviChildCriteriaGroup;
        }

        public List<NaviChildCritAttribute> getAttrList()
        {
            return naviChildAttrGroupList;
        }

        public int getGroupID()
        {
            int selectedIdx = cboGroupList.SelectedIndex;

            if (selectedIdx == -1)
            {
                return -1;
            }

            int currGrpID = ((QuestionGroup)cboGroupList.Items[selectedIdx]).GroupID;
            return currGrpID;
        }

        public void loadAnswer(int step, Navigation navi)
        {
            naviChildCriteriaGroup = navi.ChildCriteriaQuestion[step - 1];
            naviChildAttrGroupList = new List<NaviChildCritAttribute>();

            int grpID = naviChildCriteriaGroup.CriteriaGrpID;
            //int destGRP = navi.DestGrpID;
            int selectedIdx = -1;
            for (int i = 0; i < cboGroupList.Items.Count; i++)
            {
                QuestionGroup qg = (QuestionGroup)cboGroupList.Items[i];
                if (qg.GroupID == grpID)
                {
                    selectedIdx = i;
                    break;
                }
            }

            cboGroupList.SelectedIndex = selectedIdx;
            Result = naviChildCriteriaGroup.Ans;

            if (navi.DestGrpID == -1)
            {
                isConclusive = true;
            }
            else
            {
                isConclusive = false;
            }

            List<NaviChildCritAttribute> navigroupList = new List<NaviChildCritAttribute>();
            naviChildOtherAttrGroupList = new List<NaviChildCritAttribute>();

            foreach (NaviChildCritAttribute naviAttr in navi.ChildCriteriaAttributes)
            {
                if (naviAttr.GroupID == grpID)
                {
                    //navigroupList.Add(naviAttr);
                    naviChildAttrGroupList.Add(naviAttr);
                    string attributeName = naviAttr.AttributeName;
                    switch (attributeName)
                    {
                        case "AGE":
                            {
                                chkRequireAge.IsChecked = true;
                                txtAge.Text = naviAttr.AttributeValue;
                                cboCompareType.SelectedIndex = naviAttr.getCompareType();
                                ageCompareResult = naviAttr.getCompareType();
                                //ageResult = naviAttr.Ans;
                                //if (naviAttr.getAttributeTypeENUM() == NaviChildCritAttribute.AttributeCmpType.MoreThan)
                                //{
                                //    radMoreEqual.IsChecked = true;
                                //    ageCompareResult = 1;
                                //}
                                //else
                                //{
                                //    this.radlessThaneqal.IsChecked = true;
                                //    ageCompareResult = 0;
                                //}
                            }
                            break;
                        default:
                            {
                                naviChildOtherAttrGroupList.Add(naviAttr);
                            }
                            break;
                    }
                }
            }

           
            loadCheckedAgeMoreOrLess();
            loadCheckedYN();

            if (naviChildOtherAttrGroupList.Count > 0)
            {
                chkOtherAttr.IsChecked = true;
                reloadAttribute(naviChildOtherAttrGroupList);
            }
            else
            {
                chkOtherAttr.IsChecked = false;
            }
            //loadCheckedAgeTF();
            //loadCheckedAgeTF();
            //loadCheckedAgeMoreOrLess();
            //loadCheckedYN();
            //loadIsConclusive();
            //NaviChildCritAttribute.AttributeCmpType.MoreThanEqual;
            //if (naviChildAttribute.AttributeNameCMP)
            //{

            //}
        }

        private void reloadAttribute(List<NaviChildCritAttribute> otherAttr)
        {
            lstAttributeList.Items.Clear();
            foreach (NaviChildCritAttribute attr in otherAttr)
            {
                string s = "";
                if (attr.getAttributeTypeENUM() == NaviChildCritAttribute.AttributeCmpType.LessThanEqual)
                {
                    s = "<=";
                }
                else if (attr.getAttributeTypeENUM() == NaviChildCritAttribute.AttributeCmpType.LessThan)
                {
                    s = "<";
                }
                else if (attr.getAttributeTypeENUM() == NaviChildCritAttribute.AttributeCmpType.MoreThanEqual)
                {
                    s = ">=";
                }
                else if (attr.getAttributeTypeENUM() == NaviChildCritAttribute.AttributeCmpType.MoreThan)
                {
                    s = ">";
                }

                lstAttributeList.Items.Add(attr.AttributeName + " " + s + " " + attr.AttributeValue);
            }
        }



        public void loadDistinct()
        {
            cboGroupList.ItemsSource = QuestionController.getAllQuestionGroup();
            getAllCompareType();
        }

        private void chkRequireAge_Checked(object sender, RoutedEventArgs e)
        {
            stkpnlAgeSetting.Visibility = Visibility.Visible;
        }

        private void chkRequireAge_Unchecked(object sender, RoutedEventArgs e)
        {
            stkpnlAgeSetting.Visibility = Visibility.Hidden;
        }

        bool IsCheckBoxChecked(CheckBox c)
        {
            if (c.IsChecked == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private void radY_Checked(object sender, RoutedEventArgs e)
        {
            Result = true;
        }

        private void radN_Checked(object sender, RoutedEventArgs e)
        {
            Result = false;
        }

        public void loadCheckedYN()
        {
            radY.IsChecked = Result;
            radN.IsChecked = !Result;
        }

        public void loadCheckedAgeMoreOrLess()
        {
            cboCompareType.SelectedIndex = ageCompareResult;
            //if (ageCompareResult == 0)
            //{
            //    this.radlessThaneqal.IsChecked = true;
            //}
            //else
            //{
            //    radMoreEqual.IsChecked = true;
            //}
        }

        public void loadIsConclusive()
        {
            chkConclusive.IsChecked = isConclusive;
        }

        private void radless_Checked(object sender, RoutedEventArgs e)
        {
            ageCompareResult = 0;
        }

        private void radMoreEqual_Checked(object sender, RoutedEventArgs e)
        {
            ageCompareResult = 1;
        }

        private void chkOtherAttr_Checked(object sender, RoutedEventArgs e)
        {
            stkpnlOtherAttribute.Visibility = Visibility.Visible;
        }

        private void chkOtherAttr_Unchecked(object sender, RoutedEventArgs e)
        {
            stkpnlOtherAttribute.Visibility = Visibility.Collapsed;
        }

        private void btnOtherAttr_Click(object sender, RoutedEventArgs e)
        {
            frmAttributeAddingToPath cAttribute = new frmAttributeAddingToPath(naviChildOtherAttrGroupList);

            if (cAttribute.ShowDialog()==true)
            {
                naviChildOtherAttrGroupList = cAttribute.getOtherAttribute();
                reloadAttribute(naviChildOtherAttrGroupList);
            }
            //  cAttribute = new frmAttributeSetting(naviChildOtherAttrGroupList);

            //if (cAttribute.ShowDialog() == true)
            //{
            //    naviChildOtherAttrGroupList.
            //}
        }

        //private void radAgeTrue_Checked(object sender, RoutedEventArgs e)
        //{
        //    ageResult = true;
        //}

        //private void radAgeFalse_Checked(object sender, RoutedEventArgs e)
        //{
        //    ageResult = false;
        //}
    }
}
