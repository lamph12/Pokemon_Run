// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("CLo5Ggg1PjESvnC+zzU5OTk9ODuwBXlPQxkbvbTn19SF5Z6iwXbkP7o5NzgIujkyOro5OTj8RUGFd0ZG6Kz+bSIDMILgyZOLlwPWolJOCCYniqT3VbejB5Pckag4NIgWQ4oSzxMxWDNyczbahuFDFlMYG3Y2yaWMkfvYcJEYb3phxVPXMeZXA7h4LtnT8BZ0Vb9ulN8yuSvghqSMkSy2P/0ffpVlzef8dVLgwA3JvV3PvHXNlKgqYDdKkQNKiXEUkMT9C+sCSOIomvTZ0miwTMhpj3Q20k90jLr9vQRWr2hYmJSd7T1Ap1oWkJbhkD73eckoCtgmec6WoKLWRkoWvcK5HAxFqCM31KHyj71WtD6/L3RgkTQQI8fo2+cwg9eQuzo7OTg5");
        private static int[] order = new int[] { 0,8,8,3,7,6,12,7,8,10,11,13,12,13,14 };
        private static int key = 56;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
