﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AMDES_KBS.Entity;
using AMDES_KBS.Controllers;

namespace AMDES_WEB.CustomControls
{
    public partial class Section : System.Web.UI.UserControl
    {

        private int sectionID;
        private QuestionGroup section;

        public int SectionID
        {
            get { return sectionID; }
            set
            {
                sectionID = value;
                section = QuestionController.getGroupByID(value);
                loadQuestions();
            }
        }

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

        private void loadQuestions()
        {
            Control c = GetPostBackControl(Page);

            lblHeader.Text = section.Description.Replace("~~", " <br />");
            lblSection.Text = section.Header;

            if (section.getQuestionTypeENUM() == QuestionType.COUNT)
            {
                QuestionCountGroup qcg = (QuestionCountGroup)section;
                lblScore.Visible = true;
                lblScore.Text = "0";
                lblMax.Text = " / " + qcg.MaxQuestions.ToString();
            }
            else
            {
                lbl1.Visible = lblMax.Visible = lblScore.Visible = false;
            }

            this.phRegister.Controls.Clear();
            int ControlID = 0;

            for (int i = 0; i < section.Questions.Count; i++)
            {
                Question q = section.Questions[i];
                QuestionsUC qnCtrl = (QuestionsUC)LoadControl(@"~/CustomControls\QuestionsUC.ascx");
                qnCtrl.QuestionNo = i + 1;
                qnCtrl.QuestionText = q.Name;
                qnCtrl.QID = q.ID;
                this.phRegister.Controls.Add(qnCtrl);
                ControlID += 1;
            }
        }

        public Control GetPostBackControl(Page page)
        {
            Control control = null;

            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if ((ctrlname != null) & ctrlname != string.Empty)
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }

        //http://www.vbforums.com/showthread.php?649132-Preventing-an-asp-CheckBox-from-losing-it-s-checked-value
        protected void Page_Init(object sender, EventArgs e)
        {
           

            if (CLIPSCtrl.CurrentPatient.getLatestHistory() == null)
                btnPrevious.Visible = false;

            try
            {
                this.SectionID = CLIPSCtrl.getCurrentQnGroupID();
            }
            catch (Exception ex)
            {
                Alert.Show(this.SectionID.ToString() + Environment.NewLine + ex.Message);
               
                //Response.Redirect("~/PatientStart.aspx");
            }


        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                int count = 0;
                foreach (Control c in phRegister.Controls)
                {
                    if (c is QuestionsUC)
                    {
                        QuestionsUC quc = (QuestionsUC)c;
                        if (quc.isYes)
                            count += 1;
                    }
                }

                lblScore.Text = count.ToString();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CLIPSController clp = CLIPSCtrl;

            foreach (Control c in phRegister.Controls)
            {
                if (c is QuestionsUC)
                {
                    QuestionsUC quc = (QuestionsUC)c;
                    if (quc.isYes)
                        clp.assertQuestion(section.GroupID, quc.QID, true);
                }
            }
            
            clp.assertNextSection();
            clp.saveAssertLog();
            clp.saveCurrentNavex();

            CLIPSCtrl = clp;
            Response.Redirect("~/Questionnaire.aspx");
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            CLIPSController clp = CLIPSCtrl;

            clp.assertPrevSection();
            clp.saveAssertLog();
            clp.saveCurrentNavex();

            CLIPSCtrl = clp;
            Response.Redirect("~/Questionnaire.aspx");
        }


    }
}