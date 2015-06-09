using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Reflection;
using Zegeger.Diagnostics;

namespace Zegeger.Analysis
{
    public interface ITestable
    {
        bool Test(object[] args);
    }

    public class RuleList
    {
        private List<Rule> ruleList;

        public RuleList(IEnumerable<Rule> rules)
        {
            ruleList = new List<Rule>(rules);
        }

        public RuleList(string xml)
        {
            try
            {
                ruleList = new List<Rule>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNode node = doc.FirstChild;
                if (node.Name == "Rules" && node.HasChildNodes)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        Rule rule = new Rule(child);
                        ruleList.Add(rule);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public RuleList(XmlDocument doc)
        {
            try
            {
                ruleList = new List<Rule>();
                XmlNode node = doc.FirstChild;
                if (node.Name == "Rules" && node.HasChildNodes)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        Rule rule = new Rule(child);
                        ruleList.Add(rule);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public void Add(Rule rule)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            ruleList.Add(rule);
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        public string Test(params object[] args)
        {
            TraceLogger.Write("Enter RuleList", TraceLevel.Noise);
            foreach (Rule rule in ruleList)
            {
                if (rule.Test(args))
                {
                    TraceLogger.Write("Exit RuleList, return " + rule.ReturnValue, TraceLevel.Noise);
                    return rule.ReturnValue;
                }
            }
            TraceLogger.Write("Exit RuleList, return null", TraceLevel.Noise);
            return null;
        }
    }

    public class Rule
    {
        private Operator Root { get; set; }
        public string ReturnValue { get; private set; }

        public Rule(Operator root, string returnValue)
        {
            Root = root;
            ReturnValue = returnValue;
        }

        public Rule(XmlNode node)
        {
            try
            {
                if (node.Name == "Rule" && node.HasChildNodes)
                {
                    ReturnValue = node.Attributes["ReturnValue"].Value;
                    TraceLogger.Write("Rule return value: " + ReturnValue, TraceLevel.Noise);
                    Root = (Operator)Operator.CreateInstance(node.FirstChild);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public bool Test(params object[] args)
        {
            TraceLogger.Write("Enter Rule", TraceLevel.Noise);
            bool result = Root.Test(args);
            TraceLogger.Write("Exit Rule, return " + result, TraceLevel.Noise);
            return result;
        }
    }

    public abstract class Operator : ITestable
    {
        protected List<ITestable> Tests;
        public abstract bool Test(object[] args);

        internal Operator(IEnumerable<ITestable> tests)
        {
            Tests = new List<ITestable>(tests);
        }

        internal Operator(XmlNode node)
        {
            try
            {
                Tests = new List<ITestable>();
                foreach (XmlNode child in node.ChildNodes)
                {
                    Tests.Add(CreateInstance(child));
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static ITestable CreateInstance(XmlNode node)
        {
            TraceLogger.Write("Enter node name: " + node.Name, TraceLevel.Noise);
            
            Type t = Type.GetType("Zegeger.Analysis." + node.Name);
            if(t != null)
            {
                TraceLogger.Write("Creating instance with: " + t.ToString(), TraceLevel.Noise);
                ITestable op = (ITestable)Activator.CreateInstance(t, node);
                TraceLogger.Write("Returning ITestable", TraceLevel.Noise);
                return op;
            }
            TraceLogger.Write("Returning null", TraceLevel.Noise);
            return null;
        }
    }

    public abstract class Tester : ITestable
    {
        protected int ArgIndex { get; set; }
        public abstract bool Test(params object[] args);

        internal Tester()
        { }

        internal Tester(XmlNode node)
        {
            try
            {
                ArgIndex = Int32.Parse(node.Attributes["Index"].Value);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static ITestable CreateInstance(XmlNode node)
        {
            TraceLogger.Write("Enter node name: " + node.Name, TraceLevel.Noise);
            try
            {
                Type t = Type.GetType("Zegeger.Analysis." + node.Name);
                if (t != null)
                {
                    TraceLogger.Write("Creating instance with: " + t.ToString(), TraceLevel.Noise);
                    ITestable op = (ITestable)Activator.CreateInstance(t, node);
                    TraceLogger.Write("Returning ITestable", TraceLevel.Noise);
                    return op;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Returning null", TraceLevel.Noise);
            return null;
        }
    }

    public class Any : Operator
    {
        public Any(IEnumerable<ITestable> tests) : base(tests)
        { }

        public Any(XmlNode node) : base(node)
        { }

        public override bool Test(params object[] args)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            foreach (ITestable test in Tests)
            {
                if (test.Test(args))
                {
                    TraceLogger.Write("Exit return true", TraceLevel.Noise);
                    return true;
                }
            }
            TraceLogger.Write("Exit return false", TraceLevel.Noise);
            return false;
        }
    }

    public class All : Operator
    {
        public All(IEnumerable<ITestable> tests) : base(tests)
        { }

        public All(XmlNode node) : base(node)
        { }

        public override bool Test(params object[] args)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            foreach (ITestable test in Tests)
            {
                if (!test.Test(args))
                {
                    TraceLogger.Write("Exit return false", TraceLevel.Noise);
                    return false;
                }
            }
            TraceLogger.Write("Exit return true", TraceLevel.Noise);
            return true;
        }
    }

    public class None : Operator
    {
        public None(IEnumerable<ITestable> tests) : base(tests)
        { }

        public None(XmlNode node) : base(node)
        { }

        public override bool Test(params object[] args)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            foreach (ITestable test in Tests)
            {
                if (test.Test(args))
                {
                    TraceLogger.Write("Exit return false", TraceLevel.Noise);
                    return false;
                }
            }
            TraceLogger.Write("Exit return true", TraceLevel.Noise);
            return true;
        }
    }

    public class StringTest : Tester
    {
        private string Match { get; set; }
        private StringTestType TestType { get; set; }
        private Regex regex;

        public StringTest(int index, string match, StringTestType type)
        {
            try
            {
                ArgIndex = index;
                Match = match;
                TestType = type;
                if (TestType == StringTestType.Regex)
                {
                    regex = new Regex(Match);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public StringTest(XmlNode node) : base(node)
        {
            try
            {
                Match = node.Attributes["Match"].Value;
                string type = node.Attributes["Type"].Value;
                switch (type)
                {
                    case "EQ":
                        TestType = StringTestType.Equals;
                        break;
                    case "SW":
                        TestType = StringTestType.StartsWith;
                        break;
                    case "EW":
                        TestType = StringTestType.EndsWith;
                        break;
                    case "CN":
                        TestType = StringTestType.Contains;
                        break;
                    case "RX":
                        TestType = StringTestType.Regex;
                        break;
                }
                if (TestType == StringTestType.Regex)
                {
                    regex = new Regex(Match);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public override bool Test(params object[] args)
        {
            TraceLogger.Write("Enter arg to check " + args[ArgIndex] + " with type " + TestType + " against string " + Match, TraceLevel.Noise);
            bool result = false;
            switch(TestType)
            {
                case StringTestType.Equals:
                    result = ((string)args[ArgIndex]).Equals(Match);
                    break;
                case StringTestType.Contains:
                    result = ((string)args[ArgIndex]).Contains(Match);
                    break;
                case StringTestType.StartsWith:
                    result = ((string)args[ArgIndex]).StartsWith(Match);
                    break;
                case StringTestType.EndsWith:
                    result = ((string)args[ArgIndex]).StartsWith(Match);
                    break;
                case StringTestType.Regex:
                    result = regex.IsMatch((string)args[ArgIndex]);
                    break;
            }
            TraceLogger.Write("Exit result = " + result, TraceLevel.Noise);
            return result;
        }
    }

    public enum StringTestType
    {
        Equals,
        StartsWith,
        EndsWith,
        Contains,
        Regex
    }

    public class IntegerTest : Tester
    {
        private int Match { get; set; }
        private IntegerTestType TestType { get; set; }

        public IntegerTest(int index, int match, IntegerTestType type)
        {
            try
            {
                ArgIndex = index;
                Match = match;
                TestType = type;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public IntegerTest(XmlNode node) : base(node)
        {
            try
            {
                Match = Int32.Parse(node.Attributes["Match"].Value);
                string type = node.Attributes["Type"].Value;
                switch (type)
                {
                    case "EQ":
                        TestType = IntegerTestType.Equals;
                        break;
                    case "GT":
                        TestType = IntegerTestType.GreaterThan;
                        break;
                    case "GTE":
                        TestType = IntegerTestType.GreaterThanEquals;
                        break;
                    case "LT":
                        TestType = IntegerTestType.LessThan;
                        break;
                    case "LTE":
                        TestType = IntegerTestType.LessThanEquals;
                        break;
                    case "NEQ":
                        TestType = IntegerTestType.NotEquals;
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        public override bool Test(params object[] args)
        {
            TraceLogger.Write("Enter arg to check " + args[ArgIndex] + " with type " + TestType + " against int " + Match, TraceLevel.Noise);
            bool result = false;
            switch (TestType)
            {
                case IntegerTestType.Equals:
                    result = (((int)args[ArgIndex]) == Match);
                    break;
                case IntegerTestType.GreaterThan:
                    result = (((int)args[ArgIndex]) > Match);
                    break;
                case IntegerTestType.GreaterThanEquals:
                    result = (((int)args[ArgIndex]) >= Match);
                    break;
                case IntegerTestType.LessThan:
                    result = (((int)args[ArgIndex]) <= Match);
                    break;
                case IntegerTestType.LessThanEquals:
                    result = (((int)args[ArgIndex]) < Match);
                    break;
                case IntegerTestType.NotEquals:
                    result = (((int)args[ArgIndex]) != Match);
                    break;
            }
            TraceLogger.Write("Exit result = " + result, TraceLevel.Noise);
            return result;
        }
    }

    public enum IntegerTestType
    {
        Equals,
        GreaterThan,
        GreaterThanEquals,
        LessThan,
        LessThanEquals,
        NotEquals
    }
}
