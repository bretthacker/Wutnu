using System;
using System.Security.Cryptography;
using Wutnu.Data;
using System.Web;
using System.Linq;
using Wutnu.Common;
using System.Data.Entity;

namespace Infrastructure
{
    /// <summary>
    /// Range of utility methods
    /// </summary>
    public static class ShortUrlUtils
    {
        private static string shorturl_chars_lcase = "abcdefgijkmnopqrstwxyz";
        private static string shorturl_chars_ucase = "ABCDEFGHJKLMNPQRSTWXYZ";
        private static string shorturl_chars_numeric = "23456789";

        public static string UniqueShortUrl(WutNuContext models)
        {
            string short_url = RandomCharacters();
            //todo: this is a little dangerous recursion but how many times can the random number really duplicate? #buyLotto
            return (IsUnique(short_url, models)) ? short_url : UniqueShortUrl(models);
        }

        private static bool IsUnique(string url, WutNuContext models)
        {
            var count= models.WutLinks.Count(u => u.ShortUrl == url);
            return (count == 0);
        }
        public static string PublicShortUrl(string shortUrl)
        {
            //todo: might have to parameterize the protocol in case auth requires SSL
            return string.Format("https://{0}/{1}", Settings.DomainName, shortUrl);
        }

        public static string InternalShortUrl(string shortUrl)
        {
            //todo: meh
            return shortUrl.Replace("https://" + Settings.DomainName + "/", "");
        }

        public static WutLink AddUrlToDatabase(WutLink oUrl, WutNuContext models)
        {
            oUrl.ShortUrl = UniqueShortUrl(models);

            models.WutLinks.Add(oUrl);
            models.SaveChanges();
            return oUrl;
        }
        public static WutLinkPoco UpdateUrl(WutLinkPoco oUrl, WutNuContext models, int UserId)
        {
            try
            {
                var item = models.WutLinks.Include("UserAssignments").Single(u => u.ShortUrl == oUrl.ShortUrl);
                if (item.UserId != UserId)
                {
                    throw new UnauthorizedAccessException("You attempted to save a URL that is owned by someone else.");
                }

                item.Comments = oUrl.Comments;
                item.IsProtected = oUrl.IsProtected;
                item.RealUrl = oUrl.RealUrl;
                item.ShortUrl = oUrl.ShortUrl;

                var hasEmails = (oUrl.UserEmails.Trim().Length > 0);

                if (item.UserAssignments.Any())
                {
                    //remove existing user assignments
                    models.UserAssignments.RemoveRange(item.UserAssignments);
                }
                int? assignmentId = null;

                if (hasEmails)
                {
                    var emails = oUrl.UserEmails.Split(',');
                    foreach(var email in emails)
                    {
                        //var assignedUser = models.Users.SingleOrDefault(u => u.PrimaryEmail == email);
                        
                        //assignmentId = (assignedUser != null) ? assignedUser.UserId : (int?)null;

                        models.UserAssignments.Add(new UserAssignment
                        {
                            UserEmail = email.Trim(),
                            WutLinkId = item.WutLinkId,
                            UserId = null
                        });
                    }
                }
                models.SaveChanges();

                return oUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving Url", ex);
            }
        }
        public static void DeleteUrl(WutLinkPoco oUrl, WutNuContext models)
        {
            var item = models.WutLinks.Single(u => u.ShortUrl == oUrl.ShortUrl);
            models.WutLinks.Remove(item);
            models.SaveChanges();
        }

        public static WutLinkPoco RetrieveUrlFromDatabase(string internalUrl, WutNuContext models, int? userId=null, string authEmail=null)
        {
            //todo: need to ensure that this search is case-sensitive - check code page on SQL DB
            var item = models.WutLinks.Include("UserAssignments").SingleOrDefault(u => u.ShortUrl == internalUrl);
            if (item == null)
                return null;

            UserAssignment user = null;

            if (item.IsProtected)
            {
                user = item.UserAssignments.SingleOrDefault(u => u.UserEmail == authEmail);

                if (user == null)
                {
                    //user is authenticated but not authorized for this file
                    return null;
                }

                //feels like I should do this somewhere else but this will work for now
                //(refreshing the UserAssignments table with UserID - assigner only had email)
                if (userId != null)
                {
                    //user is authenticated but not authorized for this file
                    user.UserId = userId;
                    models.SaveChanges();
                }
            }

            var res = WutLinkPoco.GetPocoFromObject(item);
            if (user != null)
                res.UserAuthenticated = true;

            return res;
        }

        /// <summary>
        /// RandomCharacter routine pulled from shorturl-dotnet on github. May be a better way.
        /// </summary>
        /// <returns></returns>
        private static string RandomCharacters()
        {
            // Create a local array containing supported short-url characters
            // grouped by types.
            char[][] charGroups = new char[][] 
            {
                shorturl_chars_lcase.ToCharArray(),
                shorturl_chars_ucase.ToCharArray(),
                shorturl_chars_numeric.ToCharArray()
            };

            // Use this array to track the number of unused characters in each
            // character group.
            int[] charsLeftInGroup = new int[charGroups.Length];

            // Initially, all characters in each group are not used.
            for (int i = 0; i < charsLeftInGroup.Length; i++)
                charsLeftInGroup[i] = charGroups[i].Length;

            // Use this array to track (iterate through) unused character groups.
            int[] leftGroupsOrder = new int[charGroups.Length];

            // Initially, all character groups are not used.
            for (int i = 0; i < leftGroupsOrder.Length; i++)
                leftGroupsOrder[i] = i;

            // Because we cannot use the default randomizer, which is based on the
            // current time (it will produce the same "random" number within a
            // second), we will use a random number generator to seed the
            // randomizer.

            // Use a 4-byte array to fill it with random bytes and convert it then
            // to an integer value.
            byte[] randomBytes = new byte[4];

            // Generate 4 random bytes.
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            // Convert 4 bytes into a 32-bit integer value.
            int seed = (randomBytes[0] & 0x7f) << 24 |
                        randomBytes[1] << 16 |
                        randomBytes[2] << 8 |
                        randomBytes[3];

            // Now, this is real randomization.
            Random random = new Random(seed);

            // This array will hold short-url characters.
            char[] short_url = null;

            // Allocate appropriate memory for the short-url.
            short_url = new char[random.Next(5, 5)];

            // Index of the next character to be added to short-url.
            int nextCharIdx;

            // Index of the next character group to be processed.
            int nextGroupIdx;

            // Index which will be used to track not processed character groups.
            int nextLeftGroupsOrderIdx;

            // Index of the last non-processed character in a group.
            int lastCharIdx;

            // Index of the last non-processed group.
            int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

            // Generate short-url characters one at a time.
            for (int i = 0; i < short_url.Length; i++)
            {
                // If only one character group remained unprocessed, process it;
                // otherwise, pick a random character group from the unprocessed
                // group list. To allow a special character to appear in the
                // first position, increment the second parameter of the Next
                // function call by one, i.e. lastLeftGroupsOrderIdx + 1.
                if (lastLeftGroupsOrderIdx == 0)
                    nextLeftGroupsOrderIdx = 0;
                else
                    nextLeftGroupsOrderIdx = random.Next(0,
                                                         lastLeftGroupsOrderIdx);

                // Get the actual index of the character group, from which we will
                // pick the next character.
                nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

                // Get the index of the last unprocessed characters in this group.
                lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

                // If only one unprocessed character is left, pick it; otherwise,
                // get a random character from the unused character list.
                if (lastCharIdx == 0)
                    nextCharIdx = 0;
                else
                    nextCharIdx = random.Next(0, lastCharIdx + 1);

                // Add this character to the short-url.
                short_url[i] = charGroups[nextGroupIdx][nextCharIdx];

                // If we processed the last character in this group, start over.
                if (lastCharIdx == 0)
                    charsLeftInGroup[nextGroupIdx] =
                                              charGroups[nextGroupIdx].Length;
                // There are more unprocessed characters left.
                else
                {
                    // Swap processed character with the last unprocessed character
                    // so that we don't pick it until we process all characters in
                    // this group.
                    if (lastCharIdx != nextCharIdx)
                    {
                        char temp = charGroups[nextGroupIdx][lastCharIdx];
                        charGroups[nextGroupIdx][lastCharIdx] =
                                    charGroups[nextGroupIdx][nextCharIdx];
                        charGroups[nextGroupIdx][nextCharIdx] = temp;
                    }
                    // Decrement the number of unprocessed characters in
                    // this group.
                    charsLeftInGroup[nextGroupIdx]--;
                }

                // If we processed the last group, start all over.
                if (lastLeftGroupsOrderIdx == 0)
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                // There are more unprocessed groups left.
                else
                {
                    // Swap processed group with the last unprocessed group
                    // so that we don't pick it until we process all groups.
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                    leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    // Decrement the number of unprocessed groups.
                    lastLeftGroupsOrderIdx--;
                }
            }

            // Convert password characters into a string and return the result.
            return new string(short_url);
        }
    }
}