using System;

namespace FakeClosure
{
    public class FakeClosure
    {
        public FakeClosure(object[] Constants, object[] Locals)
        {
            this.Constants = Constants;
            this.Locals = Locals;
        }

        public readonly object[] Constants;
        public readonly object[] Locals;
    }
}
