using System;
using Moq;
using NUnit.Framework;
using PasswordChange.ViewModel.Services;

namespace PasswordChange.ViewModel.Tests
{
    [TestFixture]
    public class TestPasswordChangeViewModel
    {
        [Test]
        public void Constructor_HappyPath_DefaultsUserName()
        {
            var mockHelper = new Mock<IHelperService>();
            mockHelper.Setup(s => s.GetDefaultUserName())
                .Returns("User_Name");

            var vm = new PasswordChangeViewModel(mockHelper.Object);

            Assert.That(vm.UserName, Is.EqualTo("User_Name"));
        }

        [Test]
        public void Constructor_NullHelperService_ThrowsArgumentNullException()
        {
            Assert.That(() => new PasswordChangeViewModel(null), Throws.TypeOf(typeof(ArgumentNullException)));
        }
    }
}
