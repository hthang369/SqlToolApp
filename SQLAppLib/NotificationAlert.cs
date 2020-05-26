using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLAppLib
{
    /// <summary>This class contains the properties and methods used to create the standard Aileron alert message.</summary>  
    /// <remarks>  
    /// <para>Date Created: Jun 2018</para>  
    /// <para>Revision History: Original Version</para>  
    /// </remarks>  
    public class NotificationAlert
    {
        #region Properties  

        /// <summary>Gets the notification service used to render toast messages.</summary>  
        private static NotificationService _service;

        public static NotificationService Service
        {
            get { return AileronAlertMessage._service; }
            private set { AileronAlertMessage._service = value; }
        }

        #endregion

        #region Constructors & Destructors  

        /// <summary>Initialize the shared object instance.</summary>  
        static NotificationAlert()
        {
            NotificationAlert.Service = new NotificationService()
            {
                CustomNotificationPosition = NotificationPosition.BottomRight,
                PredefinedNotificationTemplate = NotificationTemplate.ShortHeaderAndLongText,
                UseWin8NotificationsIfAvailable = false
            };

        }

        #endregion Constructors & Destructors  

        #region Public Methods  

        /// <summary>Display a predefined status bar alert message.</summary>  
        /// <param name="typeOfAlert">Sets predefined message that should be displayed.</param>  
        public static void ShowAlert(AlertMessageType typeOfAlert)
        {
            switch (typeOfAlert)
            {
                case AlertMessageType.DeleteWasSuccessful:
                    NotificationAlert.ShowAlert(
                        alertMessage: AileronCaption.DeleteWasSuccessful,
                        alertImage: AileronImage.GetImageSource(
                            name: ImageName.StatusFlagGreen,
                            size: ImageSize.Size32));
                    break;

                case AlertMessageType.InternetIsOffline:
                    NotificationAlert.ShowAlert(
                        alertMessage: AileronCaption.InternetIsOffline,
                        alertImage: AileronImage.GetImageSource(
                            name: ImageName.GlobeError,
                            size: ImageSize.Size16));
                    break;

                case AlertMessageType.InternetIsOnline:
                    NotificationAlert.ShowAlert(
                        alertMessage: AileronCaption.InternetIsOnline,
                        alertImage: AileronImage.GetImageSource(
                            name: ImageName.GlobeConnected,
                            size: ImageSize.Size16));
                    break;

                case AlertMessageType.SaveWasSuccessful:
                    NotificationAlert.ShowAlert(
                        alertMessage: AileronCaption.SaveWasSuccessful,
                        alertImage: AileronImage.GetImageSource(
                            name: ImageName.StatusFlagGreen,
                            size: ImageSize.Size32));
                    break;
            }

        }

        /// <summary>Display a free-form status bar alert message.</summary>  
        /// <param name="alertMessage">Sets the message that should be displayed.</param>  
        /// <param name="alertImage">Sets the image that should be displayed with the message.</param>  
        public static void ShowAlert(
            string alertMessage,
            ImageSource alertImage)
        {
            INotification p_Service = AileronAlertMessage.Service.CreatePredefinedNotification(
                text1: AileronApplication.ApplicationTitle,
                text2: alertMessage,
                text3: null,
                image: alertImage);
            p_Service.ShowAsync();

        }

        /// <summary>Show an alert message denoting the Internet network status.</summary>  
        public static void ShowNetworkStatus()
        {
            if (AileronApplication.IsNetworkAvailable == true)
                AileronAlertMessage.ShowAlert(typeOfAlert: AlertMessageType.InternetIsOnline);
            else
                AileronAlertMessage.ShowAlert(typeOfAlert: AlertMessageType.InternetIsOffline);

        }

        #endregion Public Methods  

    }
}
