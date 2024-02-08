using Nua;
using Nua.Types;

namespace NuaTests
{
    [TestClass]
    public class UnitTest1
    {
        readonly NuaRuntime _globalRuntime = new NuaRuntime();

        [TestMethod]
        public void TestAdd1()
        {
            Assert.AreEqual(_globalRuntime.Evaluate("114000+514"), new NuaNumber(114514));
        }
    }
}