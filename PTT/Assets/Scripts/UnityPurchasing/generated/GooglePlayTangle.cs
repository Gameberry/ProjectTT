// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Rla53P9OFiLVC4Owcgg1D7I3azoUnVa3/Lfjnk9pa1l7+qWQ+lw8YokNE0dauHgMyGLhNH8QDXNnEVEow0BOQXHDQEtDw0BAQfI8OBWz5TJzEmTNgU0zl8z/nNbbRCNnnjm///ibsVMXduw48qDP0JSoKC9rNWQZccNAY3FMR0hrxwnHtkxAQEBEQUJyvLp3tupnwrnCqNQjzTJpm4uk7f/nk9r/DNzB0Z9V7p6fjuY1/m79PtXIlIgQ4DRlB/Dn+iMV4ujs5RcO4UNuStp5XTWF0f2MPiWWBNzdObAFrMdvJiZXvX6f7HFSjwecxZ6GoHNprsRmqqNt/HSxEzIKyu1PHWQEPyV3TTGGFtOnZspEcHfbw8GuV4DeGYdTj2QhxkNCQEFA");
        private static int[] order = new int[] { 5,2,4,4,12,10,10,12,12,12,10,13,12,13,14 };
        private static int key = 65;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
