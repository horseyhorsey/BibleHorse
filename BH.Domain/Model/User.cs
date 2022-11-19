namespace BH.Domain.Model
{
    public class User : BHEntity
    {
        /// <summary>
        /// Default: Jehovah
        /// </summary>
        public string DevineName { get; set; }

        /// <summary>
        /// Default: Jesus
        /// </summary>
        public string GodsSon { get; set; }

        /// <summary>
        /// Default: Anointed One = Christ
        /// </summary>
        public string Anointed { get; set; }
    }
}
