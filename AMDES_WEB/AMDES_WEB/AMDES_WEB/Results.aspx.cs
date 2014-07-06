﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AMDES_KBS.Entity;
using AMDES_KBS.Controllers;
using AMDES_WEB.CustomControls;

namespace AMDES_WEB
{
    public partial class Results : System.Web.UI.Page
    {

        public CLIPSController CLIPSCtrl
        {
            set
            {
                Session["clp"] = value;
            }
            get
            {
                return (CLIPSController)Session["clp"];
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                lblApp.Text = CLIPSCtrl.ApplicationName;
                lblAge.Text = CLIPSCtrl.CurrentPatient.getAge().ToString();

                bool result;

                bool.TryParse(Session["Result"].ToString(), out result);

                if (result)
                {
                    loadFindings();
                    loadRecommendations();
                    loadResources();

                    Session["CurrSection"] = 0;
                    Session["History"] = CLIPSCtrl.CurrentPatient.getLatestHistory();
                }
                else
                {
                    Response.Redirect("~/PatientStart.aspx");
                }
            }
        }

        private void loadFindings()
        {

            foreach (Symptom sym in CLIPSCtrl.CurrentPatient.getLatestHistory().SymptomsList)
            {
                Label lblSymptons = new Label();

                lblSymptons.Text = "&emsp;&emsp;" + App.bulletForm() + sym.SymptomName + "<br>";

                phFindings.Controls.Add(lblSymptons);

            }

            if (phFindings.Controls.Count == 0)
            {
                lblHeader.Text = "The evaluation does not suggest dementia in this patient";
            }
            else
            {
                lblHeader.Text = "The patient has the following issues uncovered from the questionnaire:";
            }
        }

        private void loadRecommendations()
        {
            CLIPSCtrl.getResultingDiagnosis();
            List<Diagnosis> result = CLIPSCtrl.CurrentPatient.getLatestHistory().Diagnoses;

            foreach (Diagnosis diag in result)
            {
                DiagnosisUC diagUC = (DiagnosisUC)LoadControl(@"~/CustomControls\DiagnosisUC.ascx");
                diagUC.Recommendation = diag;
                phRecommendations.Controls.Add(diagUC);


            }
        }

        private void loadResources()
        {
            List<Diagnosis> resources = DiagnosisController.getResourceRules();
            foreach (Diagnosis diag in resources)
            {
                DiagnosisUC diagUC = (DiagnosisUC)LoadControl(@"~/CustomControls\DiagnosisUC.ascx");
                diagUC.Recommendation = diag;
                phResources.Controls.Add(diagUC);
            }
        }
    }
}