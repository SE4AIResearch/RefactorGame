using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace InGameTextEditor.Format
{
    /// <summary>
    /// Text formatter applying syntax highlighting for C# code.
    /// </summary>
    public class RefactoriaSyntaxHighlighter : TextFormatter
    {
        /// <summary>
        /// Text style for comments.
        /// </summary>
        public TextStyle textStyleComment = new TextStyle(new Color(0.5f, 0.5f, 0.5f));

        /// <summary>
        /// Text style for string and characters.
        /// </summary>
        public TextStyle textStyleString = new TextStyle(new Color(182f/255f, 73f/255f, 0f/255f));

        /// <summary>
        /// Text style for numbers.
        /// </summary>
        public TextStyle textStyleNumber = new TextStyle(new Color(64f/255f, 110f/255f, 142f/255f));

        /// <summary>
        /// Text style for C# keywords
        /// </summary>
        public TextStyle textStyleKeyword = new TextStyle(new Color(94f/255f, 109f/255f, 92f/255f));

        public TextStyle textStyleKCall = new TextStyle(new Color(149f/255f, 75f/255f, 192f/255f));

        // collection of all C# keywords
        readonly string[] keywords = {"var", "if", "else if", "else", "while", "func"};

        readonly string[] kcalls = {"GOTO", "GET", "PLACE", "TAKE", "ACTIVATE", "DELIVER", "PRINT"};

        // indicates if the text formatter has been initialized
        bool initialized = false;

        // regex to match the text
        Regex regex = null;

        /// <summary>
        /// Indicates whether this text formatter has been initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public override bool Initialized
        {
            get { return initialized; }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Init()
        {
            // create regex pattern
            string regexPattern = "";

            // matches the end of a multi-line comment: ...*/
            // use mark u001B + 0 to indicate that this line starts with an open multi-line comment) 
            regexPattern += @"(?<multiLineCommentEnding>^\u001B0.*?\*/)";

            // matches the start of a multi-line comment: /*...
            regexPattern += @"|(?<multiLineCommentStarting>/\*((?!\*/).)*$)";

            // matches a comment with /* and */
            regexPattern += @"|(?<comment>/\*((?!\*/).)*\*/)";

            // matches a line comment: //...
            regexPattern += @"|(?<comment>//.*$?)";

            // matches a string within double quotes: "..."
            regexPattern += @"|(?<string>""(?:[^""\\]|\\.)*[""]?)";

            // matches the start of a verbatim string: @"...
            regexPattern += @"|(?<verbatimStringStarting>@""(?:[^""""]|"""")*$)";

            // matches the end of a verbatim string: ..." (using mark u001B + 1 to indicate that this line starts with an open verbatim string)
            regexPattern += @"|(?<verbatimStringEnding>^\u001B1(?:[^""""]|"""")*[""])";

            // matches a complete verbatim string: @"..."
            regexPattern += @"|(?<verbatimString>@""(?:[^""""]|"""")*[""])";

            // matches a string within single quotes : '...'
            regexPattern += @"|(?<char>'(?:[^'\\]|\\.)*[']?)";

            // matches a floating point number
            regexPattern += @"|(?<float>\b[0-9]+(\.[0-9]+)?[dfDF])";

            // matches a floating point number
            regexPattern += @"|(?<floatImplicit>\b[0-9]*\.[0-9]+[dfDF]?)";

            // matches an integer number
            regexPattern += @"|(?<intImplicit>\b[0-9]+)";

            // matches a C# keyword
            regexPattern += @"|(?<keyword>";
            for (int i = 0; i < keywords.Length; i++)
                regexPattern += "\\b" + keywords[i] + "\\b" + (i < keywords.Length - 1 ? "|" : "");
            regexPattern += ")";

            // matches a KCall
            regexPattern += @"|(?<kcall>";
            for (int i = 0; i < kcalls.Length; i++)
                regexPattern += "\\b" + kcalls[i] + "\\b" + (i < kcalls.Length - 1 ? "|" : "");
            regexPattern += ")";

            // create regex
            regex = new Regex(regexPattern);

            // initialization complete
            initialized = true;
        }

        /// <summary>
        /// Is called by the text editor when the given line has changed.
        /// </summary>
        /// <param name="line">Line that has changed.</param>
        public override void OnLineChanged(Line line)
        {
            // collection of text format groups
            List<TextFormatGroup> textFormatGroups = new List<TextFormatGroup>();

            // detect if line starts with an open multi-line comment
            bool lineStartsWithMultiLineComment = line.PreviousLine != null && line.PreviousLine.GetProperty<bool>("endsWithMultiLineComment", false);
            bool endsExistingMultiLineComment = false;
            bool startsNewMultiLineComment = false;

            // detect if line starts with an open verbatim string
            bool lineStartsWithVerbatimString = line.PreviousLine != null && line.PreviousLine.GetProperty<bool>("endsWithVerbatimString", false);
            bool endsExistingVerbatimString = false;
            bool startsNewVerbatimString = false;

            if (line.Text.Length > 0)
            {
                MatchCollection matches = null;

                // match line text
                if (lineStartsWithMultiLineComment)
                {
                    // add special mark to indicate that the line starts with an open multi-line comment
                    // escape character (unicode 001B) followed by 0
                    matches = regex.Matches('\u001B' + "0" + line.Text);
                }
                else if (lineStartsWithVerbatimString)
                {
                    // add special mark to indicate that the line starts with an open verbatim string
                    // escape character (unicode 001B) followed by 1
                    matches = regex.Matches('\u001B' + "1" + line.Text);
                }
                else
                    matches = regex.Matches(line.Text);

                // loop through all matches
                foreach (Match match in matches)
                {
                    int i = 0;
                    foreach (Group group in match.Groups)
                    {
                        if (group.Success && i > 0)
                        {
                            string groupName = regex.GroupNameFromNumber(i);
                            int tokenStartIndex = group.Index;
                            int tokenEndIndex = tokenStartIndex + group.Value.Length - 1;

                            // add corresponding text format group
                            switch (groupName)
                            {
                                case "multiLineCommentStarting":
                                    if (!lineStartsWithVerbatimString || endsExistingVerbatimString)
                                    {
                                        startsNewMultiLineComment = true;
                                        textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleComment));
                                    }

                                    break;
                                case "multiLineCommentEnding":
                                    endsExistingMultiLineComment = true;
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex + 2, tokenEndIndex, textStyleComment));
                                    break;
                                case "comment":
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleComment));
                                    break;
                                case "verbatimStringStarting":
                                    if (!lineStartsWithMultiLineComment || endsExistingMultiLineComment)
                                    {
                                        startsNewVerbatimString = true;
                                        textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleString));
                                    }

                                    break;
                                case "verbatimStringEnding":
                                    endsExistingVerbatimString = true;
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex + 2, tokenEndIndex, textStyleString));
                                    break;
                                case "verbatimString":
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleString));
                                    break;
                                case "string":
                                case "char":
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleString));
                                    break;
                                case "float":
                                case "floatImplicit":
                                case "intImplicit":
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleNumber));
                                    break;
                                case "keyword":
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleKeyword));
                                    break;
                                case "kcall":
                                    textFormatGroups.Add(new TextFormatGroup(tokenStartIndex, tokenEndIndex, textStyleKCall));
                                    break;
                            }
                        }

                        i++;
                    }
                }

                // correct offset introduced by multiline escape sequence
                if (lineStartsWithMultiLineComment || lineStartsWithVerbatimString)
                {
                    foreach (TextFormatGroup textFormatGroup in textFormatGroups)
                    {
                        textFormatGroup.startIndex -= 2;
                        textFormatGroup.endIndex -= 2;
                    }
                }
            }

            // set property indicating if the current line ends with a multi-line comment
            if (lineStartsWithMultiLineComment && !endsExistingMultiLineComment)
            {
                textFormatGroups.Clear();
                if (line.Text.Length > 0)
                    textFormatGroups.Add(new TextFormatGroup(0, line.Text.Length - 1, textStyleComment));
                line.SetProperty<bool>("endsWithMultiLineComment", true);
            }
            else
                line.SetProperty<bool>("endsWithMultiLineComment", startsNewMultiLineComment);

            // set property indicating if the current line ends with a verbatim string
            if (lineStartsWithVerbatimString && !endsExistingVerbatimString)
            {
                textFormatGroups.Clear();
                if (line.Text.Length > 0)
                    textFormatGroups.Add(new TextFormatGroup(0, line.Text.Length - 1, textStyleString));
                line.SetProperty<bool>("endsWithVerbatimString", true);
            }
            else
                line.SetProperty<bool>("endsWithVerbatimString", startsNewVerbatimString);

            // apply format to line
            line.ApplyTextFormat(textFormatGroups);
        }
    }
}