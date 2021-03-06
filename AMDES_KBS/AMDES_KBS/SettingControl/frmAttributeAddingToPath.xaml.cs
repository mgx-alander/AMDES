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
using AMDES_KBS;

namespace AMDES_KBS
{
    /// <summary>
    /// Interaction logic for frmAttributeAddingToPath.xaml
    /// </summary>
    public partial class frmAttributeAddingToPath : Window
    {

        List<NaviChildCritAttribute> naviChildAttribute;

        public frmAttributeAddingToPath()
        {
            InitializeComponent();
            loadAllAttribute();
            getAllCompareType();
        }

        public frmAttributeAddingToPath(List<NaviChildCritAttribute> otherAttribute)
        {
            InitializeComponent();
            if (otherAttribute == null)
            {
                this.naviChildAttribute = new List<NaviChildCritAttribute>();
            }
            else
            {
                this.naviChildAttribute = otherAttribute;
            }
            loadAllAttribute();
            reloadAttribute();
            getAllCompareType();
        }

        private void getAllCompareType()
        {
            Array values = Enum.GetValues(typeof(AttributeCmpType));

            foreach (Enum val in values)
            {
                cboCompareType.Items.Add(App.processEnumStringForDataBind(val.ToString()));
            }
            cboCompareType.SelectedIndex = 0;
        }


        private void loadAllAttribute()
        {
            cboAttrList.ItemsSource = null;
            cboAttrList.ItemsSource = PatAttributeController.getAllAttributes();
            cboAttrList.SelectedIndex = 0;
        }

        private void reloadAttribute()
        {
            lstAttributeList.Items.Clear();
            foreach (NaviChildCritAttribute attr in naviChildAttribute)
            {
                string s = "";
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
                else if (attr.getAttributeTypeENUM() == AttributeCmpType.Equal)
                {
                    s = "=";
                }

                if (!isAttrCompareNumerical(attr.AttributeName))
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

        private void btnAddAttribute_Click(object sender, RoutedEventArgs e)
        {
            NaviChildCritAttribute newAttribute = new NaviChildCritAttribute();

            if (tcAttribute.SelectedIndex == 0)
            {
                int sidx = cboCompareType.SelectedIndex;
                if (sidx == -1)
                {
                    return;
                }
                newAttribute.AttributeName = "AGE";
                newAttribute.setRuleType(sidx);
                newAttribute.AttributeValue = txtAttrAgeValue.Text;
            }
            else
            {
                int sidx = cboAttrList.SelectedIndex;
                if (sidx == -1)
                {
                    return;
                }
                PatAttribute Attr = (PatAttribute)cboAttrList.Items[sidx];
                newAttribute.AttributeName = Attr.AttributeName;

                if (Attr.getAttributeTypeNUM() == PatAttribute.AttributeType.CATEGORICAL)
                {
                    newAttribute.AttributeValue = cboAttrCatValue.SelectedIndex.ToString();
                    newAttribute.setRuleType((int)AttributeCmpType.Equal);
                }
                else if (Attr.getAttributeTypeNUM() == PatAttribute.AttributeType.NUMERIC)
                {
                    if (txtAttrNumMinValue.Text.Trim().Length == 0 && txtAttrNumMaxValue.Text.Trim().Length == 0)
                    {
                        MessageBox.Show("Please enter the values before adding!", "Invalid inputs", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else if (txtAttrNumMinValue.Text.Trim().CompareTo(txtAttrNumMaxValue.Text.Trim()) == 0)
                    {
                        newAttribute = new NaviChildCritAttribute();
                        newAttribute.AttributeName = Attr.AttributeName;
                        newAttribute.AttributeValue = txtAttrNumMaxValue.Text;
                        newAttribute.setRuleType((int)AttributeCmpType.Equal);
                    }
                    else if (txtAttrNumMinValue.Text.Trim() != "" && txtAttrNumMaxValue.Text.Trim() != "")
                    {
                        newAttribute.AttributeValue = txtAttrNumMinValue.Text;
                        newAttribute.setRuleType((int)AttributeCmpType.MoreThanEqual);
                        if (!checkDuplicateAttribute(newAttribute))
                        {
                            naviChildAttribute.Add(newAttribute);
                        }
                        newAttribute = new NaviChildCritAttribute();
                        newAttribute.AttributeName = Attr.AttributeName;
                        newAttribute.AttributeValue = txtAttrNumMaxValue.Text;
                        newAttribute.setRuleType((int)AttributeCmpType.LessThanEqual);
                    }
                    else
                    {
                        if (txtAttrNumMinValue.Text.Trim() != "" && txtAttrNumMaxValue.Text.Trim() == "")
                        {
                            newAttribute.AttributeValue = txtAttrNumMinValue.Text;
                            newAttribute.setRuleType((int)AttributeCmpType.MoreThanEqual);
                        }
                        else
                        {
                            newAttribute.AttributeValue = txtAttrNumMaxValue.Text;
                            newAttribute.setRuleType((int)AttributeCmpType.LessThanEqual);
                        }
                    }
                }
            }
            if (!checkDuplicateAttribute(newAttribute))
            {
                naviChildAttribute.Add(newAttribute);
            }
            reloadAttribute();
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

        private bool checkDuplicateAttribute(NaviChildCritAttribute attr)
        {
            foreach (NaviChildCritAttribute critAttr in this.naviChildAttribute)
            {
                if (attr.AttributeName == critAttr.AttributeName)
                {
                    if (attr.getAttributeTypeENUM() == critAttr.getAttributeTypeENUM())
                    {
                        if (!isAttrCompareNumerical(attr.AttributeName))
                        {
                            if (attr.AttributeValue == critAttr.AttributeValue)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            critAttr.AttributeValue = attr.AttributeValue;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool isAttrCompareNumerical(String attrName)
        {
            if (attrName=="AGE")
            {
                return true;
            }

            try
            {
                if (PatAttributeController.searchPatientAttribute(attrName).getAttributeTypeNUM() == PatAttribute.AttributeType.NUMERIC)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public List<NaviChildCritAttribute> getOtherAttribute()
        {
            return this.naviChildAttribute;
        }

        private void cboAttrList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sidx = cboAttrList.SelectedIndex;
            if (sidx == -1)
            {
                return;
            }
            PatAttribute Attr = (PatAttribute)cboAttrList.Items[sidx];
            txtAttrNumMinValue.Text = "";
            txtAttrNumMaxValue.Text = "";
            cboAttrCatValue.Items.Clear();
            if (Attr.getAttributeTypeNUM() == PatAttribute.AttributeType.NUMERIC)
            {
                stkpnlNum.Visibility = Visibility.Visible;
                cboAttrCatValue.Visibility = Visibility.Collapsed;
            }
            else if (Attr.getAttributeTypeNUM() == PatAttribute.AttributeType.CATEGORICAL)
            {
                stkpnlNum.Visibility = Visibility.Collapsed;
                cboAttrCatValue.Visibility = Visibility.Visible;
                foreach (string value in Attr.CategoricalVals)
                {
                    cboAttrCatValue.Items.Add(value);
                }
            }
        }

        private void txtAttrNumValue_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = App.NumericValidationTextBox(e.Text);
        }
    }
}
