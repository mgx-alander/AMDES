﻿
 ﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AMDES_KBS.Entity;
using Mommosoft.ExpertSystem;

namespace AMDES_KBS.Controllers
{
    public class CLIPSController
    {
        public static bool ExpertUser;
        public static bool enableSavePatient;
        public static bool enablePrev;
        public static bool enableStats;
        public static bool secretUser;

        public static ApplicationContext selectedAppContext;

        public static List<ApplicationContext> AllAppContexts;

        private static Patient pat;
        public static bool? savePatient = false;
        private List<History> hyList = new List<History>();

        public static bool? readOnly = false;
        //warning only for viewing the data will only be saved for latest test for assertions 
        //all other tests are readonly, do not call clips controller!

        //for debug purpose, to pull out to test on clips, and to pull out to assert for restore patient
        private static List<string> assertLog = new List<string>();

        static int count = 0; //just a counter to check the number of assertions... 

        //WARNING MOMOSOFT CLIPS REQUIRED x86 MODE ONLY, ALL OTHER MODE WILL FAIL
        private static Mommosoft.ExpertSystem.Environment env = new Mommosoft.ExpertSystem.Environment();


        public static string dataPath = "";
        private static string clpPath = "";// = @"engine\" + EnginePathController.readEngineFileName().FileName; //dementia.clp";

        public static void setCLPPath(EngineFile ef)
        {
            clpPath = @"engine\" + ef.FileName; //dementia.clp";
        }

        public static string getCLPPath()
        {
            return clpPath;
        }

        public static Patient CurrentPatient
        {
            get
            {
                return CLIPSController.pat;
            }
            set
            {
                if (value != null)
                    CLIPSController.pat = value;
                else
                    throw new NullReferenceException("Current Patient is NULL!");
            }
        }

        public static void saveAssertLog()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in assertLog)
            {
                sb.AppendLine(s);
            }
            string filePath = dataPath + CurrentPatient.NRIC + ".log";
            File.WriteAllText(filePath, sb.ToString());
        }

        public static History loadSavedAssertions()
        {
            //CurrentPatient.SymptomsList.Clear();
            //call this f(x) everytime u click a new patient
            env.Clear();
            assertLog.Clear();
            count = 0;

            env.Load(clpPath);
            reset();

            if (File.Exists(dataPath) && HistoryController.isHistoryExist(pat.NRIC))
            {
                //File Ops here
                string line;
                StreamReader file = new System.IO.StreamReader(dataPath);

                while ((line = file.ReadLine()) != null)
                {
                    assert(new StringBuilder(line));
                    count++;
                }

                file.Close();
                saveAssertLog();
                //End File Op
                run();
                return CurrentPatient.getLatestHistory();
            }
            else
            {
                clearAndLoadNew();
                return null;
            }
        }

        public static void clearAndLoadNew()
        {
            if (CurrentPatient != null)
            {
                //CurrentPatient.SymptomsList.Clear();
                //call this f(x) everytime u click a new patient
                env.Clear();
                assertLog.Clear();
                count = 0;

                env.Load(clpPath);
                reset();
                run();

                List<QuestionGroup> grps = QuestionController.getAllQuestionGroup();

                FirstQuestion fq = FirstQuestionController.readFirstQuestion();
                List<Rules> rList = NavigationController.getAllRules();
                List<Navigation> defBehavior = DefaultBehaviorController.getAllDefaultBehavior();

                if (grps.Count == 0)
                {
                }
                else if (fq == null)
                {
                    throw new InvalidOperationException("First question is undefined!");
                }
                else
                {
                    loadQuestions(grps);

                    loadDiagnosis();

                    loadNavex(fq, rList, defBehavior);
                    saveAssertLog();
                    assertAllAttributes();
                    run();
                }

            }
            else
            {
                throw new NullReferenceException("Current Patient is Null!, please set CurrentPatient before loading.");
            }
        }



        private static void loadDiagnosis()
        {
            List<Diagnosis> diagList = DiagnosisController.getAllDiagnosis();
            StringBuilder sb;

            foreach (Diagnosis d in diagList)
            {
                sb = new StringBuilder();
                //(candidate-diagnosis (RID R9) (Header "Patient OK") (Comment "Patient has no cognitive deficit or dementia"))
                sb.Append("(candidate-diagnosis (RID R" + d.RID.ToString() + ") ");
                sb.Append("(Header " + "\"" + d.Header.Trim().Replace("~~", " ") + "\"" + ") ");
                sb.Append("(Comment " + "\"" + d.Comment.Trim().Replace("~~", " ") + "\"" + ") ");

                if (d.Link.Length > 0)
                    sb.Append("(Link " + "\"" + d.Link + "\"" + ")");
                sb.Append(") ");
                assert(sb);
            }

        }

        private static void assertAllAttributes()
        {
            assert(new StringBuilder("(attribute AGE " + CurrentPatient.getAge() + ")"), false);

            foreach (KeyValuePair<string, double> kvp in CurrentPatient.getAttributes())
            {
                assert(new StringBuilder("(attribute " + kvp.Key.ToUpper().Replace(" ", "_") + " " + kvp.Value.ToString() + ")"), false);
            }
        }

        private static void run()
        {
            env.Run();
            //assertLog.Add("(run)");
        }

        public static void reset()
        {
            env.Reset();
            assertLog.Add("(reset)");
        }

        private static void assert(StringBuilder sb, bool init = true)
        {
            String a = sb.ToString().Trim();
            try
            {
                env.AssertString(a);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            assertLog.Add(a);

            if (!init)
                saveAssertLog();

            count++;

        }

        //http://msdn.microsoft.com/en-us/library/dd460713(v=vs.100).aspx
        private static void loadQuestions(List<QuestionGroup> grps)
        {
            StringBuilder sb;

            foreach (QuestionGroup qg in grps)
            //Parallel.ForEach(grps, qg =>
            {
                sb = new StringBuilder();
                sb.Append("(group (GroupID _" + qg.GroupID + ") (SuccessType ");

                if (qg.getQuestionTypeENUM() == QuestionType.COUNT)
                {
                    QuestionCountGroup qcg = (QuestionCountGroup)qg;

                    sb.Append(qcg.getQuestionTypeENUM().ToString() + ") (SuccessArg " + qcg.Threshold + ")) ");
                }
                else
                {
                    sb.Append(qg.getQuestionTypeENUM().ToString() + ")) ");
                }

                assert(sb);
                sb.Clear();

                //grp symptom assertion
                if (qg.Symptom.Length > 0)
                {
                    //20150930 - Negation Logic
                    if (qg.isNegation)
                        sb.Append("(groupid-symptoms (GroupID _" + qg.GroupID + ") (negation Yes) (symptom " + "\"" + qg.Symptom + "\"" + ") )");
                    else
                        sb.Append("(groupid-symptoms (GroupID _" + qg.GroupID + ") (negation No) (symptom " + "\"" + qg.Symptom + "\"" + ") )");
                    assert(sb);
                    sb.Clear();
                }


                foreach (Question q in qg.Questions)
                {
                    sb.Clear();

                    sb.Append("(question (ID _" + qg.GroupID + "." + q.ID + ") ");
                    sb.Append("(GroupID _" + qg.GroupID + ") ");

                    sb.Append("(QuestionText " + "\"" + q.Name + "\"" + ") ");
                    //irrelevant to dump to clips required only when doing on command prompt
                    sb.Append("(Score " + q.Score + ") ");
                    sb.Append(")");
                    assert(sb);

                    //question symptom assertion
                    if (q.Symptom.Length > 0)
                    {
                        sb.Clear();
                        if (q.isNegation)
                            sb.Append("(questionid-symptoms (GroupID _" + qg.GroupID + ") (negation Yes) (QuestionID _" + qg.GroupID + "." + q.ID + ") (symptom " + "\"" + q.Symptom + "\"" + ") )");
                        else
                            sb.Append("(questionid-symptoms (GroupID _" + qg.GroupID + ") (negation No) (QuestionID _" + qg.GroupID + "." + q.ID + ") (symptom " + "\"" + q.Symptom + "\"" + ") )");
                        assert(sb);
                    }
                }

            }//);

        }

        private static void loadNavex(FirstQuestion fq, List<Rules> rList, List<Navigation> defBehavior)
        {
            //1st navex point
            assert(new StringBuilder("(Navigation  (DestinationGroupID _" + fq.GrpID + ") (NavigationID _0) )"));

            foreach (Navigation n in defBehavior)
            {
                createNavigationAssertion(n);
            }

            List<Navigation> nList = new List<Navigation>();
            int i = 0;
            foreach (Rules r in rList)
            {
                foreach (Navigation n in r.Navigations)
                {
                    if (n != null && !nList.Contains(n))
                        nList.Add(n);
                }

            }
            Navigation.CriteriaSortingComparer comparer = new Navigation.CriteriaSortingComparer();
            nList.Sort(comparer);


            foreach (Navigation n in nList)
            {
                createNavigationAssertion(n);
            }
        }



        private static void createNavigationAssertion(Navigation n)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(Navigation " + "(NavigationID N" + n.NavID + ") ");


            if (n.DestGrpID != -1)
            {
                sb.Append("(DestinationGroupID _" + n.DestGrpID + ") ");
            }

            if (n.isConclusive())
            {
                sb.Append("(RID");
                foreach (int x in n.DiagnosesID)
                {
                    sb.Append(" R" + x.ToString());
                }
                sb.Append(") ");
            }

            sb.Append(")");
            assert(sb);

            sb.Clear();
            foreach (NaviChildCriteriaQuestion ncq in n.ChildCriteriaQuestion)
            {
                sb = new StringBuilder();
                sb.Append("(NaviChildCritQuestion (NavigationID N" + n.NavID + ") ");
                sb.Append("(CriteriaGroupID _" + ncq.CriteriaGrpID + ") ");
                if (ncq.Ans == false)
                {
                    sb.Append("(CriteriaAnswer No)");
                }
                else
                {
                    sb.Append("(CriteriaAnswer Yes)");
                }
                sb.Append(")");

                assert(sb);

                sb.Clear();
            }

            foreach (NaviChildCritAttribute nca in n.ChildCriteriaAttributes)
            {
                sb = new StringBuilder();
                //(NaviChildCritA (NavigationID GO_C) (AttributeName Age) (AttributeValue 50) (AttributeCompareType <) )
                sb.Append("(NaviChildCritAttribute (NavigationID N" + n.NavID + ") ");
                sb.Append("(AttributeName " + nca.AttributeName.ToUpper().Replace(" ", "_") + ") ");
                sb.Append("(AttributeValue " + nca.AttributeValue + ") ");
                sb.Append("(AttributeCompareType " + NaviChildCritAttribute.getCompareTypeString(nca.getAttributeTypeENUM()) + ") ");
                sb.Append(")");

                assert(sb);

                sb.Clear();
            }
        }

        public static void assertQuestion(int grpID, int id, bool answer, bool isNegation)
        {
            //to paste to load questions
            StringBuilder sb = new StringBuilder("(choice _" + grpID + "." + id + " ");

            if (isNegation)
            {
                if (answer)
                    sb.Append("No");
                else
                    sb.Append("Yes");
            }
            else
            {
                if (answer)
                    sb.Append("Yes");
                else
                    sb.Append("No");
            }

            sb.Append(")");

            assert(sb, false);
            run();


        }

        public static void assertNextSection()
        {
            assert(new StringBuilder("(next)"), false);
            run();
        }

        public static void assertPrevSection()
        {
            assert(new StringBuilder("(previous)"), false);
            run();
        }

        public static void retractDiagnosis()
        {
            assertPrevSection();
        }

        public static int getCurrentQnGroupID()
        {
            //WARNING: Require Navigation to be SETUP OR INSTANT FAIL!

            String evalStr = "(find-all-facts ((?f Currentgroup)) TRUE)"; //" (find-all-facts((?a Currentgroup)) TRUE)";
            MultifieldValue mv = ((MultifieldValue)env.Eval(evalStr));

            string x = "-1";
            foreach (FactAddressValue fv in mv)
            {
                x = fv.GetFactSlot("GroupID").ToString();
            }
            x = x.Remove(0, 1); //remove _

            if (x.CompareTo("RESULT") == 0) //
            {
                return -1; //when -1 call getResultingDiagnosis()
            }
            else
            {
                return int.Parse(x);
            }
        }

        public static void getResultingDiagnosis()
        {
            History h = getCurrentPatientHistory();

            String evalStr = "(find-all-facts ((?f diagnosis)) TRUE)"; //" (find-all-facts((?a Currentgroup)) TRUE)";
            //String evalStr = "(find-all-facts ((?f NaviHistory)) TRUE)";
            MultifieldValue mv = ((MultifieldValue)env.Eval(evalStr));


            foreach (FactAddressValue fv in mv)
            {
                //multi field need to use array choices
                MultifieldValue ArrayChoices = (MultifieldValue)fv.GetFactSlot("RID");

                for (int i = 0; i < ArrayChoices.Count(); i++)
                {
                    string x = ArrayChoices[i].ToString().Remove(0, 1);
                    int diagID = int.Parse(x);
                    if (diagID == -99)
                    {
                        Diagnosis d = new Diagnosis(diagID, fv.GetFactSlot("Comment").ToString(), "Rule was unhandled");
                        h.addDiagnosis(d);
                        break;
                    }
                    else
                    {
                        Diagnosis d = DiagnosisController.getDiagnosisByID(diagID);

                        foreach (CmpAttribute kvp in d.getAttributes())
                        {
                            if (kvp.Key.ToUpper().CompareTo("AGE") == 0)
                            {
                                bool match = false;
                                switch (kvp.CompareType)
                                {
                                    case AttributeCmpType.Equal:
                                        if (CurrentPatient.getAge() == kvp.Value)
                                            match = true;
                                        break;
                                    case AttributeCmpType.LessThan:
                                        if (CurrentPatient.getAge() < kvp.Value)
                                            match = true;
                                        break;

                                    case AttributeCmpType.LessThanEqual:
                                        if (CurrentPatient.getAge() <= kvp.Value)
                                            match = true;
                                        break;

                                    case AttributeCmpType.MoreThan:
                                        if (CurrentPatient.getAge() > kvp.Value)
                                            match = true;
                                        break;

                                    case AttributeCmpType.MoreThanEqual:
                                        if (CurrentPatient.getAge() >= kvp.Value)
                                            match = true;
                                        break;

                                }
                                if (match == true)
                                {
                                    if (d.Comment.Trim().Length > 0)
                                    {
                                        d.Comment += System.Environment.NewLine;
                                    }
                                    d.Comment += "   " + App.bulletForm() + " Age "
                                        + NaviChildCritAttribute.getCompareTypeString(kvp.CompareType) + " " + kvp.Value.ToString();
                                }

                            }
                            else
                            {
                                //Get the key value from CurrentPatient.
                                PatAttribute pa = PatAttributeController.searchPatientAttribute(kvp.Key.ToUpper());
                                CmpAttribute ca = kvp;

                                if (pa.AttrType == PatAttribute.AttributeType.CATEGORICAL)
                                {
                                    if (CurrentPatient.retrieveAttribute(pa.AttributeName) == ca.Value)
                                    {
                                        if (d.Comment.Trim().Length > 0)
                                        {
                                            d.Comment += System.Environment.NewLine;
                                        }
                                        d.Comment += "   " + App.bulletForm() + " "
                                            + kvp.Key + " is of " + pa.getCategoryByID(ca.Value);
                                    }
                                }
                                else if (pa.AttrType == PatAttribute.AttributeType.NUMERIC)
                                {
                                    bool match = false;
                                    switch (ca.CompareType)
                                    {
                                        case AttributeCmpType.Equal:
                                            if (CurrentPatient.retrieveAttribute(pa.AttributeName) == ca.Value)
                                                match = true;
                                            break;

                                        case AttributeCmpType.LessThan:
                                            if (CurrentPatient.retrieveAttribute(pa.AttributeName) < ca.Value)
                                                match = true;
                                            break;

                                        case AttributeCmpType.LessThanEqual:
                                            if (CurrentPatient.retrieveAttribute(pa.AttributeName) <= ca.Value)
                                                match = true;
                                            break;

                                        case AttributeCmpType.MoreThan:
                                            if (CurrentPatient.retrieveAttribute(pa.AttributeName) > ca.Value)
                                                match = true;
                                            break;

                                        case AttributeCmpType.MoreThanEqual:
                                            if (CurrentPatient.retrieveAttribute(pa.AttributeName) >= ca.Value)
                                                match = true;
                                            break;
                                    }
                                    if (match == true)
                                    {
                                        if (d.Comment.Trim().Length > 0)
                                        {
                                            d.Comment += System.Environment.NewLine;
                                        }
                                        d.Comment += "   " + App.bulletForm() + " " + kvp.Key + " "
                                            + NaviChildCritAttribute.getCompareTypeString(ca.CompareType) + " " + ca.Value.ToString();
                                    }
                                }
                            }
                        }


                        if (d.RetrieveSym && d.RetrievalIDList.Count > 0)
                        {
                            //foreach (int qgID in d.RetrievalIDList)
                            for (int k = 0; k < d.RetrievalIDList.Count; k++)
                            {
                                int qgID = d.RetrievalIDList[k];

                                QuestionGroup qg = QuestionController.getGroupByID(qgID);

                                //List<Symptom> grpSymptoms = getPatientSymptomByQG(qgID);
                                List<Symptom> grpSymptoms = getPatientSymptomByQG(qgID);
                                for (int j = 0; j < grpSymptoms.Count; j++)
                                {
                                    Symptom s = grpSymptoms[j];
                                    if (d.Comment.Trim().Length == 0)
                                    {
                                        d.Comment += "   " + App.bulletForm() + " " + s.SymptomName.Trim();
                                    }
                                    else
                                    {
                                        d.Comment += System.Environment.NewLine + "   " + App.bulletForm() + " " + s.SymptomName.Trim();
                                    }

                                    if (j == 0 && qg.getQuestionTypeENUM() == QuestionType.COUNT)
                                    {
                                        d.Header += System.Environment.NewLine + qg.Header + " - " + s.ScoreString;
                                    }
                                }
                            }

                        }
                        d.Comment = d.Comment.Trim();

                        h.addDiagnosis(d);
                    }
                }

            }

            h = getCurrentPatientSymptom(h);
            h.setCompleted();

            HistoryController.updatePatientNavigationHistory(h, CurrentPatient.AssessmentDate.Date);
        }

        public static void saveCurrentNavex()
        {
            History h = getCurrentPatientHistory();

            String evalStr = "(find-all-facts ((?f diagnosis)) TRUE)"; //" (find-all-facts((?a Currentgroup)) TRUE)";
            //String evalStr = "(find-all-facts ((?f NaviHistory)) TRUE)";
            MultifieldValue mv = ((MultifieldValue)env.Eval(evalStr));

            foreach (FactAddressValue fv in mv)
            {
                //multi field need to use array choices
                MultifieldValue ArrayChoices = (MultifieldValue)fv.GetFactSlot("RID");

                for (int i = 0; i < ArrayChoices.Count(); i++)
                {
                    string x = ArrayChoices[i].ToString().Remove(0, 1);
                    int diagID = int.Parse(x);
                    if (diagID == -99)
                    {
                        Diagnosis d = new Diagnosis(diagID, fv.GetFactSlot("Comment").ToString(), "Rule was unhandled");
                        h.addDiagnosis(d);
                        break;
                    }
                    else
                    {
                        Diagnosis d = DiagnosisController.getDiagnosisByID(diagID);
                        if (d.RetrieveSym && d.RetrievalIDList.Count > 0)
                        {
                            //foreach (int k in d.RetrievalIDList)
                            for (int k = 0; k < d.RetrievalIDList.Count; k++)
                            {
                                int qgID = d.RetrievalIDList[k];

                                QuestionGroup qg = QuestionController.getGroupByID(qgID);

                                //List<Symptom> grpSymptoms = getPatientSymptomByQG(qgID);
                                List<Symptom> grpSymptoms = getPatientSymptomByQG(qgID);
                                //foreach (Symptom s in grpSymptoms)
                                for (int j = 0; j < grpSymptoms.Count; j++)
                                {
                                    Symptom s = grpSymptoms[j];

                                    if (d.Comment.Trim().Length == 0)
                                    {
                                        d.Comment += s.SymptomName.Trim();
                                    }
                                    else
                                    {
                                        d.Comment += ", " + s.SymptomName.Trim();
                                    }

                                    d.Comment = d.Comment.Trim();

                                    if (j == 0 && qg.getQuestionTypeENUM() == QuestionType.COUNT)
                                    {
                                        d.Header += System.Environment.NewLine + qg.Header + " - " + s.ScoreString;
                                    }
                                }
                            }
                        }
                        d.Comment = d.Comment.Trim();
                        h.addDiagnosis(d);
                    }
                }

            }

            h = getCurrentPatientSymptom(h);
            if (savePatient.Value)
                HistoryController.updatePatientNavigationHistory(h, CurrentPatient.AssessmentDate.Date);

            else
                HistoryController.updatePatientNavigationHistory(h, new DateTime(0));

        }

        private static List<int> getNaviHistory()
        {
            List<int> naviHistory = new List<int>();
            String evalStr = "(find-all-facts((?a NaviHistory)) TRUE)";
            MultifieldValue mv = ((MultifieldValue)env.Eval(evalStr));

            foreach (FactAddressValue fv in mv)
            {
                //multi field need to use array choices
                MultifieldValue ArrayChoices = (MultifieldValue)fv.GetFactSlot("ID");

                for (int i = 1; i < ArrayChoices.Count() - 1; i++)
                {
                    string x = ArrayChoices[i].ToString().Remove(0, 1);
                    if (x.CompareTo("RESULT") != 0) //
                    {
                        naviHistory.Add(int.Parse(x));
                    }
                }
            }

            return naviHistory;
        }

        private static List<Symptom> getPatientSymptomByQG(int qgID)
        {
            List<Symptom> sList = new List<Symptom>();

            String evalStr = "(find-all-facts((?s symptoms)) TRUE)";
            MultifieldValue mv = ((MultifieldValue)env.Eval(evalStr));

            foreach (FactAddressValue fv in mv)
            {
                string grpID = fv.GetFactSlot("GroupID").ToString().Remove(0, 1);
                //string qID = fv.GetFactSlot("QuestionID").ToString().Remove(0, 1);
                int y;
                bool result = int.TryParse(grpID, out y);

                if (result && qgID == y)
                {
                    Symptom s = new Symptom(fv.GetFactSlot("symptom").ToString().Replace('"', ' '), grpID.ToString());

                    QuestionGroup qg = QuestionController.getGroupByID(y);
                    if (qg.getQuestionTypeENUM() == QuestionType.COUNT)
                    {
                        evalStr = "(find-all-facts((?g group)) TRUE)";
                        MultifieldValue mv1 = ((MultifieldValue)env.Eval(evalStr));
                        foreach (FactAddressValue fav in mv1)
                        {
                            int id = int.Parse(fav.GetFactSlot("GroupID").ToString().Remove(0, 1));
                            if (id == y)
                            {
                                float succArg = float.Parse(fav.GetFactSlot("SuccessArg").ToString());
                                float trueCount = float.Parse(fav.GetFactSlot("TrueCount").ToString());
                                //20151012 - Bug here should not be tagged to individual symptom
                                // s.SymptomName += "Patient's Score: " + trueCount + ", Normal Score: " + succArg + " or more";
                                s.ScoreString = "Patient's Score: " + trueCount + ", Normal Score: " + succArg + " or more";
                                break;
                            }
                        }
                    }

                    sList.Add(s);
                }
            }
            return sList;
        }

        private static History getCurrentPatientSymptom(History h)
        {
            String evalStr = "(find-all-facts((?s symptoms)) TRUE)";
            MultifieldValue mv = ((MultifieldValue)env.Eval(evalStr));

            foreach (FactAddressValue fv in mv)
            {
                string grpID = fv.GetFactSlot("GroupID").ToString().Remove(0, 1);
                string qID = fv.GetFactSlot("QuestionID").ToString().Remove(0, 1);
                int y;
                bool result = int.TryParse(grpID, out y);


                Symptom s = new Symptom(fv.GetFactSlot("symptom").ToString().Replace('"', ' '),
                                        grpID.ToString());
                if (result)
                {
                    QuestionGroup qg = QuestionController.getGroupByID(y);
                    if (qg.getQuestionTypeENUM() == QuestionType.COUNT)
                    {
                        evalStr = "(find-all-facts((?g group)) TRUE)";
                        MultifieldValue mv1 = ((MultifieldValue)env.Eval(evalStr));
                        foreach (FactAddressValue fav in mv1)
                        {
                            int id = int.Parse(fav.GetFactSlot("GroupID").ToString().Remove(0, 1));
                            if (id == y)
                            {
                                float succArg = float.Parse(fav.GetFactSlot("SuccessArg").ToString());
                                float trueCount = float.Parse(fav.GetFactSlot("TrueCount").ToString());
                                //20151012 - Bug here should not be tagged to individual symptom
                                //s.SymptomName += "- Patient's Score: " + trueCount + ", Normal Score: " + succArg + " or more";
                                s.ScoreString = "Patient's Score: " + trueCount + ", Normal Score: " + succArg + " or more";
                                break;
                            }
                        }
                    }
                }

                h.addSymptom(s);
            }

            return h;
        }

        private static History getCurrentPatientHistory()
        {
            History history;
            List<int> navHistory = getNaviHistory();
            if (savePatient.Value)
            {
                history = new History(CurrentPatient.NRIC, CurrentPatient.AssessmentDate);
            }
            else
            {
                history = new History(CurrentPatient.NRIC, new DateTime(0));
            }

            String evalStr = " (find-all-facts((?a question)) TRUE)";

            for (int i = 0; i < navHistory.Count; i++)
            {
                history.createNewHistory(navHistory[i]);
            }

            MultifieldValue mv = ((MultifieldValue)env.Eval(evalStr));

            foreach (FactAddressValue fv in mv)
            {
                //question history YES ONLY
                int qgID = int.Parse(fv.GetFactSlot("GroupID").ToString().Remove(0, 1));

                if (navHistory.Contains(qgID))
                {
                    string clpQID = fv.GetFactSlot("ID").ToString().Remove(0, 1);
                    int realQID = int.Parse(clpQID.Split('.')[1]);

                    string answer = fv.GetFactSlot("answer").ToString();

                    if (answer.ToUpper().CompareTo("YES") == 0)
                        history.updateHistoryItem(qgID, realQID, true);

                    else
                        history.updateHistoryItem(qgID, realQID, false);
                }
            }
            return history;
        }
    }
}

