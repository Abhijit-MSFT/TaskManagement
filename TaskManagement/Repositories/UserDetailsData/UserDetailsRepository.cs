using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Repositories.UserDetailsData
{
    public class UserDetailsRepository : BaseRepository<UserDetailsEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="isFromAzureFunction">Flag to show if created from Azure Function.</param>
        public UserDetailsRepository(IConfiguration configuration, bool isFromAzureFunction = false)
            : base(
                configuration,
                PartitionKeyNames.UserDetailsDataTable.TableName,
                PartitionKeyNames.UserDetailsDataTable.UserDetailsDataPartition,
                isFromAzureFunction)
        {

        }

        public async Task<UserDetailsEntity> GeUserDetails(string emailId)
        {
            var allRows = await this.GetAllAsync(PartitionKeyNames.UserDetailsDataTable.TableName);
            UserDetailsEntity userDetailsEntity = allRows.Where(c => c.EmailId == emailId).FirstOrDefault();
            return userDetailsEntity;
        }
    }
}
