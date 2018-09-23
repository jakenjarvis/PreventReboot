using Windows.UI.Notifications;
using System.Drawing;
using Windows.Data.Xml.Dom;

namespace PreventReboot
{
    // Add Reference: System.Runtime.dll
    // https://docs.microsoft.com/en-us/previous-versions/windows/apps/jj856306(v=win.10)#consuming_standard_windows_runtime_types

    public class WindowsNotification
    {
        // ----- ToastImageAndText01 -----
        // <toast>
        //     <visual>
        //         <binding template="ToastImageAndText01">
        //             <image id="1" src=""/>
        //             <text id="1"></text>
        //         </binding>
        //     </visual>
        // </toast>

        public string AppUserModelID { get; set; }

        public WindowsNotification(string appUserModelID)
        {
            this.AppUserModelID = appUserModelID;
        }

        public void Show(string message)
        {
            var template = ToastTemplateType.ToastImageAndText01;
            var content = ToastNotificationManager.GetTemplateContent(template);
            //string xml = content.GetXml();

            XmlElement image = (XmlElement)content.GetElementsByTagName("image").Item(0);
            //XmlAttribute placement = content.CreateAttribute("placement");
            //placement.Value = "appLogoOverride";
            //image.SetAttributeNode(placement);

            //XmlAttribute src = (XmlAttribute)image.Attributes.GetNamedItem("src");
            //src.Value = "Assets/Images/app_icon.png";

            //var bmp = new Bitmap(Properties.Resources.app_icon);
            //var path = Path.GetTempPath();
            //Properties.Resources.app_icon.Save(path);

            // Assets/Images/YourOwnImage.png

            XmlElement text = (XmlElement)content.GetElementsByTagName("text").Item(0);
            text.AppendChild(content.CreateTextNode(message));

            ToastNotification toast = new ToastNotification(content);
            var notifier = ToastNotificationManager.CreateToastNotifier(this.AppUserModelID);
            notifier.Show(toast);
        }

    }
}
