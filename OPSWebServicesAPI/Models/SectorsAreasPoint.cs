using System.ComponentModel;

namespace OPSWebServicesAPI.Models
{
    public class SectorsAreasPoint
    {
        /// <summary>
        /// sector area point id
        /// </summary>
        [DisplayName("mga_id")]
        public int mga_id { get; set; }
        /// <summary>
        /// sector area zone id
        /// </summary>
        [DisplayName("mga_zone_id")]
        public int mga_zone_id { get; set; }
        /// <summary>
        /// sector area sector id
        /// </summary>
        [DisplayName("mga_sector_id")]
        public int mga_sector_id { get; set; }
        /// <summary>
        /// area number
        /// </summary>
        [DisplayName("mga_area_id")]
        public int mga_area_id { get; set; }
        /// <summary>
        /// point number
        /// </summary>
        [DisplayName("mga_point_number")]
        public int mga_point_number { get; set; }
        /// <summary>
        /// sector area point coordinates
        /// </summary>
        [DisplayName("mga_coordinates")]
        public string mga_coordinates { get; set; }

    }
}