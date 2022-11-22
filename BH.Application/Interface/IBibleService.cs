namespace BH.Application.Interface
{
    public interface IBibleService
    {
        /// <summary>
        /// Returns a comma seperated list of bible books for a given translation
        /// </summary>
        /// <param name="translationId"></param>
        /// <returns></returns>
        Task<string> GetAllBooks(long translationId = 1);

        /// <summary>
        /// Gets information if any for a given book title. html formatted
        /// </summary>
        /// <param name="bookTitle"></param>
        /// <param name="translationId"></param>
        /// <returns></returns>
        Task<string> GetBookInformation(string bookTitle, long translationId = 1);

        /// <summary>
        /// Gets verses from a given query.
        /// </summary>
        /// <param name="userId">User id is used to replace with customer names and settings</param>
        /// <param name="query">`rev,9:11` or `rev,9:11-13` for range</param>
        /// <param name="translationId"></param>
        /// <returns></returns>
        Task<string> GetVerses(long? userId, string query, long translationId = 1);
    }
}
