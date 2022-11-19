namespace BH.Domain.Sites
{
    public static class SiteHelper
    {
        const string SITE_2001 = "https://2001translation.org";
        const string SITE_BIBLE_HUB = "https://biblehub.com/";
        const string SITE_BIBLE_GATEWAY = "https://www.biblegateway.com";

        /// <summary>
        /// Append Chapter:Verse #_8:10 to end or #_8:10-15
        /// </summary>
        /// <param name="bookTitle"></param>
        /// <returns></returns>
        public static string GetSiteLink_2001(string bookTitle)
        {
            return $"{SITE_2001}/read/{bookTitle.ToLower().Replace(" ", "")}";
        }

        /// <summary>
        /// Append Chapter-Verse to end and .htm like 8-10.htm
        /// </summary>
        /// <param name="bookTitle"></param>
        /// <returns></returns>
        public static string GetSiteLink_BibleHub(string bookTitle)
        {
            return $"{SITE_BIBLE_HUB}/{bookTitle.ToLower().Replace(" ", "")}/";
        }

        /// <summary>
        /// Append Chapter:Verse to end
        /// </summary>
        /// <param name="bookTitle"></param>
        /// <returns></returns>
        public static string GetSiteLink_BibleGateway(string bookTitle)
        {
            return $"{SITE_BIBLE_GATEWAY}/verse/en/{bookTitle.ToLower().Replace(" ", "")}/";
        }
    }
}
