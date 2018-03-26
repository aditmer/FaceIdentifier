using Microsoft.ProjectOxford.Face;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FaceIdentifier
{
	public partial class MainPage : ContentPage
	{
        MediaFile file = null;
		public MainPage()
		{
			InitializeComponent();
		}

        async void btnTakePhoto_Clicked(object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                DisplayAlert("No Camera", ":( No camera available.", "OK");

                return;
            }

            file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Images",
                Name = "Face.jpg",
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
            });

            imgPhotoToIdentify.Source = ImageSource.FromFile(file.Path);

            if (file == null)
            {
                return;
            }
        }

        async void btnPickPhoto_Clicked(object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            file = await CrossMedia.Current.PickPhotoAsync();

            imgPhotoToIdentify.Source = ImageSource.FromFile(file.Path);

            if (file == null)
            {
                return;
            }
        }

        async void btnIdentifyPhoto_Clicked(object sender, System.EventArgs e)
        {
            var faceServiceClient = new FaceServiceClient(Keys.FaceAPIKey);
            string personGroupId = "celebs";

            if (file != null)
            {
               
                    var faces = await faceServiceClient.DetectAsync(file.GetStream());
                    var faceIds = faces.Select(face => face.FaceId).ToArray();

                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    foreach (var identifyResult in results)
                    {
                        //DisplayAlert("Results!",$"Result of face: {identifyResult.FaceId}","Yay!");
                        if (identifyResult.Candidates.Length == 0)
                        {
                           DisplayAlert("Bummer","No one identified","Ok");
                        }
                        else
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                            DisplayAlert("Found!",$"Identified as {person.Name}","Nice!");
                        }
                    }
                
            }
            else
            {
                DisplayAlert("No Image", "Please pick or take a photo first", "Ok");
            }
        }
    }
}
