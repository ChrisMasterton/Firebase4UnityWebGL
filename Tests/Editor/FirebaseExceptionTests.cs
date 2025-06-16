using System;
using NUnit.Framework;
using Firebase.Core;

namespace Firebase.Tests.Editor
{
    public class FirebaseExceptionTests
    {
        [Test]
        public void FirebaseException_BasicConstructor_SetsPropertiesCorrectly()
        {
            var errorCode = "invalid_email";
            var message = "The email address is not valid";
            
            var exception = new FirebaseException(errorCode, message);
            
            Assert.AreEqual(errorCode, exception.ErrorCode);
            Assert.AreEqual(message, exception.Message);
            Assert.IsNull(exception.InnerException);
        }

        [Test]
        public void FirebaseException_WithInnerException_SetsPropertiesCorrectly()
        {
            var errorCode = "network_error";
            var message = "Failed to connect to server";
            var innerException = new System.Net.WebException("Connection timeout");
            
            var exception = new FirebaseException(errorCode, message, innerException);
            
            Assert.AreEqual(errorCode, exception.ErrorCode);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }

        [Test]
        public void FirebaseException_InheritsFromException()
        {
            var exception = new FirebaseException("test", "test message");
            
            Assert.IsInstanceOf<Exception>(exception);
        }

        [Test]
        public void FirebaseException_CanBeCaught()
        {
            var errorCode = "test_error";
            var message = "Test error message";
            
            Assert.Throws<FirebaseException>(() => {
                throw new FirebaseException(errorCode, message);
            });
        }

        [Test]
        public void FirebaseException_ErrorCodeAccessible()
        {
            var errorCode = "permission_denied";
            var exception = new FirebaseException(errorCode, "Access denied");
            
            try
            {
                throw exception;
            }
            catch (FirebaseException ex)
            {
                Assert.AreEqual(errorCode, ex.ErrorCode);
            }
        }
    }
}