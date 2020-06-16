using AdaptiveCards;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using TaskManagement.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using TaskManagement.Repositories.TaskDetailsData;

namespace TaskManagement.Helper
{
    public class CardHelper
    {
        private readonly IConfiguration _configuration;
        public CardHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AdaptiveCard TaskInformationCard(TaskInfo taskInfo)
        {
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = taskInfo.taskNumber + " : " + (taskInfo.title.Length > 30 ? taskInfo.title.Substring(0, 30) : taskInfo.title),
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                        Wrap = true
                                    }
                                },
                                Width = "auto",
                            },
                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveImage()
                                    {
                                        Url = new Uri(_configuration["BaseUri"] + "/Images/" + taskInfo.priority + ".png")
                                    }
                                },
                                Width = "stretch"
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text ="Owner:",
                                        Size = AdaptiveTextSize.Medium
                                    }
                                },
                                 Width = "auto"
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = taskInfo.taskAssignedTo,
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text ="Progress:",
                                        Size = AdaptiveTextSize.Medium
                                    }
                                },
                                Width = "auto"
                            },
                             new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = taskInfo.status,
                                    }
                                }
                            }

                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text ="Depends on: ",
                                        Size = AdaptiveTextSize.Medium,
                                    }
                                },
                                Width = "auto"
                            },
                             new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = taskInfo.akkTaskIDs == null ? " " : string.Join(", ", taskInfo.akkTaskIDs),
                                        Wrap = true
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveImage()
                                    {
                                        Url = new Uri(_configuration["BaseUri"] + "/Images/AddDep.png"),
                                        SelectAction = new AdaptiveSubmitAction()
                                        {
                                            Type = "Action.Submit",
                                            Title = "Depends On",
                                            DataJson=@"{'type':'task/fetch','taskId':'" + taskInfo.taskID +"'}",
                                            Data =
                                            new TaskModuleActionHelper.AdaptiveCardValue<TaskModuleActionDetails>()
                                            {
                                                Data = new TaskModuleActionDetails()
                                                {
                                                    type ="task/fetch",
                                                    URL =_configuration["BaseUri"] + "/createNewTask/",
                                                    CreateType ="Depends on",
                                                    TaskId = taskInfo.taskNumber,
                                                }
                                            }
                                        },
                                    }
                                },
                                Width = "auto"
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text ="Blocks:",
                                        Size = AdaptiveTextSize.Medium,
                                        Color = taskInfo.blocks == null ? AdaptiveTextColor.Default : AdaptiveTextColor.Attention
                                    }
                                },
                                Width = "auto"
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = string.Join(", ", taskInfo.blocks),
                                        Wrap = true
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveImage()
                                    {
                                        Url = new Uri(_configuration["BaseUri"] + "/Images/AddDep.png"),
                                        SelectAction = new AdaptiveSubmitAction()
                                        {
                                            Type = "Action.Submit",
                                            Title = "Depends On",
                                            DataJson=@"{'type':'task/fetch','taskId':'" + taskInfo.taskID +"'}",
                                            Data =
                                            new TaskModuleActionHelper.AdaptiveCardValue<TaskModuleActionDetails>()
                                            {
                                                Data = new TaskModuleActionDetails()
                                                {
                                                    type ="task/fetch",
                                                    URL =_configuration["BaseUri"] + "/createNewTask/",
                                                    CreateType ="Blocks",
                                                    TaskId = taskInfo.taskNumber,
                                                }
                                            }
                                        },

                                    }
                                },
                                Width = "auto"
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>()
                {
                   new AdaptiveSubmitAction()
                    {
                        Type = "Action.Submit",
                        Title = "View Details",
                        DataJson=@"{'type':'task/fetch','taskId':'" + taskInfo.taskID +"' }",
                        Data =
                        new TaskModuleActionHelper.AdaptiveCardValue<TaskModuleActionDetails>()
                        {
                            Data = new TaskModuleActionDetails()
                            {
                                type ="task/fetch",
                                URL =_configuration["BaseUri"] + "/EditTask/" + taskInfo.taskID
                            }
                        }
                    },
                }
            };

            return Card;
        }


    }
}