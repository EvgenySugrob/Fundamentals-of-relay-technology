using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuItem
{
    public string Label;
    public Action Action;
    public Func<string> DynamicLabel;
    public bool CloseOnClick = true;

    public string GetLabel()
    {
        return DynamicLabel != null ? DynamicLabel() : Label;
    }
}
