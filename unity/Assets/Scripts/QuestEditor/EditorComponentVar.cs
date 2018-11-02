using UnityEngine;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentVar : EditorComponent
{
    QuestData.VarDefinition varComponent;

    UIElementEditable initialiseUIE;
    UIElementEditable minimumUIE;
    UIElementEditable maximumUIE;

    public EditorComponentVar(string nameIn) : base()
    {
        var game = Game.Get();
        varComponent = game.quest.qd.components[nameIn] as QuestData.VarDefinition;
        component = varComponent;
        name = component.sectionName;
        Update();
    }

    override protected void RefreshReference()
    {
        base.RefreshReference();
        varComponent = component as QuestData.VarDefinition;
    }

    override public float AddSubComponents(float offset)
    {
        var ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_TYPE")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 8, 1);
        ui.SetText(new StringKey("val","VAR_TYPE_" + varComponent.variableType));
        ui.SetButton(CycleVarType);
        new UIElementBorder(ui);
        offset += 2;

        offset = AddCampaignControl(offset);
        offset = AddRandomControl(offset);
        offset = AddInitialiseControl(offset);
        offset = AddLimitsControl(offset);

        return offset;
    }

    protected float AddCampaignControl(float offset)
    {
        if (varComponent.variableType.Equals("trigger")) return offset;

        var ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_CAMPAIGN")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        ui.SetText(new StringKey("val", varComponent.campaign ? "TRUE" : "FALSE"));
        ui.SetButton(CampaignToggle);
        new UIElementBorder(ui);

        return offset + 2;
    }

    protected float AddRandomControl(float offset)
    {
        if (varComponent.variableType.Equals("trigger")) return offset;

        var ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_RANDOM")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(8, offset, 4, 1);
        ui.SetText(new StringKey("val", varComponent.random ? "TRUE" : "FALSE"));
        ui.SetButton(RandomToggle);
        new UIElementBorder(ui);

        return offset + 2;
    }

    protected float AddInitialiseControl(float offset)
    {
        if (varComponent.variableType.Equals("trigger")) return offset;
        if (varComponent.campaign) return offset;
        if (varComponent.random) return offset;
        
        var ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_INITIALISE")));

        if (varComponent.variableType.Equals("bool"))
        {
            string initString = new StringKey("val","FALSE").Translate();
            if (varComponent.initialise != 0)
            {
                initString = new StringKey("val","TRUE").Translate();
            }
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(8, offset, 4, 1);
            ui.SetText(initString);
            ui.SetButton(SetInitialise);
            new UIElementBorder(ui);
        }
        else
        {
            initialiseUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            initialiseUIE.SetLocation(8, offset, 4, 1);
            initialiseUIE.SetText(varComponent.initialise.ToString());
            initialiseUIE.SetButton(SetInitialise);
            initialiseUIE.SetSingleLine();
            new UIElementBorder(initialiseUIE);
        }

        return offset + 2;
    }

    protected float AddLimitsControl(float offset)
    {
        if (varComponent.variableType.Equals("trigger")) return offset;
        if (varComponent.variableType.Equals("bool")) return offset;

        var ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_MINIMUM")));
        
        if (!varComponent.random)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(8, offset, 4, 1);
            string minEnableString = new StringKey("val", varComponent.minimumUsed ? "TRUE" : "FALSE").Translate();
            ui.SetText(minEnableString);
            ui.SetButton(SetMinimumEnable);
            new UIElementBorder(ui);
        }

        if (varComponent.minimumUsed)
        {
            minimumUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            minimumUIE.SetLocation(12.5f, offset, 4, 1);
            minimumUIE.SetText(varComponent.minimum.ToString());
            minimumUIE.SetButton(SetMinimum);
            minimumUIE.SetSingleLine();
            new UIElementBorder(minimumUIE);
        }

        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 8, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "VAR_MAXIMUM")));

        if (!varComponent.random)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(8, offset, 4, 1);
            string maxEnableString = new StringKey("val", varComponent.maximumUsed ? "TRUE" : "FALSE").Translate();
            ui.SetText(maxEnableString);
            ui.SetButton(SetMaximumEnable);
            new UIElementBorder(ui);
        }

        if (varComponent.maximumUsed)
        {
            maximumUIE = new UIElementEditable(Game.EDITOR, scrollArea.GetScrollTransform());
            maximumUIE.SetLocation(12.5f, offset, 4, 1);
            maximumUIE.SetText(varComponent.maximum.ToString());
            maximumUIE.SetButton(SetMaximum);
            maximumUIE.SetSingleLine();
            new UIElementBorder(maximumUIE);
        }

        return offset + 2;
    }

    public void CycleVarType()
    {
        if (varComponent.variableType.Equals("float"))
        {
            varComponent.SetVariableType("int");
        }
        else if (varComponent.variableType.Equals("int"))
        {
            varComponent.SetVariableType("bool");
        }
        else if (varComponent.variableType.Equals("bool"))
        {
            varComponent.SetVariableType("trigger");
        }
        else
        {
            varComponent.SetVariableType("float");
        }
        Update();
    }

    public void CampaignToggle()
    {
        varComponent.campaign = !varComponent.campaign;
        Update();
    }

    public void RandomToggle()
    {
        varComponent.random = !varComponent.random;
        varComponent.minimumUsed = true;
        varComponent.maximumUsed = true;
        Update();
    }

    public void SetInitialise()
    {
        if (varComponent.variableType.Equals("bool"))
        {
            if (varComponent.initialise == 0)
            {
                varComponent.initialise = 1;
            }
            else
            {
                varComponent.initialise = 0;
            }
        }
        else
        {
            float.TryParse(initialiseUIE.GetText(), out varComponent.initialise);
        }
    }

    public void SetMinimumEnable()
    {
        varComponent.minimumUsed = !varComponent.minimumUsed;
        Update();
    }

    public void SetMaximumEnable()
    {
        varComponent.maximumUsed = !varComponent.maximumUsed;
        Update();
    }

    public void SetMinimum()
    {
        float.TryParse(minimumUIE.GetText(), out varComponent.minimum);
        Update();
    }

    public void SetMaximum()
    {
        float.TryParse(maximumUIE.GetText(), out varComponent.maximum);
        Update();
    }
}
