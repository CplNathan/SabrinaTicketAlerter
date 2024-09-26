using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Helpers
{
    public static class UserDataHelper
    {
        public static string UserDataFolderName => "sabrinaUserData";

        public static string DocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static string UserDataPath => Path.Combine(DocumentsPath, UserDataFolderName);
    }
}
