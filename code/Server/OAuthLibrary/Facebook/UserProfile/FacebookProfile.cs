// <copyright file="FacebookProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.OAuth
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Class describing a user profile obtained from Facebook.
    /// C# class corresponding to the fields of a profile in Facebook. These fields
    /// are filled based on the JSON response to a Graph-API call for user profile.
    /// These fields were obtained from: https://developers.facebook.com/docs/graph-api/reference/user
    ///
    /// NOTE: Some of the C# types of the fields below do not match the specification. This is due
    /// to some of the complex types used in the specification like VideoUploadLimits or WorkExperience.
    /// However, they should be easy to support by creating the correct C# types corresponding to
    /// these complex Facebook types.
    /// </summary>
    public class FacebookProfile
    {
        /// <summary>
        /// Gets the id of this person's user account. This ID is unique to each app and cannot be used across different apps.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the About Me section of this person's profile
        /// </summary>
        [JsonProperty(PropertyName = "about")]
        public string About { get; internal set; }

        /// <summary>
        /// Gets the person's address
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public string Address { get; internal set; }

        /// <summary>
        /// Gets the age segment for this person expressed as a minimum and maximum age. For example, more than 18, less than 21.
        /// </summary>
        [JsonProperty(PropertyName = "age_range")]
        public string AgeRange { get; internal set; }

        /// <summary>
        /// Gets the person's bio
        /// </summary>
        [JsonProperty(PropertyName = "bio")]
        public string Bio { get; internal set; }

        /// <summary>
        /// Gets the person's birthday. This is a fixed format string, like MM/DD/YYYY.
        /// However, people can control who can see the year they were born separately from the month and day
        /// so this string can be only the year (YYYY) or the month + day (MM/DD)
        /// </summary>
        [JsonProperty(PropertyName = "birthday")]
        public string Birthday { get; internal set; }

        /// <summary>
        /// Gets the the social context for this person
        /// </summary>
        [JsonProperty(PropertyName = "context")]
        public string Context { get; internal set; }

        /// <summary>
        /// Gets the person's local currency information
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; internal set; }

        /// <summary>
        /// Gets the list of devices the person is using. This will return only iOS and Android devices
        /// </summary>
        [JsonProperty(PropertyName = "devices")]
        public List<string> Devices { get; internal set; }

        /// <summary>
        /// Gets the person's education
        /// </summary>
        [JsonProperty(PropertyName = "education")]
        public List<string> Education { get; internal set; }

        /// <summary>
        /// Gets the person's primary email address listed on their profile. This field will not be returned if no valid email address is available
        /// </summary>
        [JsonProperty(PropertyName = "email")]
        public string Email { get; internal set; }

        /// <summary>
        /// Gets the athletes the person likes
        /// </summary>
        [JsonProperty(PropertyName = "favorite_experience")]
        public List<string> FavoriteExperience { get; internal set; }

        /// <summary>
        /// Gets the sports teams the person likes
        /// </summary>
        [JsonProperty(PropertyName = "favorite_teams")]
        public List<string> FavoriteTeams { get; internal set; }

        /// <summary>
        /// Gets the person's first name
        /// </summary>
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; internal set; }

        /// <summary>
        /// Gets the gender selected by this person, male or female. This value will be omitted if the gender is set to a custom value
        /// </summary>
        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; internal set; }

        /// <summary>
        /// Gets the person's hometown
        /// </summary>
        [JsonProperty(PropertyName = "hometown")]
        public string Hometown { get; internal set; }

        /// <summary>
        /// Gets the person's inspirational people
        /// </summary>
        [JsonProperty(PropertyName = "inspirational_people")]
        public List<string> InspirationalPeople { get; internal set; }

        /// <summary>
        /// Gets the install type
        /// </summary>
        [JsonProperty(PropertyName = "install_type")]
        public string InstallType { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the app making the request installed?
        /// </summary>
        [JsonProperty(PropertyName = "installed")]
        public bool Installed { get; internal set; }

        /// <summary>
        /// Gets the genders the person is interested in
        /// </summary>
        [JsonProperty(PropertyName = "interested_in")]
        public List<string> InterestedIn { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this a shared login (e.g. a gray user)
        /// </summary>
        [JsonProperty(PropertyName = "is_shared_login")]
        public bool IsSharedLogin { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the people with large numbers of followers can have
        /// the authenticity of their identity manually verified by Facebook.
        /// This is distinct from the verified field
        /// </summary>
        [JsonProperty(PropertyName = "is_verified")]
        public bool IsVerified { get; internal set; }

        /// <summary>
        /// Gets the Facebook Pages representing the languages this person knows
        /// </summary>
        [JsonProperty(PropertyName = "languages")]
        public List<string> Languages { get; internal set; }

        /// <summary>
        /// Gets the person's last name
        /// </summary>
        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; internal set; }

        /// <summary>
        /// Gets a link to the person's Timeline
        /// </summary>
        [JsonProperty(PropertyName = "link")]
        public string Link { get; internal set; }

        /// <summary>
        /// Gets the person's current location as entered by them on their profile. This field is not related to check-ins
        /// </summary>
        [JsonProperty(PropertyName = "location")]
        public string Location { get; internal set; }

        /// <summary>
        /// Gets the person's locale
        /// </summary>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; internal set; }

        /// <summary>
        /// Gets the what the person is interested in meeting for
        /// </summary>
        [JsonProperty(PropertyName = "meeting_for")]
        public List<string> MeetingFor { get; internal set; }

        /// <summary>
        /// Gets the person's middle name
        /// </summary>
        [JsonProperty(PropertyName = "middle_name")]
        public string MiddleName { get; internal set; }

        /// <summary>
        /// Gets the person's full name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the person's name formatted to correctly handle Chinese, Japanese, or Korean ordering
        /// </summary>
        [JsonProperty(PropertyName = "name_format")]
        public string NameFormat { get; internal set; }

        /// <summary>
        /// Gets the person's payment price points
        /// </summary>
        [JsonProperty(PropertyName = "payment_pricepoints")]
        public string PaymentPricepoints { get; internal set; }

        /// <summary>
        /// Gets the platform test group
        /// </summary>
        [JsonProperty(PropertyName = "test_group")]
        public uint TestGroup { get; internal set; }

        /// <summary>
        /// Gets the person's political views
        /// </summary>
        [JsonProperty(PropertyName = "political")]
        public string Political { get; internal set; }

        /// <summary>
        /// Gets the person's relationship status
        /// </summary>
        [JsonProperty(PropertyName = "relationship_status")]
        public string RelationshipStatus { get; internal set; }

        /// <summary>
        /// Gets the person's religion
        /// </summary>
        [JsonProperty(PropertyName = "religion")]
        public string Religion { get; internal set; }

        /// <summary>
        /// Gets the security settings
        /// </summary>
        [JsonProperty(PropertyName = "personal")]
        public string SecuritySettings { get; internal set; }

        /// <summary>
        /// Gets the person's significant other
        /// </summary>
        [JsonProperty(PropertyName = "significant_other")]
        public string SignificantOther { get; internal set; }

        /// <summary>
        /// Gets the sports this person likes
        /// </summary>
        [JsonProperty(PropertyName = "sports")]
        public List<string> Sports { get; internal set; }

        /// <summary>
        /// Gets the person's favorite quotes
        /// </summary>
        [JsonProperty(PropertyName = "quotes")]
        public string Quotes { get; internal set; }

        /// <summary>
        /// Gets a string containing an anonymous, but unique identifier for the person. You can use this identifier with third parties
        /// </summary>
        [JsonProperty(PropertyName = "third_party_id")]
        public string ThirdPartyId { get; internal set; }

        /// <summary>
        /// Gets the person's current time zone offset from UTC
        /// </summary>
        [JsonProperty(PropertyName = "timezone")]
        public float Timezone { get; internal set; }

        /// <summary>
        /// Gets the token that is the same across a business's apps.
        /// Access to this token requires that the person be logged into your app.
        /// This token will change if the business owning the app changes
        /// </summary>
        [JsonProperty(PropertyName = "token_for_business")]
        public string TokenForBusiness { get; internal set; }

        /// <summary>
        /// Gets the updated time
        /// </summary>
        [JsonProperty(PropertyName = "updated_time")]
        public DateTime UpdatedTime { get; internal set; }

        /// <summary>
        /// Gets the time that the shared login needs to be upgraded to Business Manager by
        /// </summary>
        [JsonProperty(PropertyName = "shared_login_upgrade_required_by")]
        public DateTime SharedLoginUpgradeRequiredBy { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the account has been verified. This is distinct from the is_verified field.
        /// Someone is considered verified if they take any of the following actions:
        ///     Register for mobile
        ///     Confirm their account via SMS
        ///     Enter a valid credit card
        /// </summary>
        [JsonProperty(PropertyName = "verified")]
        public bool Verified { get; internal set; }

        /// <summary>
        /// Gets the video upload limits
        /// </summary>
        [JsonProperty(PropertyName = "video_upload_limits")]
        public string VideoUploadLimits { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the viewer send a gift to this person?
        /// </summary>
        [JsonProperty(PropertyName = "viewer_can_send_gift")]
        public bool ViewerCanSendGift { get; internal set; }

        /// <summary>
        /// Gets the person's website
        /// </summary>
        [JsonProperty(PropertyName = "website")]
        public string Website { get; internal set; }

        /// <summary>
        /// Gets the details of a person`s work experience
        /// </summary>
        [JsonProperty(PropertyName = "work")]
        public List<string> Work { get; internal set; }

        /// <summary>
        /// Gets the person's PGP public key
        /// </summary>
        [JsonProperty(PropertyName = "public_key")]
        public string PublicKey { get; internal set; }

        /// <summary>
        /// Gets the person's cover photo
        /// </summary>
        [JsonProperty(PropertyName = "cover")]
        public string Cover { get; internal set; }

        /// <summary>
        /// Gets a simpler profile
        /// </summary>
        public GenericUserProfile GenericUserProfile
        {
            get
            {
                GenericUserProfile authProfile = new GenericUserProfile
                {
                    AccountId = this.Id,
                };

                // Facebook passes names back as either first and last names (if access token is valid)
                // or as name (as a full name) if access token belongs to a different app.
                if (!string.IsNullOrEmpty(this.FirstName) && !string.IsNullOrEmpty(this.LastName))
                {
                    authProfile.FirstName = this.FirstName;
                    authProfile.LastName = this.LastName;
                }
                else if (!string.IsNullOrEmpty(this.Name))
                {
                    Tuple<string, string> firstAndLastNames = OAuthUtil.SingleName2FirstAndLastNames(this.Name);
                    authProfile.FirstName = firstAndLastNames.Item1;
                    authProfile.LastName = firstAndLastNames.Item2;
                }

                if (this.Email != null)
                {
                    authProfile.Emails = new List<string>() { this.Email };
                }

                return authProfile;
            }
        }
    }
}
