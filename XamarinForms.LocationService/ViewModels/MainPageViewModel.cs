﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinForms.LocationService.Messages;
using XamarinForms.LocationService.Utils;

namespace XamarinForms.LocationService.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        #region vars
        private double latitude;
        private double longitude;
        public string userMessage;
        public bool startEnabled;
        public bool stopEnabled;
        #endregion vars

        private ObservableCollection<MyClass> myVar;

        public ObservableCollection<MyClass> MyProperty
        {
                    
                get => myVar;
                set => SetProperty(ref myVar, value);

        }


        #region properties
        public double Latitude
        {
            get => latitude;
            set => SetProperty(ref latitude, value);
        }
        public double Longitude
        {
            get => longitude;
            set => SetProperty(ref longitude, value);
        }
        public string UserMessage
        {
            get => userMessage;
            set => SetProperty(ref userMessage, value);
        }
        public bool StartEnabled
        {
            get => startEnabled;
            set => SetProperty(ref startEnabled, value);
        }
        public bool StopEnabled
        {
            get => stopEnabled;
            set => SetProperty(ref stopEnabled, value);
        }
        #endregion properties

        #region commands
        public Command StartCommand { get; }
        public Command EndCommand { get; }
        #endregion commands

        readonly ILocationConsent locationConsent;

        public MainPageViewModel()
        {
            MyProperty = new ObservableCollection<MyClass>();
            locationConsent = DependencyService.Get<ILocationConsent>();
            StartCommand = new Command(() => OnStartClick());
            EndCommand = new Command(() => OnStopClick());
            HandleReceivedMessages();
            locationConsent.GetLocationConsent();
            StartEnabled = true;
            StopEnabled = false;
            ValidateStatus();
        }

        public void OnStartClick()
        {
            Start();
        }

        public void OnStopClick()
        {
            var message = new StopServiceMessage();
            MessagingCenter.Send(message, "ServiceStopped");
            UserMessage = "Location Service has been stopped!";
            SecureStorage.SetAsync(Constants.SERVICE_STATUS_KEY, "0");
            StartEnabled = true;
            StopEnabled = false;
        }

        void ValidateStatus() 
        {
            var status = SecureStorage.GetAsync(Constants.SERVICE_STATUS_KEY).Result;
            if (status != null && status.Equals("1")) 
            {
                Start();
            }
        }

        void Start() 
        {
            var message = new StartServiceMessage();
            MessagingCenter.Send(message, "ServiceStarted");
            UserMessage = "Location Service has been started!";
            SecureStorage.SetAsync(Constants.SERVICE_STATUS_KEY, "1");
            StartEnabled = false;
            StopEnabled = true;
        }

        void HandleReceivedMessages()
        {
            MessagingCenter.Subscribe<LocationMessage>(this, "Location", message => {
                Device.BeginInvokeOnMainThread(() => {
                    Latitude = message.Latitude;
                    Longitude = message.Longitude;
                    Longitude = message.Longitude;
                    MyProperty.Add(new MyClass(Latitude,Longitude,DateTime.Now));
                    UserMessage = "Location Updated";
                });
            });
            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message => {
                Device.BeginInvokeOnMainThread(() => {
                    UserMessage = "Location Service has been stopped!";
                });
            });
            MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError", message => {
                Device.BeginInvokeOnMainThread(() => {
                    UserMessage = "There was an error updating location!";
                });
            });
        }

        public class MyClass : INotifyPropertyChanged
        {
            private double lat;
            private double lon;
            private DateTime date;

            public MyClass(double lat, double lon, DateTime date)
            {
                this.lat = lat;
                this.lon = lon;
                this.date = date;
            }

            public double Lat
            {

                get { return lat; }
                set { lat = value; OnPropertyChanged("Lat"); }
            }

            

            public double Lon
            {
                get { return lon; }
                set { lon = value; OnPropertyChanged("Lon"); }
            }

            

            public DateTime Date
            {
                get { return date; }
                set { date = value; OnPropertyChanged("Date"); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs(propertyName));
                }



            }

        }
    }
}
