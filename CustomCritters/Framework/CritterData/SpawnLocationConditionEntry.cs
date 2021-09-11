using System;
using System.Collections.Generic;
using System.Reflection;
using StardewValley;

namespace CustomCritters.Framework.CritterData
{
    internal class SpawnLocationConditionEntry
    {
        public bool Not { get; set; } = false;

        public double Chance { get; set; } = 1.0;
        public string Variable { get; set; }
        public bool RequireNotNull { get; set; }
        public string Is { get; set; }
        public string ValueEquals { get; set; }

        public string ChildrenCombine { get; set; } = "and";
        public List<SpawnLocationConditionEntry> Children { get; set; } = new();

        public bool Check(object obj)
        {
            bool ret = true;

            if (this.Children.Count > 0)
            {
                if (this.ChildrenCombine != "and")
                    ret = false;

                int totalMet = 0;
                foreach (var child in this.Children)
                {
                    bool childCheck = child.Check(obj);
                    if (childCheck)
                        ++totalMet;

                    ret = this.ChildrenCombine switch
                    {
                        "and" => ret && childCheck,
                        "or" => ret || childCheck,
                        "xor" => ret ^ childCheck,
                        _ => ret
                    };
                }

                if (this.ChildrenCombine.StartsWith("atleast"))
                {
                    ret = totalMet >= int.Parse(this.ChildrenCombine.Substring(7));
                }
                else if (this.ChildrenCombine.StartsWith("exactly"))
                {
                    ret = totalMet == int.Parse(this.ChildrenCombine.Substring(7));
                }
                else if (this.ChildrenCombine != "and" && this.ChildrenCombine != "or" && this.ChildrenCombine != "xor")
                {
                    throw new ArgumentException("Bad ChildrenCombine: " + this.ChildrenCombine);
                }
            }
            else
            {
                if (this.Chance != 1.0 && Game1.random.NextDouble() > this.Chance)
                    ret = false;
                if (!string.IsNullOrEmpty(this.Variable))
                {
                    string[] tokens = this.Variable.Split('.');

                    object o = obj;
                    foreach (string token in tokens)
                    {
                        if (o == null)
                            break;
                        var f = o.GetType().GetField(token, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (f == null)
                        {
                            o = null;
                            break;
                        }

                        o = f.GetValue(o);
                    }

                    if (o != null)
                    {
                        if (!string.IsNullOrEmpty(this.Is) && !o.GetType().IsInstanceOfType(Type.GetType(this.Is)))
                            ret = false;
                        else if (!string.IsNullOrEmpty(this.ValueEquals) && !o.ToString().Equals(this.ValueEquals))
                            ret = false;
                    }
                    else if (this.RequireNotNull)
                        ret = false;
                }
            }

            if (this.Not)
                ret = !ret;

            return ret;
        }
    }
}
