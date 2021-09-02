using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OPSWebServicesAPI.Models
{
    /// <summary>
    /// class to request report with user operations for dates
    /// </summary>
    public class UserReportQuery
    {
        /// <summary>
        /// (*) id del municipio
        /// </summary>
        [DisplayName("contid")]
        [Required]
        public int contractId { get; set; }
        /// <summary>
        /// (*) Authentication token
        /// </summary>
        [DisplayName("mui")]
        [Required]
        public string authorizationToken { get; set; }
        /// <summary>
        /// (*) Report start date (Format: hh24missddMMYY)
        /// </summary>
        [DisplayName("d1")]
        [Required]
        public string dateStart { get; set; }
        /// <summary>
        /// (*) Report end date (Format: hh24missddMMYY)
        /// </summary>
        [DisplayName("d2")]
        [Required]
        public string dateEnd { get; set; }
        /// <summary>
        /// (*) User email
        /// </summary>
        [DisplayName("mail")]
        [Required]
        public string mail { get; set; }
        /// <summary>
        /// (*) Report Format (1: PDF, something else?)
        /// </summary>
        [DisplayName("rfmt")]
        [Required]
        public int reportFormat { get; set; }

    }
}