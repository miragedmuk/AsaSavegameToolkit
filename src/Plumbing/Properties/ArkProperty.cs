using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsaSavegameToolkit.Plumbing.Properties
{
    public abstract class ArkProperty
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public class ArkProperty<T> : ArkProperty
    {
        public T Value { get; set; }
        public override string ToString()
        {
            return $"{Name} ({typeof(T).Name}): {Value}";
        }
    }
}
