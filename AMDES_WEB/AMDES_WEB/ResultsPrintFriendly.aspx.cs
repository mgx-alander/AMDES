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
    public partial class WebForm1 : System.Web.UI.Page
    {
        public CLIPSWebController CLIPSCtrl
        {
            set
            {
                Session["clp"] = value;
            }
            get
            {
                return (CLIPSWebController)Session["clp"];
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                lblDate.Text = DateTime.Now.ToString("dd MMM yyyy");
                lblAge.Text = CLIPSCtrl.CurrentPatient.getAge().ToString();
                lblApp.Text = CLIPSCtrl.ApplicationContext.Name ;
                lblTestName.Text = App.readAppTitle(CLIPSCtrl.ApplicationContext.FolderName);
                bool result;
                bool.TryParse(Session["Result"].ToString(), out result);

                if (result)
                {
                    loadFindings();
                    loadRecommendations();
                }
                else
                {
                    Response.Redirect("~/PatientStart.aspx");
                }
            }
        }

        private void loadFindings()
        {

            foreach (Symptom sym in CLIPSCtrl.CurrentPatient.getLatestHistory(CLIPSCtrl.ApplicationContext.HistoryDataPath).SymptomsList)
            {
                Label lblSymptons = new Label();

                lblSymptons.Text = "&emsp;&emsp;" + App.bulletForm() + sym.SymptomName + "<br>";
                phFindings.Controls.Add(lblSymptons);

            }

            if (phFindings.Controls.Count == 0)
            {
                lblHeader.Text = App.readNoSymptomString(CLIPSCtrl.ApplicationContext.FolderName);//"The evaluation does not suggest Dementia in this patient";
            }
            else
            {
                lblHeader.Text = "The patient has the following issues uncovered from the questionnaire:";
            }
        }

        private void loadRecommendations()
        {
            CLIPSCtrl.getResultingDiagnosis();
            List<Diagnosis> result = CLIPSCtrl.CurrentPatient.getLatestHistory(CLIPSCtrl.ApplicationContext.HistoryDataPath).Diagnoses;

            foreach (Diagnosis diag in result)
            {
                DiagnosisUC diagUC = (DiagnosisUC)LoadControl(@"~/CustomControls\DiagnosisUC.ascx");
                diagUC.Recommendation = diag;
                phRecommendations.Controls.Add(diagUC);
                phRecommendations.Controls.Add(new LiteralControl("<br>"));
            }
        }

      
    }
}
