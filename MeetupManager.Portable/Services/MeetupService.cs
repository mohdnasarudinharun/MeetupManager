/*
 * MeetupManager:
 * Copyright (C) 2013 Refractored LLC: 
 * http://github.com/JamesMontemagno
 * http://twitter.com/JamesMontemagno
 * http://refractored.com
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using MeetupManager.Portable.Interfaces;
using System.Threading.Tasks;
using MeetupManager.Portable.Services.Responses;
using System.Net.Http;
using Newtonsoft.Json;
using MeetupManager.Portable.Helpers;
using MeetupManager.Portable.Models;

namespace MeetupManager.Portable.Services
{
	public class MeetupService : IMeetupService
	{
		#region IMeetupService implementation

		//private const string GroupUrlName = "<YOUR GROUP NAME HERE>";
		private const string GroupUrlName = "SeattleMobileDevelopers";
		//private const string ApiKey = "&key=<YOUR API KEY HERE>";

		private const string ClientId = "";
		private const string ClientSecrete = "";

		private const string GetEventsUrl = @"https://api.meetup.com/2/events?group_urlname=" + GroupUrlName + "&offset={0}&status=upcoming,past&desc=true&access_token={1}";
		private const string GetRSVPsUrl = @"https://api.meetup.com/2/rsvps?offset={0}&event_id={1}&order=name&rsvp=yes&access_token={2}";
		private const string GetUserUrl = @"https://api.meetup.com/2/member/self?access_token={0}";

		private const string RefreshUrl = "https://secure.meetup.com/oauth2/access?client_id={0}&client_secret={1}&grant_type=refresh_token&refresh_token={2}";

		public async Task<EventsRootObject> GetEvents (int skip)
		{
			await RenewAccessToken ();

			var httpClient = new HttpClient ();
			var request = string.Format (GetEventsUrl, "0", Settings.AccessToken);
			var response = await httpClient.GetStringAsync (request);
			return await JsonConvert.DeserializeObjectAsync<EventsRootObject> (response);
		}

		public async Task<RSVPsRootObject> GetRSVPs(string eventId, int skip)
		{
			await RenewAccessToken ();

			var httpClient = new HttpClient ();
			var request = string.Format (GetRSVPsUrl, "0", eventId, Settings.AccessToken);
			var response = await httpClient.GetStringAsync (request);
			return await JsonConvert.DeserializeObjectAsync<RSVPsRootObject> (response);

		}

		private const string clientId = "kgqtisiigj7mpbpbfs1ei7s2h0";
		private const string clientSecret = "g4k3oiourvnos0nf9varqt5eaf";
		public async Task<bool> RenewAccessToken()
		{
			if (string.IsNullOrWhiteSpace (Settings.AccessToken))
				return false;

			if (DateTime.UtcNow.Ticks > Settings.KeyValidUntil)
				return true;

			var httpClient = new HttpClient ();
			var request = string.Format (RefreshUrl, clientId, clientSecret, Settings.RefreshToken);

			try
			{
				var response = await httpClient.GetStringAsync (request);
				var refreshResponse = await JsonConvert.DeserializeObjectAsync<RefreshRootObject> (response);
				Settings.AccessToken = refreshResponse.AccessToken;
				Settings.KeyValidUntil = DateTime.UtcNow.Ticks + refreshResponse.ExpiresIn;
				Settings.RefreshToken = refreshResponse.RefreshToken;
			}
			catch(Exception ex) {
				return false;
			}

			return true;
		}


		public async Task<LoggedInUser> GetCurrentMember ()
		{
			await RenewAccessToken();
			var httpClient = new HttpClient ();
			var request = string.Format (GetUserUrl, Settings.AccessToken);
			var response = await httpClient.GetStringAsync (request);
			return await JsonConvert.DeserializeObjectAsync<LoggedInUser> (response);
		}
		#endregion


	}
}

