using Windows.UI.Notifications;
using System.Drawing;
using Windows.Data.Xml.Dom;
using System.IO;
using System.Drawing.Imaging;

namespace PreventReboot
{
    // Add Reference: System.Runtime.dll
    // https://docs.microsoft.com/en-us/previous-versions/windows/apps/jj856306(v=win.10)#consuming_standard_windows_runtime_types

    public class WindowsNotification
    {
        // ----- ToastImageAndText02 -----
        // <toast>
        //     <visual>
        //         <binding template="ToastImageAndText02">
        //             <image id="1" src=""/>
        //             <text id="1"></text>
        //             <text id="2"></text>
        //         </binding>
        //     </visual>
        // </toast>

        public string AppUserModelID { get; set; }

        public WindowsNotification(string appUserModelID)
        {
            this.AppUserModelID = appUserModelID;
        }

        public void Show(string title, string message)
        {
            var template = ToastTemplateType.ToastImageAndText02;
            var content = ToastNotificationManager.GetTemplateContent(template);
            //string xml = content.GetXml();

            XmlElement image = (XmlElement)content.GetElementsByTagName("image").Item(0);
            //XmlAttribute placement = content.CreateAttribute("placement");
            //placement.Value = "appLogoOverride";
            //image.SetAttributeNode(placement);

            var appIconImagePath = Path.Combine(Path.GetTempPath(), "app_icon.png");
            using (Image img = new Bitmap(Properties.Resources.app_icon))
            {
                img.Save(appIconImagePath, ImageFormat.Png);
            }

            XmlAttribute src = (XmlAttribute)image.Attributes.GetNamedItem("src");
            src.Value = appIconImagePath;

            XmlNodeList nodes = content.GetElementsByTagName("text");
            ((XmlElement)nodes.Item(0)).AppendChild(content.CreateTextNode(title));
            ((XmlElement)nodes.Item(1)).AppendChild(content.CreateTextNode(message));

            ToastNotification toast = new ToastNotification(content);
            var notifier = ToastNotificationManager.CreateToastNotifier(this.AppUserModelID);
            notifier.Show(toast);
        }

    }
}
