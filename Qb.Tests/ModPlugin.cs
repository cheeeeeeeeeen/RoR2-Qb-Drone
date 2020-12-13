using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClassBeingTested = Chen.Qb.ModPlugin;

namespace Chen.Qb.Tests
{
    [TestClass]
    public class ModPlugin
    {
        [TestMethod]
        public void DebugCheck_Toggled_ReturnsFalse()
        {
            bool result = ClassBeingTested.DebugCheck();

            Assert.IsFalse(result);
        }
    }
}