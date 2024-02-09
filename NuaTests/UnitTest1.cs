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
            Assert.AreEqual(_globalRuntime.Evaluate("114514-514"), new NuaNumber(114000));
        }

        [TestMethod]
        public void TestRecFib()
        {
            string code =
                """
                fib = func(index) {
                  if index < 2 {
                    1
                  } else {
                    fib(index - 1) + fib(index - 2)
                  }
                }

                fib(15)
                """;

            Assert.AreEqual(_globalRuntime.Evaluate(code), new NuaNumber(987));
        }

        [TestMethod]
        public void TestMetaTable()
        {
            string code =
                """
                t1 = { "testkey": "test value" }

                t2 = { }
                t2.__meta_table = {
                  __get: t1
                }

                t3 = { }
                t3.__meta_table = {
                  __set: func() { }
                }
                
                t4 = { }
                t4.__meta_table = {
                  __set: func(t, key, value) {
                    table.raw_set(t, key, 114514)
                  }
                }
                
                t3.abc = "test text"
                t4.abc = "test text"
                """;

            _globalRuntime.Evaluate(code);

            Assert.AreEqual(_globalRuntime.Evaluate("t2.testkey"), new NuaString("test value"));
            Assert.AreEqual(_globalRuntime.Evaluate("t3.abc"), null);
            Assert.AreEqual(_globalRuntime.Evaluate("t4.abc"), new NuaNumber(114514));
        }
    }
}