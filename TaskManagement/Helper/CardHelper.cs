using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
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

namespace TaskManagement.Helper
{    
    public class CardHelper
    {
        private readonly IConfiguration _configuration;
        public CardHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
         
        public AdaptiveCard  TaskInformationCard()
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
                                        Text = "T12345: install security cameras in building 1",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                        Wrap = true
                                    }
                                },
                                Width = "stretch",
                            },
                            new AdaptiveColumn()
                            {
                                Items= new List<AdaptiveElement>()
                                {
                                    new AdaptiveImage()
                                    {
                                        Url = new Uri(_configuration["BaseUri"] + "/Images/high.png")
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
                                        Text ="Owner:",
                                        Size = AdaptiveTextSize.Medium
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text ="",
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
                                     new AdaptiveChoiceSetInput()
                                    {
                                        Choices =
                                        {
                                            new AdaptiveChoice()
                                            {
                                                Title = "Not started"
                                            },
                                            new AdaptiveChoice()
                                            {
                                                Title = "In Progress"
                                            },
                                            new AdaptiveChoice()
                                            {
                                                Title = "Blocked"
                                            },
                                            new AdaptiveChoice()
                                            {
                                                Title = "Complete"
                                            }
                                        },
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
                                        Text ="Depends on:",
                                        Size = AdaptiveTextSize.Medium,
                                        Color = AdaptiveTextColor.Accent
                                    }
                                },
                                Width = "stretch"
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveImage()
                                    {
                                        Url = new Uri(_configuration["BaseUri"] + "/Images/Plus.png")
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
                                        Color = AdaptiveTextColor.Attention
                                    }
                                },
                                Width = "stretch"
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveImage()
                                    {
                                        Url = new Uri(_configuration["BaseUri"] + "/Images/Plus.png")
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
                        Title = "View Details"
                    }
                }
            };

            return Card;
            //return new Attachment()
            //{
            //    ContentType = AdaptiveCard.ContentType,
            //    Content = Card
            //};
        }

    }
}
