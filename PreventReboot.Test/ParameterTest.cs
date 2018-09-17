using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PreventReboot.Test
{
    [TestClass]
    public class ParameterTest
    {
        private StringWriter _consoleBuffer;

        [TestInitialize]
        public void TestInitialize()
        {
            this._consoleBuffer = new StringWriter();
            Console.SetOut(this._consoleBuffer);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestMethod1()
        {
            int result = Program.Main(new string[0]);
            var output = this._consoleBuffer.GetStringBuilder().ToString();

            //Assert.AreEqual(output, "Hello, World!");
        }
    }
}
