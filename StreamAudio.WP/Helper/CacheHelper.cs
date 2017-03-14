
using StreamAudio.WP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StreamAudio.WP.Helper
{
    public class CacheHelper
    {
        public static void createlstsong(List<AudioModel> lstsong) //ham nay tui se tao 1 listsong gom cac thuoc tinh
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists("_listsong"))
                    isoStore.DeleteFile("_listsong");

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("_listsong", FileMode.CreateNew, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        writer.Write(SerializeListToXml(lstsong));
                    }
                }
            }
        }
        public static string SerializeListToXml(List<AudioModel> List) // parse qua xml
        {
            try
            {
                XmlSerializer xmlIxer = new XmlSerializer(typeof(List<AudioModel>));
                var writer = new StringWriter();
                xmlIxer.Serialize(writer, List);
                System.Diagnostics.Debug.WriteLine(writer.ToString());
                return writer.ToString();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc);
                return String.Empty;
            }
        }

    }
}
