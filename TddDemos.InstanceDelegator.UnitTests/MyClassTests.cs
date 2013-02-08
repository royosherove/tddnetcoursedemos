using NUnit.Framework;

namespace Refactored
{
    [TestFixture]
    public class MyClassTests
    {
        [Test]
        public void GetConfig_WhenCalled_CanBeFaked()
        {
            TestableMyClass mc= new TestableMyClass();

            mc.FakeMachineName = "abc";
            mc.FakeMaxLen = 1;

            bool result = mc.IsMachingNameLongerThanNeeded();

            Assert.True(result);
        }
    }




    class TestableMyClass : MyClass
    {
        public int FakeMaxLen;
        public string FakeMachineName;

        protected override int GetSetting(string machinename)
        {
            return FakeMaxLen;
        }
        protected override string GetMachineName()
        {
            return FakeMachineName;
        }
    }
}
