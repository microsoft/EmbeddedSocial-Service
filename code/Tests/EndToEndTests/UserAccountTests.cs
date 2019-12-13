using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SocialPlus;
using SocialPlus.Models;

namespace SocialPlus.Server.EndToEndTests
{
    [TestClass]
    public class UserAccountTests
    {
        UserRequest req;
        string AppHandle = "coins";
        string UserHandle = "alec";

        public UserAccountTests()
        {
            req = new UserRequest()
            {
                UserHandle = this.UserHandle,
                AppHandle = this.AppHandle,
                Agent = null,
                UserSessionExpirationTime = 0,
                UserSessionSignature = "OK",
                InstanceHandle = null,
                Location = null,
                Time = DateTime.Now.ToFileTime()
            };
        }

        [TestMethod]
        // Add a topic, then remove it.
        public async Task AddUserAccountTest()
        {
            CreateUserRequest addUserAccountRequest = new CreateUserRequest(req)
            {
                FirstName = "Lenin",
                LastName = "Ravindranath",
                Username = "lenin",
                Email = "slenin@gmail.com",                                
                AccountType = (int)AccountType.Facebook,
                ThirdPartyAccountHandle = "1234567890",
                ThirdPartyAccessToken = "AccessToken",                
                PasswordHash = null,                
            };

            CreateUserResponse addUserAccountResponse = await ServerTask<CreateUserRequest, CreateUserResponse>.PostRequest(addUserAccountRequest);
            Assert.AreEqual(addUserAccountResponse, ResponseCode.Success);  // *#*#* not sure if this is the right comparison ... 
        }
    }
}

    

