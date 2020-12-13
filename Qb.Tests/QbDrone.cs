using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClassBeingTested = Chen.Qb.QbDrone;

namespace Chen.Qb.Tests
{
    [TestClass]
    public class QbDrone
    {
        [TestMethod]
        public void DebugCheck_Toggled_ReturnsFalse()
        {
            bool result = ClassBeingTested.DebugCheck();

            Assert.IsFalse(result);
        }
    }
}