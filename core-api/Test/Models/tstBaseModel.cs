using core_api.Models;
using Xunit;
using System.IO;
using System;

namespace core_api.Test
{
    public class tstBaseModel
    {
        [Fact]
        public void InitBaseModel()
        {
            BaseModel baseModel = new BaseModel();
            Assert.Equal("BaseModel", baseModel.ModelName);
        }

        [Fact]
        public void CloneBaseModel()
        {
            BaseModel baseModel = new BaseModel();
            BaseModel clone = (BaseModel)baseModel.Clone();
            baseModel = null;
            Assert.NotNull(clone);
            Assert.Equal("{}", clone.ToJsonString());
        }

        [Fact]
        public void InitFromJsonString()
        {
            BaseModel baseModel = new BaseModel();
            baseModel.FromJsonString("{\"fakeProperty\":\"fakeValue\"}");
            Assert.False(baseModel.InitFromObjecJson());
        }
    }
}