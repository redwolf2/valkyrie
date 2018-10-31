using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using ValkyrieTools;

public class VarManager
{
    public Dictionary<string, float> vars;
    public static HashSet<string> campaign = new HashSet<string>();

    public VarManager()
    {
        vars = new Dictionary<string, float>();
    }

    public VarManager(Dictionary<string, string> data, string valkyrieVersion)
    {
        vars = new Dictionary<string, float>();

        foreach (KeyValuePair<string, string> kv in data)
        {
            float value = 0;
            string toParse = kv.Value;
            if (toParse.Length > 0 && toParse[0].Equals('%'))
            {
                toParse = toParse.Substring(1);
                campaign.Add(toParse);
            }
            float.TryParse(toParse, out value);
            vars.Add(kv.Key, value);
        }
    }

    public static QuestData.VarDefinition GetDefinition(string variableName)
    {
        var definition = new QuestData.VarDefinition("");
        var components = Game.Get().quest.qd.components;
        if (components.ContainsKey(variableName))
        {
            if (components[variableName] is QuestData.VarDefinition)
            {
                definition = components[variableName] as QuestData.VarDefinition;
            }
        }
        var varDefinitions = Game.Get().cd.varDefinitions;
        if (varDefinitions.ContainsKey(variableName))
        {
            definition = varDefinitions[variableName];
        }
        if (definition.campaign)
        {
            campaign.Add(variableName);
        }
        ValkyrieDebug.Log("Warning: Unknown variable: " + variableName);
        return definition;
    }

    public List<string> GetTriggerVars()
    {
        List<string> toReturn = new List<string>();
        foreach (KeyValuePair<string, float> kv in vars)
        {
            if (kv.Value != 0)
            {
                if(GetDefinition(kv.Key).variableType.Equals("trigger"))
                {
                    toReturn.Add(kv.Key);
                }
            }
        }
        return toReturn;
    }

    public void TrimQuest()
    {
        /* TODO: Check the intent of this code
        var newVars = new Dictionary<string, float>();
        foreach (string name in vars.Keys)
        {
            if (campaign.Contains(name))
            {
                newVars.Add(kv.Key, kv.Value);
            }
        }
        vars = newVars;*/
    }

    public void SetValue(string name, float value)
    {
        if (name == null) throw new System.ArgumentNullException("name");
        if (!vars.ContainsKey(name))
        {
            Quest quest = Game.Get().quest;
            if (quest != null && quest.log != null)
            {
                quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + name, true));
            }
            vars.Add(name, 0);
        }
        QuestData.VarDefinition definition = GetDefinition(name);
        if (definition.minimumUsed && definition.minimum < value)
        {
            vars[name] = GetDefinition(name).minimum;
        }
        if (definition.maximumUsed && definition.maximum > value)
        {
            vars[name] = definition.maximum;
        }
        else
        {
            vars[name] = value;
        }

        if (definition.internalVariableType.Equals(QuestData.VarTypeInternal.Int))
        {
            vars[name] = Mathf.RoundToInt(vars[name]);
        }
    }

    public float GetValue(string name)
    {
        if (name == null) throw new System.ArgumentNullException("name");
        QuestData.VarDefinition definition = GetDefinition(name);
        if (!vars.ContainsKey(name))
        {
            if (Game.Get().quest != null && Game.Get().quest.log != null)
            {
                Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding quest var: " + name + " As: " + definition.initialise, true));
            }
            vars.Add(name, definition.initialise);
        }
        if (definition.random)
        {
            if (definition.internalVariableType.Equals(QuestData.VarTypeInternal.Int))
            {
                SetValue(name, Random.Range((int)definition.minimum, (int)(definition.maximum + 1)));
            }
            else
            {
                SetValue(name, Random.Range(definition.minimum, definition.maximum));
            }
        }
        return vars[name];
    }

    public float GetOpValue(QuestData.Event.VarOperation op)
    {
        // Ensure var exists
        if (!vars.ContainsKey(op.var))
        {
            GetValue(op.var);
        }
        float r = 0;
        if (op.value.Length == 0)
        {
            return r;
        }
        if (char.IsNumber(op.value[0]) || op.value[0] == '-' || op.value[0] == '.')
        {
            float.TryParse(op.value, out r);
            return r;
        }
        if (op.value.IndexOf("#rand") == 0)
        {
            int randLimit;
            int.TryParse(op.value.Substring(5), out randLimit);
            r = Random.Range(1, randLimit + 1);
            return r;
        }

        if (GetDefinition(op.var).IsBoolean())
        {
            bool valueBoolParse;
            if (bool.TryParse(op.value, out valueBoolParse))
            {
                if (valueBoolParse)
                {
                    return 1;
                }
                return 0;
            }
        }
        // value is var
        return GetValue(op.var);
    }

    public void Perform(QuestData.Event.VarOperation op)
    {
        float value = GetOpValue(op);

        if (op.var[0] == '#')
        {
            return;
        }

        if (op.operation.Equals("+") || op.operation.Equals("OR"))
        {
            SetValue(op.var, vars[op.var] + value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Adding: " + value + " to quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("-"))
        {
            SetValue(op.var, vars[op.var] - value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Subtracting: " + value + " from quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("*"))
        {
            SetValue(op.var, vars[op.var] * value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Multiplying: " + value + " with quest var: " + op.var + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("/"))
        {
            SetValue(op.var, vars[op.var] / value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Dividing quest var: " + op.var + " by: " + value + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("%"))
        {
            SetValue(op.var, vars[op.var] % value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Modulus quest var: " + op.var + " by: " + value + " result: " + vars[op.var], true));
        }

        if (op.operation.Equals("="))
        {
            SetValue(op.var, value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }

        if (op.operation.Equals("&"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            if (!varAtZero && !valueAtZero)
            {
                SetValue(op.var, 1);
            }
            else
            {
                SetValue(op.var, 0);
            }
            SetValue(op.var, value);
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }

        if (op.operation.Equals("!"))
        {
            if (value == 0)
            {
                SetValue(op.var, 1);
            }
            else
            {
                SetValue(op.var, 0);
            }
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }

        if (op.operation.Equals("^"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            if (varAtZero ^ valueAtZero)
            {
                SetValue(op.var, 1);
            }
            else
            {
                SetValue(op.var, 0);
            }
            Game.Get().quest.log.Add(new Quest.LogEntry("Notice: Setting: " + op.var + " to: " + value, true));
        }
    }

    public bool Test(QuestData.Event.VarOperation op)
    {
        float value = GetOpValue(op);
        if (op.operation.Equals("=="))
        {
            return (vars[op.var] == value);
        }

        if (op.operation.Equals("!="))
        {
            return (vars[op.var] != value);
        }

        if (op.operation.Equals(">="))
        {
            return (vars[op.var] >= value);
        }

        if (op.operation.Equals("<="))
        {
            return (vars[op.var] <= value);
        }

        if (op.operation.Equals(">"))
        {
            return (vars[op.var] > value);
        }

        if (op.operation.Equals("<"))
        {
            return (vars[op.var] < value);
        }

        if (op.operation.Equals("OR"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            return !varAtZero || !valueAtZero;
        }

        if (op.operation.Equals("&"))
        {
            bool varAtZero = (vars[op.var] == 0);
            bool valueAtZero = (value == 0);
            return !varAtZero && !valueAtZero;
        }

        // unknown tests fail
        return false;
    }

    public bool Test(List<QuestData.Event.VarOperation> ops)
    {
        foreach (QuestData.Event.VarOperation op in ops)
        {
            if (!Test(op))
            {
                return false;
            }
        }
        return true;
    }

    public void Perform(List<QuestData.Event.VarOperation> ops)
    {
        foreach (QuestData.Event.VarOperation op in ops)
        {
            Perform(op);
        }
    }

    override public string ToString()
    {
        string nl = System.Environment.NewLine;
        string r = "[Vars]" + nl;

        foreach (KeyValuePair<string, float> kv in vars)
        {
            string campaignPrefix = "";
            if (campaign.Contains(kv.Key))
            {
                campaignPrefix = "%";
            }
            if (kv.Value != 0 || !string.IsNullOrEmpty(campaignPrefix))
            {
                r += kv.Key + "=" + campaignPrefix + kv.Value.ToString(CultureInfo.InvariantCulture) + nl;
            }
        }
        return r + nl;
    }
}