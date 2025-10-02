using System.Collections.Generic;

namespace DeliveryApp.Application.Contracts.Dtos
{
    /// <summary>
    /// This class can be used to return a paged result.
    /// </summary>
    /// <typeparam name="T">Type of the items</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Total count of Items.
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Items of this page.
        /// </summary>
        public IReadOnlyList<T> Items { get; set; }

        /// <summary>
        /// Creates a new <see cref="PagedResultDto{T}"/> object.
        /// </summary>
        public PagedResultDto()
        {
        }

        /// <summary>
        /// Creates a new <see cref="PagedResultDto{T}"/> object.
        /// </summary>
        /// <param name="totalCount">Total count of Items</param>
        /// <param name="items">Items of this page</param>
        public PagedResultDto(long totalCount, IReadOnlyList<T> items)
        {
            TotalCount = totalCount;
            Items = items;
        }
    }
}

