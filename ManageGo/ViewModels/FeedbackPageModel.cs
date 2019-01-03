﻿using System;
using System.Net.Http;
using System.Text;
using FreshMvvm;
using Xamarin.Essentials;
namespace ManageGo
{
    public class FeedbackPageModel : FreshBasePageModel
    {

        public string SelectedTopic { get; set; }
        public string Message { get; set; }
        public bool HamburgerIsVisible { get; set; } = true;

        private readonly string receivingEmail = "naftaliklein@gmail.com";//"nklein@managego.com";

        public FreshAwaitCommand OnMasterMenuTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    App.MenuIsPresented = true;
                    HamburgerIsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSubmitButtonTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    if (string.IsNullOrWhiteSpace(Message) || string.IsNullOrWhiteSpace(SelectedTopic))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please select a topic and write your message", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    var finalMessage = $" Topic: {SelectedTopic}\n\r" +
                    $" Message: {Message}\n\r" +
                    "\n\r------------\n\r" +
                    "USER INFO\n\r" +
                    "------------\n\r" +
                    $" User id: {App.UserInfo.UserID}\n\r" +
                    $" Username: {App.UserInfo.UserFirstName + " " + App.UserInfo.UserLastName}\n\r" +
                    $" User email: {App.UserInfo.UserEmailAddress}\n\r" +
                    $" PMC: {App.PMCName}\n\r" +
                    "\n\r------------\n\r" +
                    "DEVICE INFO\n\r" +
                    "------------\n\r" +
                    $" Device Manufacturer: {DeviceInfo.Manufacturer}\n\r" +
                    $" Device Model: {DeviceInfo.Model}\n\r" +
                    $" Device Name: {DeviceInfo.Name}\n\r" +
                    $" Device Platform: {DeviceInfo.Platform}\n\r" +
                    $" Device OS Version: {DeviceInfo.VersionString}\n\r" +
                    $" Device Type: {DeviceInfo.DeviceType}\n\r";


                    var stringToSend = "{\"serverId\": \"12003\",\"APIKey\": \"Hb35KdWo4g2A7Qwc8SFa\",\"Messages\": [  " +
                        "{ \"To\": [ {\"emailAddress\": \"" +
                        $"{receivingEmail}" +
                        "\",\"friendlyName\": \"App feedback ManageGo\" }  ],    \"From\": {  \"emailAddress\": \"" +
                        $"{receivingEmail}" +
                        "\", \"friendlyName\": \"" +
                        "App feedback ManageGo" +
                        "\"  }, \"Subject\": \"" +
                        $"{SelectedTopic}" +
                        "\",\"TextBody\": \"" +
                        $"{finalMessage}" +
                        "\"}]}";

                    try
                    {
                        var content = new StringContent(stringToSend
                            , encoding: Encoding.UTF8,
                            mediaType: "application/json");
                        var response = await Services.DataAccess.client.PostAsync("https://inject.socketlabs.com/api/v1/email", content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Unable to send message.{Environment.NewLine + response.ReasonPhrase}");
                        }
                        Message = null;
                        await CoreMethods.DisplayAlert("Success!", "We received your feedback", "OK");
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong!", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }
                });
            }
        }
    }
}
