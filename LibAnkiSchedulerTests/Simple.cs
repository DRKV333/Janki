using LibAnkiScheduler;
using NUnit.Framework;

namespace LibAnkiSchedulerTests
{
    public class Simple
    {
        [Test]
        public void Name()
        {
            PythonScheduler scheduler = new PythonScheduler();

            Assert.AreEqual("std2", scheduler.Name);
        }
    }
}