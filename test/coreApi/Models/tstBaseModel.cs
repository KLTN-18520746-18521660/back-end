using coreApi.Models;
using NUnit.Framework;

namespace test.coreApi
{
    [TestFixture]
    public class tstBaseModel
    {
        private BaseModel baseModel;
        [SetUp]
        public void Setup()
        {
            baseModel = new BaseModel();
        }
        [Test]
        public void InitBaseModel()
        {
            Assert.AreEqual("BaseModel", baseModel.ModelName);
        }
        [Test]
        public void CloneBaseModel()
        {
            BaseModel clone = (BaseModel)baseModel.Clone();
            baseModel = null;
            Assert.NotNull(clone);
            Assert.AreEqual("{}", clone.ToJsonString());
        }
        [Test]
        public void InitFromJsonString()
        {
            baseModel.FromJsonString("{\"fakeProperty\":\"fakeValue\"}");
            Assert.False(baseModel.InitFromObjecJson());
        }
    }
}