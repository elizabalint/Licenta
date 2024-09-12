using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using PROJECT;
public class TagManager 
{
    private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
    {
        { "<player>", () => ProjectGameSave.activeFile.playerName},
        {"<time>", () => DateTime.Now.ToString("hh:mm tt") },
        { "<input>", () => InputPanel.instance.lastInput},
        {"<tempVal1>", () => "42"}
    };
    private static readonly Regex tagRegex = new Regex("<\\w+>");

    public static string Inject(string text, bool injectTags =true, bool injectVariables = true)
    {
        if(injectTags)
            text =InjectTags(text);
       /* if(injectVariables)
            text =InjectVariables(text);*/
        return text;
    }
    //ok
    private static string InjectTags(string value)
    {
        if (tagRegex.IsMatch(value))
        {
            foreach (Match match in tagRegex.Matches(value))
            {
                if (tags.TryGetValue(match.Value, out var tagValueRequest))
                {
                    value = value.Replace(match.Value, tagValueRequest());
                }
            }
        }
        return value;
    }

    private static string InjectVariables(string value)
    {
        var matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_IDS);
        var matchesList = matches.Cast<Match>().ToList();
        for(int i= matchesList.Count-1; i>=0; i--)
        {
            var match = matchesList[i];
            string variableName = match.Value.TrimStart(VariableStore.VARIABLE_ID, '!');
            bool negate = match.Value.StartsWith('!');

            bool endInIllegalCharacter = variableName.EndsWith(VariableStore.DATABASE_VARIABLE_RELATION_ID);
            if (endInIllegalCharacter)
            {
                variableName = variableName.Substring(0,variableName.Length-1);
            }
            
            if (!VariableStore.TryGetValue(variableName, out object variableValue))
            {
                //Debug.LogError($"Variable {variableName} not found in string assignment");
                continue;
            }

            if (negate && variableValue is bool )
            {
                variableValue= !(bool)variableValue;
            }

            int lengthToBeRemoved = (match.Index + match.Length)> value.Length ? value.Length-match.Index:match.Length;
            if (endInIllegalCharacter)
                lengthToBeRemoved -= 1;

            value = value.Remove(match.Index, lengthToBeRemoved);
            value = value.Insert(match.Index, variableValue.ToString());

        }
        //Debug.Log(value);
        return value;
    }
}
