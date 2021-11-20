using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StyleCop;
using StyleCop.CSharp;

using Newtonsoft.Json;

namespace MiaoRule
{
    /// <summary>  
    /// Custom analyzer for demo purposes.  
    /// </summary>  
    [SourceAnalyzer(typeof(CsParser))]
    public class MyCustomAnalyzer : SourceAnalyzer
    {


        /// <summary>  
        /// Extremely simple analyzer for demo purposes.  
        /// </summary>  
        public override void AnalyzeDocument(CodeDocument document)
        {
            CsDocument doc = (CsDocument)document;

            // skipping wrong or auto-generated documents  
            if (doc.RootElement == null || doc.RootElement.Generated)
                return;

            // check all class entries  
            doc.WalkDocument(
                ProcessElement,
                null,
                ProcessExpression
                );
 
        }

        /// <summary>  
        /// 对元素进行检查
        /// </summary>  
        private bool ProcessElement(
            CsElement element,
            CsElement parentElement,
            object context)
        {
            //检查喵喵类
            this.CheckMiaoClass(element, parentElement, context);
            //检查喵函数
            this.CheckSayMethod(element, parentElement, context);

            return true;
        }
       
        private bool CheckMiaoClass(CsElement element, CsElement parentElement, object context)
        {
            // if current element is not a class then continue walking  
            if (element.ElementType != ElementType.Class)
                return true;

            // check whether class name contains "a" letter  
            Class classElement = (Class)element;
            if (classElement.Declaration.Name.Contains("cat") || classElement.Declaration.Name.Contains("Cat"))
            {
                // add violation  
                // (note how custom message arguments could be used)  
                AddViolation(
                    classElement,
                    classElement.Location,
                    "MiaoMiaoFindTargetClass",
                    classElement.FriendlyTypeText);
            }

            // continue walking in order to find all classes in file  
            return true;
        }
        private bool CheckSayMethod(CsElement element, CsElement parentElement, object context)
        {
            if (element.ElementType != ElementType.Method) { return true; }   
            Method methodElement = (Method)element;

            if (methodElement.Declaration.Name.Contains("Say"))
            {
                if (methodElement.Parameters[0].Name.Contains("word"))
                {
                    AddViolation(
                    methodElement,
                    methodElement.Location,
                    "MiaoMiaoFindTargetMethod",
                    methodElement.FriendlyTypeText);
                }
            }
            // continue walking in order to find all classes in file  
            return true;
        }


     /// <summary>
     /// 对函数进行检查
     /// </summary>
        private bool ProcessExpression(
            Expression expression,
            Expression parentExpression,
            Statement parentStatement,
            CsElement parentElement,
            object context)
        {
            if (expression.ExpressionType == ExpressionType.MethodInvocation)
            {
                MethodInvocationExpression methodInvocation = (MethodInvocationExpression)expression;
                if (methodInvocation.Name.Tokens.MatchTokens("word", ".", "ToString")) //此处必须是完全匹配
                {
                    this.CheckArguments(parentElement, methodInvocation);
                }
                else
                {
                    Dictionary<string, object> reportDic = new Dictionary<string, object>();
                    reportDic.Add("type", "MethodInvocation 中未识别的Token");


                    string tokensName = "";
                    foreach (var t in methodInvocation.Name.Tokens)
                    {
                        tokensName += t.Text;
                    }
                    reportDic.Add("Tokens", tokensName);
                   
                    if (expression.Text != null) { reportDic.Add("expression", expression.Text); }
                    if (parentExpression != null && parentExpression.Text != null) { reportDic.Add("parentExpression", parentExpression.Text);}
                    if (parentElement != null && parentElement.Name != null) { reportDic.Add("parentElement" , parentElement.Name); }
                   
                    string Msg = JsonConvert.SerializeObject(reportDic);
                    AddViolation(parentElement,methodInvocation.LineNumber, "MiaoMiaoDebugReport", Msg);
                }
            }
            else { }
            return true;
        }

  
        private void CheckArguments(CsElement element, MethodInvocationExpression debugAssertMethodCall)
        {
            Param.AssertNotNull(element, "element");
            Param.AssertNotNull(debugAssertMethodCall, "debugAssertMethodCall");
            
            // Extract the second argument.
            Argument Argument = null;
            if (debugAssertMethodCall.Arguments.Count >= 1)
            {
                Argument = debugAssertMethodCall.Arguments[0];
            }


            if (Argument == null || Argument.Tokens.First == null)
            {
                // There is no message argument or the message argument is empty.
                this.AddViolation(element, debugAssertMethodCall.LineNumber, "MiaoMiaoToStringMustProvideFormat");
            }
            else if (ArgumentTokensMatchStringEmpty(Argument))
            {
                // The message argument contains an empty string or null.
                this.AddViolation(element, debugAssertMethodCall.LineNumber, "MiaoMiaoToStringMustProvideFormat");
            }
        }


        /// <summary>
        /// Determine whether the argument passed in is equivalent to ""
        /// </summary>
        /// <param name="argument">The Argument to check.</param>
        /// <returns>True if equivalent to string.empty otherwise false.</returns>
        private static bool ArgumentTokensMatchStringEmpty(Argument argument)
        {
            CsToken firstToken = argument.Tokens.First.Value;

            if (firstToken.CsTokenType == CsTokenType.String && IsEmptyString(firstToken.Text))
            {
                return true;
            }

            if (firstToken.CsTokenType == CsTokenType.Null)
            {
                return true;
            }

            if (argument.Tokens.MatchTokens(StringComparison.OrdinalIgnoreCase, "string", ".", "empty"))
            {
                return true;
            }

            if (argument.Tokens.MatchTokens(StringComparison.OrdinalIgnoreCase, "system", ".", "string", ".", "empty"))
            {
                return true;
            }

            if (argument.Tokens.MatchTokens(StringComparison.OrdinalIgnoreCase, "global", "::", "system", ".", "string", ".", "empty"))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Determines whether the given text contains an empty string, which can be represented as "" or @"".
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>Returns true if the </returns>
        private static bool IsEmptyString(string text)
        {
            Param.AssertNotNull(text, "text");

            // A string is always considered empty if it is two characters or less, because then it must have at least
            // the opening and closing quotes plus something in between.
            if (text.Length > 2)
            {
                // If this is a literal string, then it must be more than three characters.
                if (text[0] == '@')
                {
                    if (text.Length > 3)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            // This is an empty string.
            return true;
        }

    }
}




