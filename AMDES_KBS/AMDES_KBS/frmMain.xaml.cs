﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using AMDES_KBS.Controllers;
using AMDES_KBS.Entity;

namespace AMDES_KBS
{
    /// <summary>
    /// Interaction logic for frmMain.xaml
    /// </summary>
    public partial class frmMain : Window
    {
        //private Type currPageType;
        //private AMDESPage currPage;


        public frmMain()
        {
            InitializeComponent();
            frameDisplay.Navigate(new frmOverview(frameDisplay));
        }

        public frmMain(bool? savePatient)
        {
            CLIPSController.savePatient = savePatient;
            InitializeComponent();
            if (CLIPSController.savePatient == false)
            {
                //frameDisplay.Navigate(new frmOverview(frameDisplay));
                frameDisplay.Navigate(new frmPatientDetails(frameDisplay));
                btnPatients.Visibility = Visibility.Collapsed;
                //stkpnlSearchBox.Margin.Left += btnPatients.Width;
                stkpnlSearchBox.Visibility = Visibility.Hidden;
            }
            else
            {
                frameDisplay.Navigate(new frmOverview(frameDisplay));
                btnPatients.Visibility = Visibility.Visible;
                stkpnlSearchBox.Visibility = Visibility.Visible;
            }

            if (!CLIPSController.ExpertUser)
            {
                btnSetting.Visibility = Visibility.Collapsed;
                stkpnlSearchBox.Margin = new Thickness(stkpnlSearchBox.Margin.Left + btnSetting.Width, 0, 0, 0);
            }

            else
                btnSetting.Visibility = Visibility.Visible;



            if (!CLIPSController.enableStats)
            {
                btnStats.Visibility = Visibility.Collapsed;
                stkpnlSearchBox.Margin = new Thickness(stkpnlSearchBox.Margin.Left + btnStats.Width, 0, 0, 0);
            }
            else
                btnStats.Visibility = Visibility.Visible;

        }

        private void listPatients()
        {
            frameDisplay.Navigate(new frmOverview(frameDisplay));
        }

        private void btnNewTest_Click(object sender, RoutedEventArgs e)
        {
            frameDisplay.Navigate(new frmPatientDetails(frameDisplay));
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult result =
            //    MessageBox.Show("This configurations is for expert users only for the setting up of AMDES, "
            //                    + Environment.NewLine +
            //                    "Changing the settings (any one of the them) will cause all existing data to be archived and the system may fail if it is incorrectly configured"
            //                    + Environment.NewLine +
            //                    "Are you sure you want to continue??? ", "Confirmation of entering configuration form",
            //                    MessageBoxButton.YesNo, MessageBoxImage.Question);

            // if (result == MessageBoxResult.Yes)
            // {
            frmSetting SettingForm = new frmSetting();

            SettingForm.ShowDialog();


            //}
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            frmOverview patientResultDisplay;
            string criteria = txtSearchCriteria.Text.Trim();
            List<Patient> plist = PatientController.searchPatient(criteria);
            patientResultDisplay = new frmOverview(frameDisplay, plist);
            frameDisplay.Navigate(patientResultDisplay);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnPatients_Click(object sender, RoutedEventArgs e)
        {
            listPatients();
        }

        private void btnStats_Click(object sender, RoutedEventArgs e)
        {
            frmP2 p2 = new frmP2();
            p2.Show();
        }

    }
}
