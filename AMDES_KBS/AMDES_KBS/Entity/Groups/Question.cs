﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMDES_KBS.Entity
{
    public class Question
    {
        public Question()
        {
        }

        //public Question(int id, string qn)
        //{
        //    this.id = id;
        //    this.name = qn;
        //}

        public Question(int id, string qn, string sym)
        {
            this.id = id;
            this.name = qn;
            this.symptom = sym;
        }


        private string name;
        private int id;

        private string symptom; //if the answer to this group is true, what should it assert about the patient???
        //e.g. Amensia, Apraxia, etc.. Clips need to know what to assert?

        public string Symptom
        {
            get { return symptom; }
            set { symptom = value; }
        }
      
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }
       
    }
}
