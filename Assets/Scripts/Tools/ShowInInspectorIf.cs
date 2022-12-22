using System;

namespace RaceManager.Tools
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowInInspectorIf : HideInInspectorIf
    {
        public ShowInInspectorIf(string propertyToCheck) : base(propertyToCheck, false)
        {
        }
    }
}

