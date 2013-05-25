using Android.Locations;

namespace MapsAndLocationDemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Android.App;
    using Android.OS;
    using Android.Util;
    using Android.Widget;

    using Android.Gms.Common;
    using Android.Gms.Location;

    using ILocationListener = Android.Gms.Location.ILocationListener;

    using LocationDemo;

    [Activity(Label = "@string/activity_label_location", MainLauncher = true)]
    public class LocationActivity : Activity, ILocationListener, 
        IGooglePlayServicesClientConnectionCallbacks, IGooglePlayServicesClientOnConnectionFailedListener
    {
        private LocationClient _locClient;

        public void OnLocationChanged(Location location)
        {
            TextView locationText = FindViewById<TextView>(Resource.Id.locationTextView);

            locationText.Text = String.Format("Latitude = {0}, Longitude = {1}, Accuracy = {2}, Provider = {3}", 
                                              location.Latitude, location.Longitude, location.Accuracy, location.Provider);

            // demo geocoder

            new Thread(() =>{
                Geocoder geocdr = new Geocoder(this);

                IList<Address> addresses = geocdr.GetFromLocation(location.Latitude, location.Longitude, 5);

                RunOnUiThread(() =>{
                    TextView addrText = FindViewById<TextView>(Resource.Id.addressTextView);

                    addresses.ToList().ForEach((addr) => addrText.Append(addr.ToString() + "\r\n\r\n"));
                });
            }).Start();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.LocationView);

            // use location service directly 
            _locClient = new LocationClient(this, this, this);
            _locClient.Connect();
        }

        #region IGooglePlayServicesClientConnectionCallbacks implementation

        public void OnConnected(Bundle p0)
        {
            Log.Warn("LocationDemo", "Location client connected.");
            RequestLocationIfConnected();
        }

        public void OnDisconnected()
        {
            Log.Warn("LocationDemo", "Location client disconnected.");
        }

        #endregion

        #region IGooglePlayServicesClientOnConnectionFailedListener implementation

        public void OnConnectionFailed(ConnectionResult p0)
        {
            Log.Warn("LocationDemo", "Could not connect a location client.");
        }

        #endregion

        protected override void OnPause()
        {
            base.OnPause();

            _locClient.RemoveLocationUpdates(this);
        }

        protected override void OnResume()
        {
            base.OnResume();

            RequestLocationIfConnected();
        }

        protected override void OnDestroy()
        {
            _locClient.Disconnect();

            base.OnDestroy();
        }

        private void RequestLocationIfConnected()
        {
            if (!_locClient.IsConnected)
            {
                Log.Warn("LocationDemo", "Not connect yet.");
                return;
            }

            var request = LocationRequest.Create()
                .SetPriority(LocationRequest.PriorityBalancedPowerAccuracy)
                .SetInterval(2000);

            _locClient.RequestLocationUpdates(request, this);
        }

    }
}
