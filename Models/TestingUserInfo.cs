using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Информация о пользователе, участвующем в процессе наладки
    /// </summary>    
    public class TestingUserInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public const string TestingTimePrefix = ":";
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="userInfoStr">Имя пользователя, либо строка в формате UserName:TestingTime</param>
        public TestingUserInfo(string userInfoStr)
        {
            string[] userInfo = userInfoStr.Split(new string[] { TestingTimePrefix }, StringSplitOptions.RemoveEmptyEntries);
            UserName = userInfo[0];
            if (userInfo.Length > 1)
            {
                long.TryParse(userInfo[1], out testingTime);
            }
            UserName = userName;
        }
        #endregion
        
        private string userName;
        private long testingTime;

        /// <summary>
        /// Имя пользователя (логин)
        /// </summary>
        public string UserName
        {
            get { return userName; }
            private set
            {
                if (userName != value)
                {
                    userName = value;                    
                }
            }
        }
        /// <summary>
        /// Оценка времени, затраченного на наладку этим пользователем
        /// </summary>
        public long TestingTime
        {
            get { return testingTime; }
            set
            {
                if (testingTime != value)
                {
                    testingTime = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var userInfo = obj as TestingUserInfo;
            if (userInfo == null)
                return false;
            return UserName == userInfo.UserName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return UserName + TestingTimePrefix + TestingTime;
        }
    }
    /// <summary>
    /// 
    /// </summary>    
    public class TestingUserInfoList : List<TestingUserInfo>
    {
        /// <summary>
        /// 
        /// </summary>
        public const string UserInfoSeparator = "|";
        
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="usersInfoString">Строка в формате UserName:TestingTime|UserName2:TestingTime2|...</param>
        public TestingUserInfoList()
            : base()
        {
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="usersInfoString">Строка в формате UserName:TestingTime|UserName2:TestingTime2|...</param>
        public TestingUserInfoList(string usersInfoString) : this()            
        {
            string[] userInfoStrList = usersInfoString.Split(new string[] { UserInfoSeparator }, StringSplitOptions.RemoveEmptyEntries);                
            foreach (var userStr in userInfoStrList)
            {
                this.Add(new TestingUserInfo(userStr));
            }            
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool ContainsUser(string userName)
        {
            return this.Any(user => user.UserName == userName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public TestingUserInfo GetUser(string userName)
        {
            return this.FirstOrDefault(user => user.UserName == userName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Count; i++)
            {
                var userInfo = this[i];
                result += userInfo.ToString() + (i < Count - 1 ? UserInfoSeparator : "");
            }      
            return result;
        }        
    }
}
