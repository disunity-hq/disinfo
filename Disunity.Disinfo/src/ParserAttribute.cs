using System;
using System.Text.RegularExpressions;


namespace Disunity.Disinfo {

    [AttributeUsage(AttributeTargets.Method)]
    public class ParserAttribute : Attribute {

        public Regex Pattern { get; }

        public ParserAttribute(string pattern) {
            Pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }
        
    }

}