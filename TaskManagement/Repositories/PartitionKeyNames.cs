// <copyright file="PartitionKeyNames.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TaskManagement.Repositories
{
    /// <summary>
    /// Partition key names used in the table storage.
    /// </summary>
    /// 

    public class PartitionKeyNames
    {
        /// <summary>
        /// Users data table partition key names.
        /// </summary>
        /// 
        public static class TaskActivityDataTable
        {
            /// <summary>
            /// Table name for Reflection data table
            /// </summary>
            public static readonly string TableName = "TaskActivityData";

            /// <summary>
            /// Reflection data partition key name.
            /// </summary>
            public static readonly string TaskActivityDataPartition = "TaskActivityData";
        }

        /// <summary>
        /// Users data table partition key names.
        /// </summary>
        /// 
        public static class TaskAttachementsDataTable
        {
            /// <summary>
            /// Table name for Reflection data table
            /// </summary>
            public static readonly string TableName = "TaskAttachementsData";

            /// <summary>
            /// Reflection data partition key name.
            /// </summary>
            public static readonly string TaskAttachementsDataPartition = "TaskAttachementsData";
        }

        /// <summary>
        /// Users data table partition key names.
        /// </summary>
        /// 
        public static class TaskDependencyDataTable
        {
            /// <summary>
            /// Table name for Reflection data table
            /// </summary>
            public static readonly string TableName = "TaskDependencyData";

            /// <summary>
            /// Reflection data partition key name.
            /// </summary>
            public static readonly string TaskDependencyDataPartition = "TaskDependencyData";
        }

        /// <summary>
        /// Users data table partition key names.
        /// </summary>
        /// 
        public static class TaskDetailsDataTable
        {
            /// <summary>
            /// Table name for user data table
            /// </summary>
            public static readonly string TableName = "TaskDetailsData";

            /// <summary>
            /// Users data partition key name.
            /// </summary>
            public static readonly string TaskDataPartition = "TaskDetailsData";
        }


        /// <summary>
        /// Users data table partition key names.
        /// </summary>
        /// 
        public static class TaskSubscribersDataTable
        {
            /// <summary>
            /// Table name for Questions data table
            /// </summary>
            public static readonly string TableName = "TaskSubscribersData";

            /// <summary>
            /// Questions data partition key name.
            /// </summary>
            public static readonly string TaskSubscribersDataPartition = "TaskSubscribersData";
            
        }     
    }
}


