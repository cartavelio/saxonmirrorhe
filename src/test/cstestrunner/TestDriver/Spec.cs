using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace TestDriver {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EnumAttr : Attribute
    {

        public EnumAttr() { }

    }

    public static class EnumExtension
    {
        public static EnumAttr GetAttr(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            var atts = (EnumAttr[])fieldInfo.GetCustomAttributes(typeof(EnumAttr), false);
            return atts.Length > 0 ? atts[0] : null;
        }
    }


    public class SpecAttr : EnumAttr
    {
        public string version;
        public string fname;
        public string sname;
        public string svname;

        public SpecAttr(String l, String na, String sn, String sv)
        {
            version = l;
            fname = na;
            sname = sn;
            svname = sv;
        }

        public static string VersionOf(Spec spec) {
            return ((SpecAttr) spec.GetAttr()).version;
        }
        
        public static string FullNameOf(Spec spec) {
            return ((SpecAttr) spec.GetAttr()).fname;
        }
        
        public static string ShortNameOf(Spec spec) {
            return ((SpecAttr) spec.GetAttr()).sname;
        }
        
        public static string ShortNameAndVersionOf(Spec spec) {
            return ((SpecAttr) spec.GetAttr()).svname;
        }
    }



    public enum Spec
    {
        [SpecAttr("2.0", "XPath2.0", "XP", "XP20")]
        XP20,
        [SpecAttr("3.0", "XPath3.0", "XP", "XP30")]
        XP30,
        [SpecAttr("3.1", "XPath3.1", "XP", "XP31")]
        XP31,
        [SpecAttr("4.0", "XPath4.0", "XP", "XP40")]
        XP40,
        [SpecAttr("1.0", "XQuery1.0", "XQ", "XQ10")]
        XQ10,
        [SpecAttr("3.0", "XQuery3.0", "XQ", "XQ30")]
        XQ30,
        [SpecAttr("3.1", "XQuery3.1", "XQ", "XQ31")]
        XQ31,
        [SpecAttr("4.0", "XQuery4.0", "XQ", "XQ40")]
        XQ40,
        [SpecAttr("1.0", "XSLT1.0", "XT", "XT10")]
        XT10,
        [SpecAttr("2.0", "XSLT2.0", "XT", "XT20")]
        XT20,
        [SpecAttr("3.0", "XSLT3.0", "XT", "XT30")]
        XT30,
        [SpecAttr("4.0", "XSLT4.0", "XT", "XT40")]
        XT40,
        [SpecAttr("0", "null", "n", "null")]
        NULL,

    }


}
