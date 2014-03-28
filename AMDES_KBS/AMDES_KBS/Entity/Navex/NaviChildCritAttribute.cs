﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMDES_KBS.Entity
{
    public class NaviChildCritAttribute
    {
        private string navid;

        public string Navid
        {
            get { return navid; }
            set { navid = value; }
        }
        private string attributeName;

        public string AttributeName
        {
            get { return attributeName; }
            set { attributeName = value; }
        }
        private string attributeValue;

        public string AttributeValue
        {
            get { return attributeValue; }
            set { attributeValue = value; }
        }

        private bool ans;

        public bool Ans
        {
            get { return ans; }
            set { ans = value; }
        }

        private AttributeCmpType type;

        private AttributeCmpType Type
        {
            get { return type; }
            set { type = value; }
        }

        public enum AttributeCmpType
        {
            Equal, //==
            MoreThanEqual, // >=
            MoreThan, //>
            LessThanEqual, //<=
            LessThan
        }

        public string getCompareTypeString()
        {
            if (type == AttributeCmpType.Equal)
                return "==";
            else if (type == AttributeCmpType.MoreThanEqual)
                return ">=";
            else if (type == AttributeCmpType.MoreThan)
                return ">";
            else if (type == AttributeCmpType.LessThanEqual)
                return "<=";
            else if (type == AttributeCmpType.LessThan)
                return "<";
            else
                return "";
        }

        public void setRuleType(int x)
        {
            if (typeof(AttributeCmpType).IsEnumDefined(x))
            {
                type = (AttributeCmpType)x;
            }
            else
            {
                throw new InvalidOperationException("Invalid Type!");
            }
        }
        
        public AttributeCmpType getAttributeTypeENUM()
        {
            return type;
        }

        public int getCompareType()
        {
            return (int)type;
        }

        public NaviChildCritAttribute(string navID, string attrName, string attrVal, bool attrAns, NaviChildCritAttribute.AttributeCmpType type)
        {
            this.navid = navID;
            this.attributeName = attrName;
            this.attributeValue = attrVal;
            this.ans = attrAns;
            this.type = type;
        }

        public NaviChildCritAttribute()
        {
        }
    }
}
