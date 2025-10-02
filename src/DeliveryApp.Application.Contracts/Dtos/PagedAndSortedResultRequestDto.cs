using System.ComponentModel.DataAnnotations;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// This class can be used to get paged and sorted result.
    /// </summary>
    public class PagedAndSortedResultRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// Sorting information.
        /// Should include sorting field and optionally a direction (ASC or DESC)
        /// Can contain multiple fields separated by comma (,)
        /// </summary>
        public string Sorting { get; set; } = string.Empty;
    }

    /// <summary>
    /// This class can be used to get paged result.
    /// </summary>
    public class PagedResultRequestDto : LimitedResultRequestDto
    {
        /// <summary>
        /// Skip count (beginning of the page).
        /// </summary>
        public int SkipCount { get; set; }
    }

    /// <summary>
    /// This class can be used to get limited result.
    /// </summary>
    public class LimitedResultRequestDto
    {
        /// <summary>
        /// Maximum result count should be returned.
        /// This is generally used to limit result count on UI.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int MaxResultCount { get; set; } = 10;
    }
}

